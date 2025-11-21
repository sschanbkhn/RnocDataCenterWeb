using CsvHelper;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using NpgsqlTypes;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.PRBsLoadCell
{
    [ApiController]
    [Route("api/prbs-load/dashboard")]
    [Produces("application/json")]
    public class PRBsLoadCellDashboardApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        // Constructor - Inject IConfiguration


        // Constructor - SỬA TÊN CHO KHỚP
        public PRBsLoadCellDashboardApiController(IConfiguration configuration)  // ← SỬA ĐÂY
        {
            _configuration = configuration;
            // _connectionString = configuration.GetConnectionString("DefaultConnection"); // ← TÊN CONNECTION STRING
            _connectionString = _configuration.GetConnectionString("InformationProductionConnection");
        }
        // ket thuc public PRBsLoadCellDashboardApiController(IConfiguration configuration)  // ← SỬA ĐÂY
        //========================================================================




        //========================================================================
        // API 1: GET SUMMARY
        //========================================================================

        [HttpGet("summary/{date}")]
        public async Task<IActionResult> GetSummary(string date)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Parse date
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                {
                    return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });
                }
                //========================================================================

                var yesterday = selectedDate.AddDays(-1);

                // Get all counts
                //========================================================================

                var totalCells = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatarawtotalcellslogs WHERE period_start_time::date = @date",
                    selectedDate);
                //========================================================================

                var prbsCongestedCells = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive WHERE period_start_time::date = @date",
                    selectedDate);
                //========================================================================

                var processingCells = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataprocessinglogs WHERE period_start_time::date = @date",
                    selectedDate);
                //========================================================================

                var blacklistCells = await GetCount(connection,
                    // "SELECT COUNT(*) FROM objtablekpiprbsloadcellsdatablacklist WHERE active = true AND period_start_time::date >= @date::date - interval '3 days' AND period_start_time::date <= @date::date",
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatablacklist WHERE active = true AND period_start_time::date = @date",
                    selectedDate);
                //========================================================================

                var pendingCells = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataexisting WHERE period_start_time::date = @date",
                    selectedDate);
                //========================================================================

                var successCells = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataresolvedlogs WHERE period_start_time::date = @date",
                    selectedDate);
                //========================================================================



                var newCells = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataappnew WHERE period_start_time::date = @date",
                    selectedDate);
                //========================================================================

                // Yesterday counts
                //========================================================================

                var yesterdayPrbsCongested = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive WHERE period_start_time::date = @date",
                    yesterday);
                //========================================================================

                var yesterdayProcessing = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataprocessinglogs WHERE period_start_time::date = @date",
                    yesterday);
                //========================================================================

                var yesterdayPending = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataexisting WHERE period_start_time::date = @date",
                    yesterday);
                //========================================================================

                var yesterdaySuccess = await GetCount(connection,
                    "SELECT COUNT(*) FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataresolvedlogs WHERE period_start_time::date = @date",
                    yesterday);
                //========================================================================

                // Return response
                return Ok(new
                {
                    date = date,
                    summary = new
                    {
                        totalCells,
                        prbsCongestedCells,
                        processingCells,
                        blacklistCells,
                        pendingCells,
                        successCells,
                        newCells,
                        yesterday = new
                        {
                            prbsCongestedCells = yesterdayPrbsCongested,
                            processingCells = yesterdayProcessing,
                            pendingCells = yesterdayPending,
                            successCells = yesterdaySuccess
                        }
                        //========================================================================
                    }
                    //========================================================================
                });
                //========================================================================
            }
            //========================================================================

            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message
                });
                //========================================================================
            }
            //========================================================================
        }
        //========================================================================

        // Helper method to get count
        private async Task<int> GetCount(NpgsqlConnection connection, string query, DateTime date)
        {
            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@date", date);
            //========================================================================

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        //========================================================================


        // phan API de hien detail

        //========================================================================
        // API 2: GET CELL DETAILS
        //========================================================================
        [HttpGet("cells-detail/{type}/{date}")]
        public async Task<IActionResult> GetCellDetails(string type, string date)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Parse date
                if (!DateTime.TryParse(date, out DateTime selectedDate))
                {
                    return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });
                }
                //========================================================================

                // Get table name and query based on type
                string tableName;
                string query;

                switch (type.ToLower())
                {
                    case "total":
                        tableName = "system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatarawtotalcellslogs";
                        query = BuildSelectQueryForTotal(tableName, "period_start_time::date = @date");
                        break;
                    //========================================================================

                    case "congested":
                        tableName = "system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive";
                        query = BuildSelectQuery(tableName, "period_start_time::date = @date");
                        break;

                    case "processing":
                        tableName = "system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataprocessinglogs";
                        query = BuildSelectQuery(tableName, "period_start_time::date = @date");
                        break;

                    case "blacklist":
                        tableName = "system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatablacklist";
                        query = BuildSelectQuery(tableName,
                            "active = true AND period_start_time::date >= @date::date - interval '3 days' AND period_start_time::date <= @date::date");
                        break;

                    case "pending":
                        tableName = "system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataexisting";
                        query = BuildSelectQuery(tableName, "period_start_time::date = @date");
                        break;

                    case "success":
                        tableName = "system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataresolvedlogs";
                        query = BuildSelectQuery(tableName, "period_start_time::date = @date");
                        break;

                    case "new":
                        tableName = "system_nsn_prbsloadcell.objtablekpiprbsloadcellsdataappnew";
                        query = BuildSelectQuery(tableName, "period_start_time::date = @date");
                        break;

                    default:
                        return BadRequest(new
                        {
                            message = "Invalid type. Use: total, congested, processing, blacklist, pending, success, new"
                        });
                }
                //========================================================================

                // Execute query
                var data = new List<object>();
                //========================================================================

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@date", selectedDate);
                //========================================================================

                using var reader = await cmd.ExecuteReaderAsync();
                //========================================================================

                while (await reader.ReadAsync())
                {
                    if (type.ToLower() == "total")
                    {
                        data.Add(new
                        {
                            rawid = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            period_start_time = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1),
                            mrbts_name = reader.IsDBNull(2) ? null : reader.GetString(2),
                            lnbts_name = reader.IsDBNull(3) ? null : reader.GetString(3),
                            lncel_name = reader.IsDBNull(4) ? null : reader.GetString(4),
                            dn_mrbts_site = reader.IsDBNull(5) ? null : reader.GetString(5),
                            pdcp_sdu_volume_dl = reader.IsDBNull(6) ? (decimal?)null : reader.GetDecimal(6),
                            pdcp_sdu_volume_ul = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                            eutran_avg_prb_usage_dl = reader.IsDBNull(8) ? (decimal?)null : reader.GetDecimal(8),
                            max_pdcp_thr_dl = reader.IsDBNull(9) ? (decimal?)null : reader.GetDecimal(9),
                            max_pdcp_thr_ul = reader.IsDBNull(10) ? (decimal?)null : reader.GetDecimal(10),
                            created_at = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11)


                        });
                        //========================================================================
                    }
                    // ket thuc if (type.ToLower() == "total")
                    else
                    {
                        // Detail tables - 17 columns
                        data.Add(new
                        {
                            rawid = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            period_start_time = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1),
                            mrbts_name = reader.IsDBNull(2) ? null : reader.GetString(2),
                            lnbts_name = reader.IsDBNull(3) ? null : reader.GetString(3),
                            lncel_name = reader.IsDBNull(4) ? null : reader.GetString(4),
                            dn_mrbts_site = reader.IsDBNull(5) ? null : reader.GetString(5),
                            pdcp_sdu_volume_dl = reader.IsDBNull(6) ? (decimal?)null : reader.GetDecimal(6),
                            pdcp_sdu_volume_ul = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                            eutran_avg_prb_usage_dl = reader.IsDBNull(8) ? (decimal?)null : reader.GetDecimal(8),
                            max_pdcp_thr_dl = reader.IsDBNull(9) ? (decimal?)null : reader.GetDecimal(9),
                            max_pdcp_thr_ul = reader.IsDBNull(10) ? (decimal?)null : reader.GetDecimal(10),
                            affected_days = reader.IsDBNull(11) ? (int?)null : reader.GetInt32(11),
                            total_occurrences = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                            avg_prb = reader.IsDBNull(13) ? (decimal?)null : reader.GetDecimal(13),
                            max_prb = reader.IsDBNull(14) ? (decimal?)null : reader.GetDecimal(14),
                            daily_detail = reader.IsDBNull(15) ? null : reader.GetString(15),
                            created_at = reader.IsDBNull(16) ? (DateTime?)null : reader.GetDateTime(16)
                        });
                    }
                    // ket thuc else
                    //========================================================================
                }
                // ket thuc while (await reader.ReadAsync())
                //========================================================================

                // Return response
                return Ok(new
                {
                    type,
                    date,
                    totalCount = data.Count,
                    data
                });
                //========================================================================

            }
            //========================================================================
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message
                });
            }
            //========================================================================
        }

        // Helper method to build SELECT query
        private string BuildSelectQuery(string tableName, string whereClause)
        {
            return $@"
                SELECT 
                    rawid,
                    period_start_time,
                    mrbts_name,
                    lnbts_name,
                    lncel_name,
                    dn_mrbts_site,
                    pdcp_sdu_volume_dl,
                    pdcp_sdu_volume_ul,
                    eutran_avg_prb_usage_dl,
                    max_pdcp_thr_dl,
                    max_pdcp_thr_ul,
                    affected_days,
                    total_occurrences,
                    avg_prb,
                    max_prb,
                    daily_detail,
                    created_at
                FROM {tableName}
                WHERE {whereClause}
                ORDER BY period_start_time DESC
                LIMIT 1000
            ";
            //========================================================================
        }
        // ket thuc private string BuildSelectQuery(string tableName, string whereClause)
        //========================================================================


        private string BuildSelectQueryForTotal(string tableName, string whereClause)
        {
            return $@"
                SELECT 
                    rawid,
                    period_start_time,
                    mrbts_name,
                    lnbts_name,
                    lncel_name,
                    dn_mrbts_site,
                    pdcp_sdu_volume_dl,
                    pdcp_sdu_volume_ul,
                    eutran_avg_prb_usage_dl,
                    max_pdcp_thr_dl,
                    max_pdcp_thr_ul,
                    created_at
                FROM {tableName}
                WHERE {whereClause}
                ORDER BY period_start_time DESC
                LIMIT 50000
            ";
        }
        //========================================================================



        [HttpGet("kpi-trend-prbs")]
        public async Task<IActionResult> GetKpiTrend(
    [FromQuery] string? dn_mrbts_site,
    // [FromQuery] string? lncell_name,
    // [FromQuery] string start_date,
    // [FromQuery] string end_date)
            [FromQuery]
        string selected_date)  // ← Chỉ cần 1 ngày
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
            SELECT 
                TO_CHAR(period_start_time, 'YYYY-MM-DD HH24:MI') as time,
                eutran_avg_prb_usage_dl as dl_prb_usage
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatalogs
            WHERE (

                (@dn_mrbts_site IS NULL OR dn_mrbts_site = @dn_mrbts_site)
            )

                AND period_start_time >= (@selected_date::date - interval '21 days')
            AND period_start_time <= (@selected_date::date + interval '1 day')
            ORDER BY period_start_time ASC
        ";

                using var cmd = new NpgsqlCommand(query, connection);
                // cmd.Parameters.AddWithValue("lncell_name", (object?)lncell_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("dn_mrbts_site", (object?)dn_mrbts_site ?? DBNull.Value);
                cmd.Parameters.AddWithValue("selected_date", selected_date);
                //========================================================================

                var results = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                //========================================================================

                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        time = reader.GetString(0),
                        dl_prb_usage = reader.IsDBNull(1) ? 0 : reader.GetDouble(1)
                    });
                }
                //========================================================================

                return Ok(results);
            }
            //========================================================================

            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
            //========================================================================
        }
        //========================================================================

        /*

        [HttpGet("kpi-trend-prbs-new")]
        public async Task<IActionResult> GetKpiTrend(
    [FromQuery] string? dn_mrbts_site,
    [FromQuery] string? lncell_name,
    [FromQuery] string selected_date)
        {
            try
            {
                // Validate
                if (string.IsNullOrEmpty(dn_mrbts_site) && string.IsNullOrEmpty(lncell_name))
                {
                    return BadRequest(new { error = "Phải cung cấp dn_mrbts_site hoặc lncell_name" });
                }

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
            SELECT 
                TO_CHAR(period_start_time, 'YYYY-MM-DD HH24:MI') as time,
                eutran_avg_prb_usage_dl as dl_prb_usage
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatalogs
            WHERE (
                (@lncell_name::text IS NULL OR lncel_name = @lncell_name::text)
                OR 
                (@dn_mrbts_site::text IS NULL OR dn_mrbts_site = @dn_mrbts_site::text)
            )
            AND period_start_time >= (@selected_date::date - interval '21 days')
            AND period_start_time <= (@selected_date::date + interval '1 day')
            ORDER BY period_start_time ASC
        ";

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("lncell_name", NpgsqlDbType.Text, (object?)lncell_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("dn_mrbts_site", NpgsqlDbType.Text, (object?)dn_mrbts_site ?? DBNull.Value);
                cmd.Parameters.AddWithValue("selected_date", NpgsqlDbType.Date, DateTime.Parse(selected_date));

                var results = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        time = reader.GetString(0),
                        dl_prb_usage = reader.IsDBNull(1) ? 0 : reader.GetDouble(1)
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        */

        //========================================================================
        //************************************************************************
        //========================================================================
        // phan xu ly hien thi bieu do trend, zone 2
        //========================================================================
        //************************************************************************
        //========================================================================
        //========================================================================
        // ... Các API khác sẽ thêm ở đây
        //========================================================================

        // Đặt trong Controller, trước các methods
        private class TrendDataDto
        {
            public string Date { get; set; }
            public int CongestedCells { get; set; }
            public int ProcessingCells { get; set; }
            public int ExistingCells { get; set; }
            public int BlacklistCells { get; set; }
            public int ResolvedCells { get; set; }
            public int TotalCells { get; set; }

            // Auto-calculate
            /*
            public decimal SuccessRate => ProcessingCells > 0
                ? Math.Round(ResolvedCells * 100m / ProcessingCells, 1)
                : 0;
            */
            // ← Đổi từ auto-calculate thành settable
            public decimal SuccessRate { get; set; }
        }
        //========================================================================


        //========================================================================
        // API #1: GET TREND 14 NGÀY - TOÀN MẠNG
        //========================================================================
        /// <summary>
        /// Lấy trend data 14 ngày cho toàn mạng (tất cả tỉnh)
        /// </summary>
        /// <param name="endDate">Ngày kết thúc (format: YYYY-MM-DD). Mặc định: hôm nay</param>
        /// <returns>List trend data 14 ngày</returns>
        [HttpGet("trend-network")]
        public async Task<IActionResult> GetNetworkTrend([FromQuery] string? endDate = null)
        {
            try
            {
                // 1. Parse & validate date
                DateTime parsedEndDate;
                if (string.IsNullOrEmpty(endDate))
                {
                    parsedEndDate = DateTime.Today;
                }
                //========================================================================

                else
                {
                    if (!DateTime.TryParse(endDate, out parsedEndDate))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid date format. Use YYYY-MM-DD"
                        });
                    }
                }
                //========================================================================

                // 2. Calculate startDate (endDate - 13 days)
                // DateTime startDate = parsedEndDate.AddDays(-13);
                DateTime startDate = parsedEndDate.AddDays(-14);
                // tinh ngay dau tien, 14 ngay thi can lay ngay 15 de tinh
                //========================================================================

                // 3. SQL Query
                var results = new List<TrendDataDto>();

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
            WITH date_range AS (
                SELECT DISTINCT period_start_time::date as date
                FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive
                WHERE period_start_time::date BETWEEN @startDate AND @endDate
            )
            SELECT 
                dr.date,
                
                -- 1. Congested: tất cả cells hôm đó
                (SELECT COUNT(*) 
                 FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                 WHERE a.period_start_time::date = dr.date) as congested_cells,
                
                -- 2. Processing: Congested - Blacklist
                (SELECT COUNT(*) 
                 FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                 WHERE a.period_start_time::date = dr.date) - 
                COALESCE((SELECT COUNT(*)
                 FROM (
                     SELECT lncel_name
                     FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                     WHERE a.period_start_time::date IN (dr.date, dr.date - 1, dr.date - 2)
                     GROUP BY lncel_name
                     HAVING COUNT(DISTINCT a.period_start_time::date) = 3
                 ) bl), 0) as processing_cells,
                
                -- 3. Existing: có CẢ hôm nay VÀ hôm qua
                COALESCE((SELECT COUNT(*)
                 FROM (
                     SELECT lncel_name
                     FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                     WHERE a.period_start_time::date = dr.date
                     INTERSECT
                     SELECT lncel_name
                     FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                     WHERE a.period_start_time::date = dr.date - 1
                 ) ex), 0) as existing_cells,
                
                -- 4. Blacklist: nghẽn 3 ngày liên tục
                COALESCE((SELECT COUNT(*)
                 FROM (
                     SELECT lncel_name
                     FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                     WHERE a.period_start_time::date IN (dr.date, dr.date - 1, dr.date - 2)
                     GROUP BY lncel_name
                     HAVING COUNT(DISTINCT a.period_start_time::date) = 3
                 ) bl), 0) as blacklist_cells,
                
                -- 5. Resolved: có HÔM QUA, KHÔNG CÓ HÔM NAY
                COALESCE((SELECT COUNT(*)
                 FROM (
                     SELECT lncel_name
                     FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                     WHERE a.period_start_time::date = dr.date - 1
                     EXCEPT
                     SELECT lncel_name
                     FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                     WHERE a.period_start_time::date = dr.date
                 ) rs), 0) as resolved_cells,
                
                -- 6. New: có HÔM NAY, KHÔNG CÓ HÔM QUA
                COALESCE((SELECT COUNT(*)
                 FROM (
                     SELECT lncel_name
                     FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                     WHERE a.period_start_time::date = dr.date
                     EXCEPT
                     SELECT lncel_name
                     FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive a
                     WHERE a.period_start_time::date = dr.date - 1
                 ) nw), 0) as new_cells,
                
                -- 7. Total cells - từ bảng data gốc
                (SELECT COUNT(*)
                 FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatarawtotalcellslogs d
                 WHERE d.period_start_time::date = dr.date) as total_cells

            FROM date_range dr
            ORDER BY dr.date ASC";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", parsedEndDate);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new TrendDataDto
                    {
                        Date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                        CongestedCells = reader.GetInt32(1),
                        ProcessingCells = reader.GetInt32(2),
                        ExistingCells = reader.GetInt32(3),
                        BlacklistCells = reader.GetInt32(4),
                        ResolvedCells = reader.GetInt32(5),
                        TotalCells = reader.GetInt32(7), // index 6 = new_cells, 7 = total_cells
                        SuccessRate = 0 // Tính sau
                    });
                }
                //========================================================================

                // 4. Tính SuccessRate (so với ProcessingCells ngày trước)
                // 4. Tính SuccessRate (so với ProcessingCells ngày trước)
                for (int i = 0; i < results.Count; i++)
                {
                    if (i == 0)
                    {
                        // Ngày đầu tiên: không có data ngày trước → set 0
                        results[i].SuccessRate = 0;
                    }
                    else
                    {
                        // Ngày N: SuccessRate = Resolved(N) / Processing(N-1) * 100
                        var prevProcessing = results[i - 1].ProcessingCells;
                        results[i].SuccessRate = prevProcessing > 0
                            ? Math.Round(results[i].ResolvedCells * 100m / prevProcessing, 1)
                            : 0;
                    }
                }
                //========================================================================

                // 5. Return response
                // 5. BỎ NGÀY ĐẦU TIÊN - Chỉ return 14 ngày
                var finalResults = results.Skip(1).ToList(); // ← THÊM: Bỏ ngày 0

                // 5. Return response
                return Ok(new
                {
                    success = true,
                    data = results
                });
            }
            //========================================================================

            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
            //========================================================================
        }
        // ket thuc public async Task<IActionResult> GetNetworkTrend([FromQuery] string? endDate = null)
        //========================================================================


        //========================================================================
        // API #2: GET TREND 14 NGÀY - THEO TỈNH
        //========================================================================
        /// <summary>
        /// Lấy trend data 14 ngày cho một tỉnh cụ thể
        /// </summary>
        /// <param name="endDate">Ngày kết thúc (format: YYYY-MM-DD). Mặc định: hôm nay</param>
        /// <param name="provinceCode">Mã tỉnh (VD: HN, DN, SG)</param>
        /// <returns>List trend data 14 ngày của tỉnh</returns>
        [HttpGet("trend-province")]
        public async Task<IActionResult> GetProvinceTrend(
            [FromQuery] string? endDate = null,
            [FromQuery] string? provinceCode = null)
        {
            try
            {
                // 1. Validate provinceCode
                if (string.IsNullOrEmpty(provinceCode))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "provinceCode is required"
                    });
                }
                //========================================================================

                // 2. Parse & validate date
                DateTime parsedEndDate;
                if (string.IsNullOrEmpty(endDate))
                {
                    parsedEndDate = DateTime.Today;
                }
                else
                {
                    if (!DateTime.TryParse(endDate, out parsedEndDate))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid date format. Use YYYY-MM-DD"
                        });
                    }
                }
                //========================================================================

                // 3. Calculate startDate - Lấy 15 ngày
                DateTime startDate = parsedEndDate.AddDays(-14);

                // 4. SQL Query - Lấy từ bảng summaryprovince
                var results = new List<TrendDataDto>();

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
            SELECT 
                period_start_time::date as date,
                COALESCE(congested_cells, 0) as congested_cells,
                COALESCE(processing_cells, 0) as processing_cells,
                COALESCE(existing_cells, 0) as existing_cells,
                COALESCE(blacklist_cells, 0) as blacklist_cells,
                COALESCE(resolved_cells, 0) as resolved_cells,
                COALESCE(new_cells, 0) as new_cells,
                COALESCE(total_cells, 0) as total_cells
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatasummaryprovince
            WHERE province_code = @provinceCode
              AND period_start_time::date BETWEEN @startDate AND @endDate
            ORDER BY period_start_time ASC";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@provinceCode", provinceCode);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", parsedEndDate);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new TrendDataDto
                    {
                        Date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                        CongestedCells = reader.GetInt32(1),
                        ProcessingCells = reader.GetInt32(2),
                        ExistingCells = reader.GetInt32(3),
                        BlacklistCells = reader.GetInt32(4),
                        ResolvedCells = reader.GetInt32(5),
                        TotalCells = reader.GetInt32(7),
                        SuccessRate = 0 // Tính sau
                    });
                }

                // 5. Tính SuccessRate
                for (int i = 0; i < results.Count; i++)
                {
                    if (i == 0)
                    {
                        results[i].SuccessRate = 0;
                    }
                    else
                    {
                        var prevProcessing = results[i - 1].ProcessingCells;
                        results[i].SuccessRate = prevProcessing > 0
                            ? Math.Round(results[i].ResolvedCells * 100m / prevProcessing, 1)
                            : 0;
                    }
                }

                // 6. Bỏ ngày đầu - Return 14 ngày
                var finalResults = results.Skip(1).ToList();

                // 7. Return response
                return Ok(new
                {
                    success = true,
                    provinceCode = provinceCode,
                    data = finalResults
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
        //========================================================================

        //========================================================================
        // API #3: GET TREND 14 NGÀY - THEO HUYỆN
        //========================================================================
        /// <summary>
        /// Lấy trend data 14 ngày cho một huyện cụ thể
        /// </summary>
        /// <param name="endDate">Ngày kết thúc (format: YYYY-MM-DD). Mặc định: hôm nay</param>
        /// <param name="districtCode">Mã huyện (VD: HD, CG, BC)</param>
        /// <returns>List trend data 14 ngày của huyện</returns>
        [HttpGet("trend-district")]
        public async Task<IActionResult> GetDistrictTrend(
            [FromQuery] string? endDate = null,
            [FromQuery] string? districtCode = null)
        {
            try
            {
                // 1. Validate districtCode
                if (string.IsNullOrEmpty(districtCode))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "districtCode is required"
                    });
                }

                // 2. Parse & validate date
                DateTime parsedEndDate;
                if (string.IsNullOrEmpty(endDate))
                {
                    parsedEndDate = DateTime.Today;
                }
                else
                {
                    if (!DateTime.TryParse(endDate, out parsedEndDate))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid date format. Use YYYY-MM-DD"
                        });
                    }
                }

                // 3. Calculate startDate - Lấy 15 ngày
                DateTime startDate = parsedEndDate.AddDays(-14);

                // 4. SQL Query - Lấy từ bảng summarydistrict
                var results = new List<TrendDataDto>();

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
            SELECT 
                period_start_time::date as date,
                COALESCE(congested_cells, 0) as congested_cells,
                COALESCE(processing_cells, 0) as processing_cells,
                COALESCE(existing_cells, 0) as existing_cells,
                COALESCE(blacklist_cells, 0) as blacklist_cells,
                COALESCE(resolved_cells, 0) as resolved_cells,
                COALESCE(new_cells, 0) as new_cells,
                COALESCE(total_cells, 0) as total_cells
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatasummarydistrict
            WHERE district_code = @districtCode
              AND period_start_time::date BETWEEN @startDate AND @endDate
            ORDER BY period_start_time ASC";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@districtCode", districtCode);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", parsedEndDate);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new TrendDataDto
                    {
                        Date = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                        CongestedCells = reader.GetInt32(1),
                        ProcessingCells = reader.GetInt32(2),
                        ExistingCells = reader.GetInt32(3),
                        BlacklistCells = reader.GetInt32(4),
                        ResolvedCells = reader.GetInt32(5),
                        TotalCells = reader.GetInt32(7),
                        SuccessRate = 0 // Tính sau
                    });
                }

                // 5. Tính SuccessRate
                for (int i = 0; i < results.Count; i++)
                {
                    if (i == 0)
                    {
                        results[i].SuccessRate = 0;
                    }
                    else
                    {
                        var prevProcessing = results[i - 1].ProcessingCells;
                        results[i].SuccessRate = prevProcessing > 0
                            ? Math.Round(results[i].ResolvedCells * 100m / prevProcessing, 1)
                            : 0;
                    }
                }
                //========================================================================

                // 6. Bỏ ngày đầu - Return 14 ngày
                var finalResults = results.Skip(1).ToList();

                // 7. Return response
                return Ok(new
                {
                    success = true,
                    districtCode = districtCode,
                    data = finalResults
                });
            }
            //========================================================================

            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
        //========================================================================

        //========================================================================
        // API #4: GET PROVINCES LIST
        //========================================================================
        /// <summary>
        /// Lấy danh sách tất cả các tỉnh có data
        /// </summary>
        /// <param name="date">Ngày (format: YYYY-MM-DD). Mặc định: hôm nay</param>
        /// <returns>List các tỉnh với số liệu tổng hợp</returns>
        [HttpGet("list-provinces")]
        public async Task<IActionResult> GetProvinces([FromQuery] string? date = null)
        {
            try
            {
                // 1. Parse & validate date
                DateTime selectedDate;
                if (string.IsNullOrEmpty(date))
                {
                    selectedDate = DateTime.Today;
                }
                else
                {
                    if (!DateTime.TryParse(date, out selectedDate))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid date format. Use YYYY-MM-DD"
                        });
                    }
                }

                // 2. SQL Query
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
            SELECT 
                province_code,
                COALESCE(congested_cells, 0) as congested_cells,
                COALESCE(processing_cells, 0) as processing_cells,
                COALESCE(existing_cells, 0) as existing_cells,
                COALESCE(blacklist_cells, 0) as blacklist_cells,
                COALESCE(resolved_cells, 0) as resolved_cells,
                COALESCE(new_cells, 0) as new_cells,
                COALESCE(total_cells, 0) as total_cells
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatasummaryprovince
            WHERE period_start_time::date = @date


            ORDER BY province_code ASC";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@date", selectedDate);

                await using var reader = await cmd.ExecuteReaderAsync();

                var results = new List<object>();
                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        provinceCode = reader.GetString(0),
                        congestedCells = reader.GetInt32(1),
                        processingCells = reader.GetInt32(2),
                        existingCells = reader.GetInt32(3),
                        blacklistCells = reader.GetInt32(4),
                        resolvedCells = reader.GetInt32(5),
                        newCells = reader.GetInt32(6),
                        totalCells = reader.GetInt32(7)
                    });
                }

                // 3. Return response
                return Ok(new
                {
                    success = true,
                    date = selectedDate.ToString("yyyy-MM-dd"),
                    totalProvinces = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }


        }






        //========================================================================
        // API #5: GET DISTRICTS LIST
        //========================================================================
        /// <summary>
        /// Lấy danh sách tất cả các huyện có data (có thể filter theo tỉnh)
        /// </summary>
        /// <param name="date">Ngày (format: YYYY-MM-DD). Mặc định: hôm nay</param>
        /// <param name="provinceCode">Mã tỉnh (optional, để filter huyện theo tỉnh)</param>
        /// <returns>List các huyện với số liệu tổng hợp</returns>
        [HttpGet("list-districts")]
        public async Task<IActionResult> GetDistricts(
            [FromQuery] string? date = null,
            [FromQuery] string? provinceCode = null)
        {
            try
            {
                // 1. Parse & validate date
                DateTime selectedDate;
                if (string.IsNullOrEmpty(date))
                {
                    selectedDate = DateTime.Today;
                }
                else
                {
                    if (!DateTime.TryParse(date, out selectedDate))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid date format. Use YYYY-MM-DD"
                        });
                    }
                }

                // 2. SQL Query
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
            SELECT 
                district_code,
                province_code,
                COALESCE(congested_cells, 0) as congested_cells,
                COALESCE(processing_cells, 0) as processing_cells,
                COALESCE(existing_cells, 0) as existing_cells,
                COALESCE(blacklist_cells, 0) as blacklist_cells,
                COALESCE(resolved_cells, 0) as resolved_cells,
                COALESCE(new_cells, 0) as new_cells,
                COALESCE(total_cells, 0) as total_cells
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatasummarydistrict
            WHERE period_start_time::date = @date";

                // Nếu có provinceCode thì filter
                if (!string.IsNullOrEmpty(provinceCode))
                {
                    sql += " AND province_code = @provinceCode";
                }

                sql += " ORDER BY district_code ASC";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@date", selectedDate);
                if (!string.IsNullOrEmpty(provinceCode))
                {
                    cmd.Parameters.AddWithValue("@provinceCode", provinceCode);
                }

                await using var reader = await cmd.ExecuteReaderAsync();

                var results = new List<object>();
                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        districtCode = reader.GetString(0),
                        provinceCode = reader.GetString(1),
                        congestedCells = reader.GetInt32(2),
                        processingCells = reader.GetInt32(3),
                        existingCells = reader.GetInt32(4),
                        blacklistCells = reader.GetInt32(5),
                        resolvedCells = reader.GetInt32(6),
                        newCells = reader.GetInt32(7),
                        totalCells = reader.GetInt32(8)
                    });
                }

                // 3. Return response
                return Ok(new
                {
                    success = true,
                    date = selectedDate.ToString("yyyy-MM-dd"),
                    provinceCode = provinceCode,
                    totalDistricts = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
        //========================================================================



        //========================================================================
        // API #6: GET PROVINCE DISTRIBUTION
        //========================================================================
        /// <summary>
        /// Lấy phân bố cells theo từng tỉnh cho ngày cụ thể (để vẽ chart)
        /// </summary>
        /// <param name="date">Ngày (format: YYYY-MM-DD). Mặc định: hôm nay</param>
        /// <returns>List tỉnh với breakdown theo loại cells</returns>
        [HttpGet("province-distribution")]
        public async Task<IActionResult> GetProvinceDistribution([FromQuery] string? date = null)
        {
            try
            {
                // 1. Parse & validate date
                DateTime selectedDate;
                if (string.IsNullOrEmpty(date))
                {
                    selectedDate = DateTime.Today;
                }
                else
                {
                    if (!DateTime.TryParse(date, out selectedDate))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid date format. Use YYYY-MM-DD"
                        });
                    }
                }

                // 2. SQL Query
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
            SELECT 
                province_code,
                COALESCE(congested_cells, 0) as congested_cells,
                COALESCE(processing_cells, 0) as processing_cells,
                COALESCE(existing_cells, 0) as existing_cells,
                COALESCE(blacklist_cells, 0) as blacklist_cells,
                COALESCE(resolved_cells, 0) as resolved_cells,
                COALESCE(new_cells, 0) as new_cells,
                COALESCE(total_cells, 0) as total_cells
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatasummaryprovince
            WHERE period_start_time::date = @date
            ORDER BY congested_cells DESC, province_code ASC";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@date", selectedDate);

                await using var reader = await cmd.ExecuteReaderAsync();

                var results = new List<object>();
                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        provinceCode = reader.GetString(0),
                        congestedCells = reader.GetInt32(1),
                        processingCells = reader.GetInt32(2),
                        existingCells = reader.GetInt32(3),
                        blacklistCells = reader.GetInt32(4),
                        resolvedCells = reader.GetInt32(5),
                        newCells = reader.GetInt32(6),
                        totalCells = reader.GetInt32(7),
                        // Tính % congested
                        congestionRate = reader.GetInt32(7) > 0
                            ? Math.Round((double)reader.GetInt32(1) / reader.GetInt32(7) * 100, 2)
                            : 0
                    });
                }

                // 3. Return response
                return Ok(new
                {
                    success = true,
                    date = selectedDate.ToString("yyyy-MM-dd"),
                    totalProvinces = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
        //========================================================================



        //========================================================================
        // API #7: GET DISTRICT DISTRIBUTION
        //========================================================================
        /// <summary>
        /// Lấy phân bố cells theo từng huyện cho ngày cụ thể (có thể filter theo tỉnh)
        /// </summary>
        /// <param name="date">Ngày (format: YYYY-MM-DD). Mặc định: hôm nay</param>
        /// <param name="provinceCode">Mã tỉnh (optional, để filter huyện theo tỉnh)</param>
        /// <returns>List huyện với breakdown theo loại cells</returns>
        [HttpGet("district-distribution")]
        public async Task<IActionResult> GetDistrictDistribution(
            [FromQuery] string? date = null,
            [FromQuery] string? provinceCode = null)
        {
            try
            {
                // 1. Parse & validate date
                DateTime selectedDate;
                if (string.IsNullOrEmpty(date))
                {
                    selectedDate = DateTime.Today;
                }
                else
                {
                    if (!DateTime.TryParse(date, out selectedDate))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid date format. Use YYYY-MM-DD"
                        });
                    }
                }

                // 2. SQL Query
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
            SELECT 
                district_code,
                province_code,
                COALESCE(congested_cells, 0) as congested_cells,
                COALESCE(processing_cells, 0) as processing_cells,
                COALESCE(existing_cells, 0) as existing_cells,
                COALESCE(blacklist_cells, 0) as blacklist_cells,
                COALESCE(resolved_cells, 0) as resolved_cells,
                COALESCE(new_cells, 0) as new_cells,
                COALESCE(total_cells, 0) as total_cells
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatasummarydistrict
            WHERE period_start_time::date = @date";

                // Nếu có provinceCode thì filter
                if (!string.IsNullOrEmpty(provinceCode))
                {
                    sql += " AND province_code = @provinceCode";
                }

                sql += " ORDER BY congested_cells DESC, district_code ASC";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@date", selectedDate);
                if (!string.IsNullOrEmpty(provinceCode))
                {
                    cmd.Parameters.AddWithValue("@provinceCode", provinceCode);
                }

                await using var reader = await cmd.ExecuteReaderAsync();

                var results = new List<object>();
                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        districtCode = reader.GetString(0),
                        provinceCode = reader.GetString(1),
                        congestedCells = reader.GetInt32(2),
                        processingCells = reader.GetInt32(3),
                        existingCells = reader.GetInt32(4),
                        blacklistCells = reader.GetInt32(5),
                        resolvedCells = reader.GetInt32(6),
                        newCells = reader.GetInt32(7),
                        totalCells = reader.GetInt32(8),
                        // Tính % congested
                        congestionRate = reader.GetInt32(8) > 0
                            ? Math.Round((double)reader.GetInt32(2) / reader.GetInt32(8) * 100, 2)
                            : 0
                    });
                }

                // 3. Return response
                return Ok(new
                {
                    success = true,
                    date = selectedDate.ToString("yyyy-MM-dd"),
                    provinceCode = provinceCode,
                    totalDistricts = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }








        //========================================================================
        // API #8: GET HEADER STATS
        //========================================================================
        /// <summary>
        /// Lấy các chỉ số tổng quan toàn mạng cho header/dashboard
        /// </summary>
        /// <param name="date">Ngày (format: YYYY-MM-DD). Mặc định: hôm nay</param>
        /// <returns>Stats tổng quan với so sánh ngày hôm qua</returns>
        [HttpGet("header-stats")]
        public async Task<IActionResult> GetHeaderStats([FromQuery] string? date = null)
        {
            try
            {
                // 1. Parse & validate date
                DateTime selectedDate;
                if (string.IsNullOrEmpty(date))
                {
                    selectedDate = DateTime.Today;
                }
                else
                {
                    if (!DateTime.TryParse(date, out selectedDate))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid date format. Use YYYY-MM-DD"
                        });
                    }
                }

                DateTime yesterday = selectedDate.AddDays(-1);

                // 2. SQL Query - Aggregate từ bảng summaryprovince
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
            SELECT 
                SUM(COALESCE(congested_cells, 0)) as congested_cells,
                SUM(COALESCE(processing_cells, 0)) as processing_cells,
                SUM(COALESCE(existing_cells, 0)) as existing_cells,
                SUM(COALESCE(blacklist_cells, 0)) as blacklist_cells,
                SUM(COALESCE(resolved_cells, 0)) as resolved_cells,
                SUM(COALESCE(new_cells, 0)) as new_cells,
                SUM(COALESCE(total_cells, 0)) as total_cells
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatasummaryprovince
            WHERE period_start_time::date = @date";

                // Get today stats
                await using var cmdToday = new NpgsqlCommand(sql, connection);
                cmdToday.Parameters.AddWithValue("@date", selectedDate);

                await using var readerToday = await cmdToday.ExecuteReaderAsync();

                int todayCongestedCells = 0;
                int todayProcessingCells = 0;
                int todayExistingCells = 0;
                int todayBlacklistCells = 0;
                int todayResolvedCells = 0;
                int todayNewCells = 0;
                int todayTotalCells = 0;

                if (await readerToday.ReadAsync())
                {
                    todayCongestedCells = readerToday.GetInt32(0);
                    todayProcessingCells = readerToday.GetInt32(1);
                    todayExistingCells = readerToday.GetInt32(2);
                    todayBlacklistCells = readerToday.GetInt32(3);
                    todayResolvedCells = readerToday.GetInt32(4);
                    todayNewCells = readerToday.GetInt32(5);
                    todayTotalCells = readerToday.GetInt32(6);
                }
                await readerToday.CloseAsync();

                // Get yesterday stats
                await using var cmdYesterday = new NpgsqlCommand(sql, connection);
                cmdYesterday.Parameters.AddWithValue("@date", yesterday);

                await using var readerYesterday = await cmdYesterday.ExecuteReaderAsync();

                int yesterdayCongestedCells = 0;
                int yesterdayProcessingCells = 0;
                int yesterdayResolvedCells = 0;

                if (await readerYesterday.ReadAsync())
                {
                    yesterdayCongestedCells = readerYesterday.GetInt32(0);
                    yesterdayProcessingCells = readerYesterday.GetInt32(1);
                    yesterdayResolvedCells = readerYesterday.GetInt32(4);
                }
                await readerYesterday.CloseAsync();

                // 3. Calculate rates and comparisons
                decimal congestionRate = todayTotalCells > 0
                    ? Math.Round((decimal)todayCongestedCells / todayTotalCells * 100, 2)
                    : 0;

                decimal successRate = yesterdayProcessingCells > 0
                    ? Math.Round((decimal)todayResolvedCells / yesterdayProcessingCells * 100, 1)
                    : 0;

                // 4. Return response
                return Ok(new
                {
                    success = true,
                    date = selectedDate.ToString("yyyy-MM-dd"),
                    stats = new
                    {
                        totalCells = todayTotalCells,
                        congestedCells = todayCongestedCells,
                        processingCells = todayProcessingCells,
                        existingCells = todayExistingCells,
                        blacklistCells = todayBlacklistCells,
                        resolvedCells = todayResolvedCells,
                        newCells = todayNewCells,
                        congestionRate = congestionRate,
                        successRate = successRate
                    },
                    comparison = new
                    {
                        congestedCells = new
                        {
                            yesterday = yesterdayCongestedCells,
                            diff = todayCongestedCells - yesterdayCongestedCells,
                            diffPercent = yesterdayCongestedCells > 0
                                ? Math.Round((decimal)(todayCongestedCells - yesterdayCongestedCells) / yesterdayCongestedCells * 100, 1)
                                : 0
                        },
                        processingCells = new
                        {
                            yesterday = yesterdayProcessingCells,
                            diff = todayProcessingCells - yesterdayProcessingCells,
                            diffPercent = yesterdayProcessingCells > 0
                                ? Math.Round((decimal)(todayProcessingCells - yesterdayProcessingCells) / yesterdayProcessingCells * 100, 1)
                                : 0
                        }
                    }
                });
            }
            //========================================================================

            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
        //========================================================================

        //========================================================================
        // API: GET CR EXECUTION DATES
        //========================================================================



        [HttpGet("cr-logs-dates")]
        public async Task<IActionResult> GetCRExecutionDates([FromQuery] string cellId, [FromQuery] string selectedDate)
        {
            try
            {
                if (string.IsNullOrEmpty(cellId))
                {
                    return BadRequest(new { message = "cellId parameter is required" });
                }

                if (string.IsNullOrEmpty(selectedDate))
                {
                    return BadRequest(new { message = "selectedDate parameter is required" });
                }

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Query để lấy các ngày thực hiện CR cho cell này
                var query = @"
            SELECT DISTINCT period_start_time::DATE as execution_date
            FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsindoffvalueprocessdatacrlogs
            WHERE (dnmrbtssran LIKE '%' || @cellId || '%' 
               OR (@cellId LIKE '%' || ecgi_adj_enb_id || '%' AND @cellId LIKE '%' ||  ecgi_lcr_id || '%'))
AND period_start_time::DATE >= @SelectedDate::DATE - INTERVAL '21 days'
  AND period_start_time::DATE <= @SelectedDate::DATE
            ORDER BY execution_date ASC
        ";

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("CellId", cellId);
                cmd.Parameters.AddWithValue("selecteddate", selectedDate);

                var dates = new List<string>();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // var date = reader.GetDateTime(0).ToString("yyyy-MM-dd");
                    var date = reader.GetDateTime(0).ToString("yyyy-MM-dd") + " 05:00";
                    dates.Add(date);
                }

                return Ok(new { dates });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching CR execution dates",
                    error = ex.Message
                });
            }
        }
        //========================================================================





    }
    // ket thuc public class PRBsLoadCellDashboardApiController : ControllerBase
    //========================================================================

}
