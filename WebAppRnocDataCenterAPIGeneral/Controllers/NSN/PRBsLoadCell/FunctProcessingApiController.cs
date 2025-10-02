using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;




using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Npgsql;
using NpgsqlTypes;


namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.PRBsLoadCell
{

    [ApiController]
    [Route("api/prbsload-cell/funprocessing")]
    [Produces("application/json")]

    public class PRBsLoadCellFunctProcessingApiController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PRBsLoadCellFunctProcessingApiController(IConfiguration config)
        {
            _config = config;
        }
        //========================================================================
        [HttpPost("upload")]

        [RequestSizeLimit(2_147_483_648)] // 2GB = 2 * 1024 * 1024 * 1024 bytes
        [RequestFormLimits(MultipartBodyLengthLimit = 2_147_483_648)]
        //========================================================================

        public async Task<IActionResult> ImportZip(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file uploaded" });

            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { success = false, message = "Only ZIP files allowed" });

            try
            {
                // var connectionString = _config.GetConnectionString("PostgreSQL");
                var connectionString = _config.GetConnectionString("InformationProductionConnection");
                // la chuoi chua thong tin ket noi db
                // trong program. cs
                

                // Giải nén ZIP và tìm file CSV
                using var zipStream = file.OpenReadStream();
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                //========================================================================

                var csvEntry = archive.Entries.FirstOrDefault(e =>
                    e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));

                if (csvEntry == null)
                    return BadRequest(new { success = false, message = "No CSV file found in ZIP" });
                //========================================================================

                // Xử lý CSV
                using var csvStream = csvEntry.Open();
                // var totalRows = await service.ImportCsvAsync(csvStream);
                
                var totalRows = await ProcessCsvAsync(csvStream, connectionString);
                //========================================================================

                return Ok(new
                {
                    success = true,
                    message = $"Imported {totalRows} rows successfully",
                    totalRows = totalRows,
                    fileName = csvEntry.Name
                });
                //========================================================================

            }
            //========================================================================
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    detail = ex.StackTrace
                });
            }
            //========================================================================
        }

        // ket thuc ham public async Task<IActionResult> ImportZip(IFormFile file)
        //========================================================================



        private async Task<int> ProcessCsvAsync(Stream fileStream, string connectionString)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            //========================================================================

            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                Delimiter = ";",  // ✅ FIX: Dùng dấu chấm phẩy
                TrimOptions = TrimOptions.Trim  // ✅ FIX: Trim khoảng trắng
            });
            //========================================================================

            await csv.ReadAsync();
            csv.ReadHeader();

            var totalRows = 0;
            var batch = new List<CsvRow>();
            var batchSize = 1000;
            //========================================================================

            while (await csv.ReadAsync())
            {
                try
                {
                    var row = new CsvRow
                    {
                        PeriodStartTime = ParseDateTime(csv.GetField("PERIOD_START_TIME")),
                        MrbtsSbtsName = csv.GetField("MRBTS/SBTS name") ?? "",
                        LnbtsName = csv.GetField("LNBTS name") ?? "",
                        LncelName = csv.GetField("LNCEL name") ?? "",
                        Dn_ = csv.GetField("DN") ?? "",
                        PdcpSduVolumeDl = ParseDecimal(csv.GetField("PDCP SDU Volume, DL (classic eNB)")),
                        PdcpSduVolumeUl = ParseDecimal(csv.GetField("PDCP SDU Volume, UL (classic eNB)")),
                        EutranAvgPrbUsageDl = ParseDecimal(csv.GetField("E-UTRAN Avg PRB usage per TTI DL")),
                        MaxPdcpThrDl = ParseDecimal(csv.GetField("Max PDCP Thr DL (classic eNB)")),
                        MaxPdcpThrUl = ParseDecimal(csv.GetField("Max PDCP Thr UL (classic eNB)"))
                    };
                    //========================================================================

                    batch.Add(row);
                    //========================================================================

                    if (batch.Count >= batchSize)
                    {
                        await BulkInsertAsync(connection, batch);
                        totalRows += batch.Count;
                        batch.Clear();
                    }
                    //========================================================================

                }
                //========================================================================

                catch (Exception ex)
                {
                    Console.WriteLine($"Error row: {ex.Message}");
                }
            }
            //========================================================================

            if (batch.Count > 0)
            {
                await BulkInsertAsync(connection, batch);
                totalRows += batch.Count;
            }

            return totalRows;
        }
        //========================================================================
        private DateTime ParseDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return DateTime.MinValue;
            return DateTime.TryParse(value, out var result) ? result : DateTime.MinValue;
        }

        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }

        private async Task BulkInsertAsync(NpgsqlConnection connection, List<CsvRow> batch)
        {
            using var writer = connection.BeginBinaryImport(
                @"COPY system_nsn_prbsloadcell.objtablekpiprbsloadcellsdata 
                (period_start_time, mrbts_name, lnbts_name, lncel_name, dn_mrbts_site,
                 pdcp_sdu_volume_dl, pdcp_sdu_volume_ul, eutran_avg_prb_usage_dl,
                 max_pdcp_thr_dl, max_pdcp_thr_ul) 
                FROM STDIN (FORMAT BINARY)"
            );

            foreach (var row in batch)
            {
                writer.StartRow();
                writer.Write(row.PeriodStartTime, NpgsqlDbType.Timestamp);
                writer.Write(row.MrbtsSbtsName, NpgsqlDbType.Varchar);
                writer.Write(row.LnbtsName, NpgsqlDbType.Varchar);
                writer.Write(row.LncelName, NpgsqlDbType.Varchar);
                writer.Write(row.Dn_, NpgsqlDbType.Varchar);
                writer.Write(row.PdcpSduVolumeDl, NpgsqlDbType.Numeric);
                writer.Write(row.PdcpSduVolumeUl, NpgsqlDbType.Numeric);
                writer.Write(row.EutranAvgPrbUsageDl, NpgsqlDbType.Numeric);
                writer.Write(row.MaxPdcpThrDl, NpgsqlDbType.Numeric);
                writer.Write(row.MaxPdcpThrUl, NpgsqlDbType.Numeric);
            }

            await writer.CompleteAsync();
        }
        //========================================================================

        private class CsvRow
        {
            public DateTime PeriodStartTime { get; set; }
            public string MrbtsSbtsName { get; set; }
            public string LnbtsName { get; set; }
            public string LncelName { get; set; }
            public string Dn_ { get; set; }
            public decimal PdcpSduVolumeDl { get; set; }
            public decimal PdcpSduVolumeUl { get; set; }
            public decimal EutranAvgPrbUsageDl { get; set; }
            public decimal MaxPdcpThrDl { get; set; }
            public decimal MaxPdcpThrUl { get; set; }
        }






    }
    // public class PRBsLoadCellFunctProcessingApiController : ControllerBase
}
