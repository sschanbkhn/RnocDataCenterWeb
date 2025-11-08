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
    [Route("api/prbs-load/monitoring")]
    [Produces("application/json")]
    public class PRBsLoadCellMonitoringApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public PRBsLoadCellMonitoringApiController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("InformationProductionConnection");
        }

        // =============================================================================
        // API MONITOR - Lấy data từ filterdetailarchive (1 ngày)
        // =============================================================================
        [HttpGet("monitor-kpi/{date}")]
        public async Task<IActionResult> GetMonitorData(string date)
        {
            try
            {
                DateTime selectedDate = DateTime.Parse(date);

                var query = @"
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
                        created_at,
                        -- PARSE PROVINCE (phần cuối sau dấu _ cuối cùng)
                        REVERSE(SPLIT_PART(REVERSE(mrbts_name), '_', 1)) as province_code,
                        -- PARSE DISTRICT (3 ký tự đầu của phần giữa)
                         SUBSTRING(SPLIT_PART(mrbts_name, '_', 2), 1, 3) as district_code,
'NSN' as vendor,     -- ← THÊM GIÁ TRỊ MẶC ĐỊNH
        'KV1' as region      -- ← THÊM GIÁ TRỊ MẶC ĐỊNH

                    FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive
                    WHERE DATE(period_start_time) = @SelectedDate
                    ORDER BY period_start_time DESC, eutran_avg_prb_usage_dl DESC, total_occurrences DESC;
                ";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@SelectedDate", selectedDate.Date);

                var records = new List<object>();

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    records.Add(new
                    {
                        rawid = reader.GetInt32(0),
                        period_start_time = reader.GetDateTime(1),
                        mrbts_name = reader.IsDBNull(2) ? null : reader.GetString(2),
                        lnbts_name = reader.IsDBNull(3) ? null : reader.GetString(3),
                        lncel_name = reader.IsDBNull(4) ? null : reader.GetString(4),
                        dn_mrbts_site = reader.IsDBNull(5) ? null : reader.GetString(5),
                        pdcp_sdu_volume_dl = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                        pdcp_sdu_volume_ul = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                        eutran_avg_prb_usage_dl = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                        max_pdcp_thr_dl = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9),
                        max_pdcp_thr_ul = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10),
                        affected_days = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                        total_occurrences = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                        avg_prb = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13),
                        max_prb = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14),
                        daily_detail = reader.IsDBNull(15) ? null : reader.GetString(15),
                        created_at = reader.GetDateTime(16),
                        province = reader.IsDBNull(17) ? null : reader.GetString(17),
                        district = reader.IsDBNull(18) ? null : reader.GetString(18),

                        vendor = reader.IsDBNull(19) ? null : reader.GetString(19),
                        region = reader.IsDBNull(20) ? null : reader.GetString(20)
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        records = records,
                        totalRecords = records.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // =============================================================================
        // API MONITOR - Lấy data từ filterdetailarchive (range)
        // =============================================================================
        [HttpGet("monitor-range-kpi")]
        public async Task<IActionResult> GetMonitorDataRange(
            [FromQuery] string startDate,
            [FromQuery] string endDate)
        {
            try
            {
                DateTime start = DateTime.Parse(startDate);
                DateTime end = DateTime.Parse(endDate);

                var query = @"
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
                        created_at,
                        -- PARSE PROVINCE (phần cuối sau dấu _ cuối cùng)
    REVERSE(SPLIT_PART(REVERSE(mrbts_name), '_', 1)) as province_code,
    -- PARSE DISTRICT (3 ký tự đầu của phần giữa)
     SUBSTRING(SPLIT_PART(mrbts_name, '_', 2), 1, 3) as district_code


                    FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsdatafilterdetailarchive
                    WHERE DATE(period_start_time) BETWEEN @StartDate AND @EndDate
                    ORDER BY period_start_time DESC, eutran_avg_prb_usage_dl DESC;
                ";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@StartDate", start.Date);
                cmd.Parameters.AddWithValue("@EndDate", end.Date);

                var records = new List<object>();

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {

                        // Đọc mrbts_name 1 lần duy nhất
                        string mrbtsName = reader.IsDBNull(2) ? null : reader.GetString(2);

                        // SKIP nếu chứa "S_" hoặc "S-"
                        if (!string.IsNullOrEmpty(mrbtsName) &&
                            (mrbtsName.Contains("S_") || mrbtsName.Substring(10, mrbtsName.Length -10).Contains("S-"))
                            )
                        {
                            continue;  // ← BỎ QUA bản ghi này
                        }

                        records.Add(new
                        {
                            rawid = reader.GetInt32(0),
                            // period_start_time = (DateTime)reader.GetDateTime(1),
                            period_start_time = reader.GetDateTime(1).ToString("yyyy-MM-dd"),
                            // mrbts_name = reader.IsDBNull(2) ? null : reader.GetString(2),
                            mrbts_name = mrbtsName,  // ← Dùng biến đã đọc
                            lnbts_name = reader.IsDBNull(3) ? null : reader.GetString(3),
                            lncel_name = reader.IsDBNull(4) ? null : reader.GetString(4),
                            dn_mrbts_site = reader.IsDBNull(5) ? null : reader.GetString(5),
                            pdcp_sdu_volume_dl = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                            pdcp_sdu_volume_ul = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                            eutran_avg_prb_usage_dl = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                            max_pdcp_thr_dl = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9),
                            max_pdcp_thr_ul = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10),
                            affected_days = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                            total_occurrences = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                            avg_prb = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13),
                            max_prb = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14),
                            daily_detail = reader.IsDBNull(15) ? null : reader.GetString(15),
                            created_at = reader.GetDateTime(16),
                            province = reader.IsDBNull(17) ? null : reader.GetString(17),
                            district = reader.IsDBNull(18) ? null : reader.GetString(18),
                            vendor = "NSN",    // ← THÊM MẶC ĐỊNH
                            region = "KV1"     // ← THÊM MẶC ĐỊNH
                        });
                    
                    
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        records = records,
                        totalRecords = records.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}