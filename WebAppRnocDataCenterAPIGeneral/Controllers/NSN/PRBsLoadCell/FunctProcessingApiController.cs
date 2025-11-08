using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;




using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Npgsql;
using NpgsqlTypes;
using Org.BouncyCastle.Asn1.X509;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using FluentFTP;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;


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
        //========================================================================

        //========================================================================
        //************************************************************************
        //========================================================================
        //************************************************************************
        //========================================================================
        //************************************************************************
        //========================================================================
        //************************************************************************
        //========================================================================
        //************************************************************************
        
        // phan nay la API doc CSV HO
        private async Task<int> ProcessHandoverCsvAsync(Stream fileStream, string connectionString)
        {
            // ham API
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            //========================================================================

            // ✅ TRUNCATE TABLE TRƯỚC KHI INSERT
            Console.WriteLine("🔵 Truncating table...");
            using (var cmd = new NpgsqlCommand(
                "TRUNCATE TABLE system_nsn_prbsloadcell.objtablekpiprbsloadcellsHOprocessdata",
                connection))
            {
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine("✅ Table truncated");
            }



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


            var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();


            var totalRows = 0;
            var batch = new List<HandoverRow>();
            var batchSize = 5000;   // Batch lớn hơn vì handover data ít cột numeric hơn
            //========================================================================




            // ✅ DÙNG CsvHelper để đọc
            while (await csv.ReadAsync())
            {
                try
                {
                    
                    var row = new HandoverRow
                    {
                        PeriodStartTime = ParseDateTime(csv.GetField(0)),
                        SourcePlmnName = csv.GetField(1)?.Trim() ?? "",
                        SourceMrbtsName = csv.GetField(2)?.Trim() ?? "",
                        SourceLnbtsName = csv.GetField(3)?.Trim() ?? "",
                        SourceLncelName = csv.GetField(4)?.Trim() ?? "",
                        TargetPlmnName = csv.GetField(5)?.Trim() ?? "",
                        TargetMrbtsName = csv.GetField(6)?.Trim() ?? "",
                        TargetLnbtsName = csv.GetField(7)?.Trim() ?? "",
                        TargetLnbtsId = csv.GetField(8)?.Trim() ?? "",
                        TargetLncelName = csv.GetField(9)?.Trim() ?? "",
                        TargetLncelTac = csv.GetField(10)?.Trim() ?? "",
                        TargetLcrId = csv.GetField(11)?.Trim() ?? "",
                        MccId = csv.GetField(12)?.Trim() ?? "",
                        MncId = csv.GetField(13)?.Trim() ?? "",
                        EciId = csv.GetField(14)?.Trim() ?? "",
                        Dn = csv.GetField(15)?.Trim() ?? "",
                        IntraEnbHoAttempts = ParseInt(csv.GetField(16)),
                        InterEnbHoAttempts = ParseInt(csv.GetField(17)),
                        InterFreqLbHoAttempts = ParseInt(csv.GetField(18))
                    };
                    
                    batch.Add(row);

                    if (batch.Count >= batchSize)
                    {
                        await BulkInsertHandoverAsync(connection, batch);
                        totalRows += batch.Count;
                        batch.Clear();
                    }
                }
                //========================================================================

                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing row: {ex.Message}");
                }
                //========================================================================
            }
            // ket thuc while (!reader.EndOfStream)
            //========================================================================

            // Insert remaining rows
            if (batch.Count > 0)
            {
                await BulkInsertHandoverAsync(connection, batch);
                totalRows += batch.Count;
            }
            //========================================================================

            // stopwatch.Stop();
            // Console.WriteLine($"🎉 Total: {totalRows} rows in {stopwatch.Elapsed.TotalSeconds:F2}s");


            return totalRows;
            //========================================================================

        }
        // ket thuc private async Task<int> ProcessHandoverCsvAsync(Stream fileStream, string connectionString)
        //========================================================================


        private async Task BulkInsertHandoverAsync(NpgsqlConnection connection, List<HandoverRow> batch)
        {
            
            using var writer = connection.BeginBinaryImport(
                @"COPY system_nsn_prbsloadcell.objtablekpiprbsloadcellsHOprocessdata 
                (period_start_time, source_plmn_name, source_mrbts_name, source_lnbts_name, source_lncel_name,
                 target_plmn_name, target_mrbts_name, target_lnbts_name, target_lnbts_id, target_lncel_name,
                 target_lncel_tac, target_lcr_id, mcc_id, mnc_id, eci_id, dn,
                 intra_enb_ho_attempts, inter_enb_ho_attempts, inter_freq_lb_ho_attempts) 
                FROM STDIN (FORMAT BINARY)"
            );

            foreach (var row in batch)
            {
                writer.StartRow();
                // writer.Write(row.PeriodStartTime, NpgsqlDbType.Timestamp);
                // writer.StartRow();
                writer.Write(row.PeriodStartTime, NpgsqlDbType.Date);
                writer.Write(row.SourcePlmnName, NpgsqlDbType.Varchar);
                writer.Write(row.SourceMrbtsName, NpgsqlDbType.Varchar);
                writer.Write(row.SourceLnbtsName, NpgsqlDbType.Varchar);
                writer.Write(row.SourceLncelName, NpgsqlDbType.Varchar);
                writer.Write(row.TargetPlmnName, NpgsqlDbType.Varchar);
                writer.Write(row.TargetMrbtsName, NpgsqlDbType.Varchar);
                writer.Write(row.TargetLnbtsName, NpgsqlDbType.Varchar);
                writer.Write(row.TargetLnbtsId, NpgsqlDbType.Varchar);
                writer.Write(row.TargetLncelName, NpgsqlDbType.Varchar);
                writer.Write(row.TargetLncelTac, NpgsqlDbType.Varchar);
                writer.Write(row.TargetLcrId, NpgsqlDbType.Varchar);
                writer.Write(row.MccId, NpgsqlDbType.Varchar);
                writer.Write(row.MncId, NpgsqlDbType.Varchar);
                writer.Write(row.EciId, NpgsqlDbType.Varchar);
                writer.Write(row.Dn, NpgsqlDbType.Varchar);
                writer.Write(row.IntraEnbHoAttempts, NpgsqlDbType.Integer);
                writer.Write(row.InterEnbHoAttempts, NpgsqlDbType.Integer);
                writer.Write(row.InterFreqLbHoAttempts, NpgsqlDbType.Integer);
            }
            // ket thuc foreach (var row in batch)
            //========================================================================

            await writer.CompleteAsync();
            //========================================================================
        }
        // ket thuc private async Task BulkInsertHandoverAsync(NpgsqlConnection connection, List<HandoverRow> batch)
        //========================================================================


        private int ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }
        // ket thuc private int ParseInt(string value)
        //========================================================================


        private class HandoverRow
        {
            public DateTime PeriodStartTime { get; set; }
            public string SourcePlmnName { get; set; }
            public string SourceMrbtsName { get; set; }
            public string SourceLnbtsName { get; set; }
            public string SourceLncelName { get; set; }
            public string TargetPlmnName { get; set; }
            public string TargetMrbtsName { get; set; }
            public string TargetLnbtsName { get; set; }
            public string TargetLnbtsId { get; set; }
            public string TargetLncelName { get; set; }
            public string TargetLncelTac { get; set; }
            public string TargetLcrId { get; set; }
            public string MccId { get; set; }
            public string MncId { get; set; }
            public string EciId { get; set; }
            public string Dn { get; set; }
            public int IntraEnbHoAttempts { get; set; }
            public int InterEnbHoAttempts { get; set; }
            public int InterFreqLbHoAttempts { get; set; }
        }
        // ket thuc private class HandoverRow
        //========================================================================


        [HttpPost("upload-HOfile")]
        [RequestSizeLimit(2_147_483_648)] // 2GB = 2 * 1024 * 1024 * 1024 bytes
        [RequestFormLimits(MultipartBodyLengthLimit = 2_147_483_648)]
        //========================================================================

        public async Task<IActionResult> ImportZipHOfile(IFormFile file)
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

                var totalRows = await ProcessHandoverCsvAsync(csvStream, connectionString);
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


        // phan nay doc file PRBscellIndOffValue
        //========================================================================
        //************************************************************************
        //========================================================================

        [HttpPost("upload-IndOfffile")]
        [RequestSizeLimit(2_147_483_648)] // 2GB = 2 * 1024 * 1024 * 1024 bytes
        [RequestFormLimits(MultipartBodyLengthLimit = 2_147_483_648)]
        //========================================================================


        // ko the ko tao bang trong database
        // csv co the doc header de tao ra bang duoc
        // nhung de do vao database thi ko biet 

        public async Task<IActionResult> ImportCSVIndOfffile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file uploaded" });

            // if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                // return BadRequest(new { success = false, message = "Only ZIP files allowed" });

            try
            {
                // var connectionString = _config.GetConnectionString("PostgreSQL");
                var connectionString = _config.GetConnectionString("InformationProductionConnection");
                // la chuoi chua thong tin ket noi db
                // trong program. cs


                // Giải nén ZIP và tìm file CSV
                // using var zipStream = file.OpenReadStream();
                // using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                //========================================================================

                // var csvEntry = archive.Entries.FirstOrDefault(e =>
                    // e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { success = false, message = "Only CSV files allowed" });

                // if (csvEntry == null)
                // return BadRequest(new { success = false, message = "No CSV file found in ZIP" });
                //========================================================================


                // ✅ FIX: Copy stream vào memory để tránh dispose
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;


                // Xử lý CSV
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var totalRows = await ProcessIndOffValueCsvAsync(memoryStream, connectionString);
                stopwatch.Stop();


                // Xử lý CSV
                // using var csvStream = csvEntry.Open();
                // var totalRows = await service.ImportCsvAsync(csvStream);
                // using var csvStream = file.OpenReadStream();

                // var totalRows = await ProcessIndOffValueCsvAsync(csvStream, connectionString);
                //========================================================================

                return Ok(new
                {
                    success = true,
                    message = $"Imported {totalRows} rows successfully",
                    totalRows = totalRows,
                    fileName = file.FileName,

                    durationMs = stopwatch.ElapsedMilliseconds,
                    durationSeconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 2)
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
        // ket thuc ham public async Task<IActionResult> ImportCSVIndOfffile(IFormFile file)
        //************************************************************************
        //========================================================================


        public class CellIndOffNeighValueImportDto
        {
            [Required]
            public string DnMrbtsSran { get; set; } = string.Empty;

            public string? Operation { get; set; }
            public string? CellIndOffNeigh { get; set; }
            public int? EcgiAdjEnbId { get; set; }
            public int? EcgiLcrId { get; set; }
            public string? NrControl { get; set; }
            public string? NrStatus { get; set; }
        }
        // ket thuc public class CellIndOffNeighImportDto
        //========================================================================



        // phan nay la ham service doc CSV 
        private async Task<int> ProcessIndOffValueCsvAsync(Stream fileStream, string connectionString)
        {
            // ham service
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            //========================================================================

            // ✅ TRUNCATE
            using (var cmd = new NpgsqlCommand(
                "TRUNCATE TABLE system_nsn_prbsloadcell.objtablekpiprbsloadcellsindoffvalueprocessdata",
                connection))
            {
                await cmd.ExecuteNonQueryAsync();

            }
            //========================================================================

            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                Delimiter = ",",  // ✅ FIX: Dùng dấu chấm phẩy
                // chu y co file doc la dau cham phay, co file la dau phay
                TrimOptions = TrimOptions.Trim  // ✅ FIX: Trim khoảng trắng
            });
            //========================================================================

            await csv.ReadAsync();
            csv.ReadHeader();

            var totalRows = 0;
            var batch = new List<CellIndOffNeighValueImportDto>();
            var batchSize = 5000;   // Batch lớn hơn vì handover data ít cột numeric hơn
            //========================================================================

            // ✅ DÙNG CsvHelper để đọc
            while (await csv.ReadAsync())
            {
                try
                {

                    var row = new CellIndOffNeighValueImportDto
                    {

                        DnMrbtsSran = csv.GetField(1)?.Trim() ?? "",
                        Operation = csv.GetField(0)?.Trim() ?? "",
                        CellIndOffNeigh = csv.GetField(2)?.Trim() ?? "",
                        EcgiAdjEnbId = ParseInt(csv.GetField(3)?.Trim() ?? ""),
                        EcgiLcrId = ParseInt(csv.GetField(4)?.Trim() ?? ""),
                        NrControl = csv.GetField(5)?.Trim() ?? "",
                        NrStatus = csv.GetField(6)?.Trim() ?? "",

                    };

                    batch.Add(row);

                    if (batch.Count >= batchSize)
                    {
                        await BulkInsertIndOffValueServiceAsync(connection, batch);
                        totalRows += batch.Count;
                        batch.Clear();
                    }
                }
                //========================================================================

                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing row: {ex.Message}");
                }
                //========================================================================
            }
            // ket thuc while (!reader.EndOfStream)
            //========================================================================

            // Insert remaining rows
            if (batch.Count > 0)
            {
                await BulkInsertIndOffValueServiceAsync(connection, batch);
                totalRows += batch.Count;
            }
            //========================================================================

            return totalRows;
            //========================================================================

        }
        // ket thuc private async Task<int> ProcessIndOffValueCsvAsync(Stream fileStream, string connectionString)
        //========================================================================

        private async Task BulkInsertIndOffValueServiceAsync(NpgsqlConnection connection, List<CellIndOffNeighValueImportDto> batch)
        {

            using var writer = connection.BeginBinaryImport(
                @"COPY system_nsn_prbsloadcell.objtablekpiprbsloadcellsindoffvalueprocessdata 
                (operation, dnMrbtsSran, cell_ind_off_neigh, ecgi_adj_enb_id,
                 ecgi_lcr_id, nr_control, nr_status ) 
                FROM STDIN (FORMAT BINARY)"
            );

            foreach (var row in batch)
            {
                writer.StartRow();
                writer.Write(row.Operation, NpgsqlDbType.Varchar);
                writer.Write(row.DnMrbtsSran, NpgsqlDbType.Varchar);
                writer.Write(row.CellIndOffNeigh, NpgsqlDbType.Varchar);
                // writer.Write(row.EcgiAdjEnbId, NpgsqlDbType.Integer);
                writer.Write(row.EcgiAdjEnbId ?? (object)DBNull.Value, NpgsqlDbType.Integer);   // ← SỬA
                // writer.Write(row.EcgiLcrId, NpgsqlDbType.Integer);
                writer.Write(row.EcgiLcrId ?? (object)DBNull.Value, NpgsqlDbType.Integer);       // ← SỬA
                writer.Write(row.NrControl ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                writer.Write(row.NrStatus, NpgsqlDbType.Varchar);





            }
            // ket thuc foreach (var row in batch)
            //========================================================================

            await writer.CompleteAsync();
            //========================================================================
        }
        // ket thuc private async Task BulkInsertHandoverAsync(NpgsqlConnection connection, List<HandoverRow> batch)
        //========================================================================
        // ket thuc phan doc gia tri file value ind off
        //========================================================================
        //************************************************************************
        //========================================================================


        // phan xu ly CR
        // create ra file xml
        // chay file o oSS
        // lay tu datatable 
        // bang crlogs

        //========================================================================
        // API: GENERATE XML
        //========================================================================
        /// <summary>
        /// Generate XML file từ CRlogs và return để download
        /// </summary>
        /// <param name="date">Ngày cần export (YYYY-MM-DD). Null = lấy ngày mới nhất</param>
        /// <returns>XML file</returns>
        [HttpGet("generate-active-CRxml")]
        public async Task<IActionResult> GenerateActiveCRXMLFile([FromQuery] string? date = null)
        {
            try
            {
                var (xmlContent, fileName) = await GenerateActiveCRXMLFileService(date);

                // Return file XML để download
                var bytes = System.Text.Encoding.UTF8.GetBytes(xmlContent);
                return File(bytes, "application/xml", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
        //========================================================================



        //========================================================================
        //========================================================================
        // HÀM: GENERATE XML - FTP DOWNLOAD TEMPLATE
        //========================================================================
        /// <summary>
        /// FTP download template từ OSS, query data từ CRlogs, tạo XML mới
        /// </summary>
        /// <param name="date">Ngày cần export (YYYY-MM-DD). Null = lấy ngày mới nhất</param>
        /// <returns>Tuple (xmlContent, fileName)</returns>


        private async Task<(string xmlContent, string fileName)> GenerateActiveCRXMLFileService(string? date = null)
        {

            try
            {
                // BƯỚC 1: FTP Download template từ OSS
                var ftpHost = "10.149.186.20";
                var ftpUser = "bvthem";
                var ftpPass = "Bk123456-";
                var ftpPort = 22;
                var templateFolder = "/d/oss/global/var/pm/shared/content3/scheduler/exportCustom/Schan/CDS/SRANPRBsLoadCells/PRBscellIndOffValueFileTemp/";
                //========================================================================

                XDocument templateDoc;
                using (var sftpClient = new SftpClient(ftpHost, ftpPort, ftpUser, ftpPass))
                // using (var ftpClient = new FtpClient(ftpHost, ftpUser, ftpPass, ftpPort))
                {
                    sftpClient.Connect(); // ← Không có async

                    // List tất cả file .xml trong folder
                    var files = sftpClient.ListDirectory(templateFolder); // ← Không có async
                    var xmlFiles = files.Where(f => f.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).ToList();
                    //========================================================================

                    if (xmlFiles.Count == 0) // ← Count là property, không phải method
                    {
                        throw new Exception($"No XML template file found in {templateFolder}");
                    }
                    //========================================================================

                    if (xmlFiles.Count > 1)
                    {
                        throw new Exception($"Multiple XML files found in {templateFolder}. Expected only 1 template file.");
                    }
                    //========================================================================

                    var templateFile = xmlFiles[0];
                    var templatePath = templateFile.FullName;
                    //========================================================================

                    // Download vào MemoryStream
                    using (var memoryStream = new MemoryStream())
                    {
                        // ftpClient.Download(memoryStream, templatePath); // ← Không có async
                        sftpClient.DownloadFile(templatePath, memoryStream);
                        memoryStream.Position = 0; // Reset position để đọc từ đầu

                        // Parse XML từ MemoryStream
                        templateDoc = XDocument.Load(memoryStream);
                    }
                    //========================================================================

                    sftpClient.Disconnect(); // ← Không có async
                }
                //========================================================================

                // BƯỚC 2: Query data từ database
                DateTime selectedDate;
                //========================================================================

                var _connectionString = _config.GetConnectionString("InformationProductionConnection");
                //========================================================================

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Nếu date NULL → lấy ngày mới nhất
                    if (string.IsNullOrEmpty(date))
                    {
                        var maxDateQuery = @"
                    SELECT MAX(period_start_time) 
                    FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsindoffvalueprocessdataCRlogs";

                        using (var cmd = new NpgsqlCommand(maxDateQuery, connection))
                        {
                            var result = await cmd.ExecuteScalarAsync();

                            if (result == null || result == DBNull.Value)
                            {
                                throw new Exception("No data found in CRlogs table");
                            }

                            selectedDate = (DateTime)result;
                        }
                    }
                    //========================================================================

                    else
                    {
                        selectedDate = DateTime.Parse(date);
                    }
                    //========================================================================

                    // Query data từ CRlogs
                    var query = @"
                SELECT 
                    dnMrbtsSran,
                    cell_ind_off_neigh_new,
                    nr_control,
                    operation
                FROM system_nsn_prbsloadcell.objtablekpiprbsloadcellsindoffvalueprocessdataCRlogs
                WHERE DATE(period_start_time) = @SelectedDate";

                    var records = new List<CellIndOffNeighValueImportCRFileClass>();
                    //========================================================================

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SelectedDate", selectedDate.Date);
                        //========================================================================

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var record = new CellIndOffNeighValueImportCRFileClass
                                {
                                    DnMrbtsSran = reader["dnMrbtsSran"]?.ToString() ?? "",
                                    CellIndOffNeighNew = reader["cell_ind_off_neigh_new"]?.ToString() ?? "",
                                    NrControl = reader["nr_control"]?.ToString() ?? "",
                                    Operation = reader["operation"]?.ToString() ?? ""
                                };
                                records.Add(record);
                            }
                            //========================================================================

                        }
                        //========================================================================

                    }
                    //========================================================================

                    if (!records.Any())
                    {
                        throw new Exception($"No data found for date: {selectedDate:yyyy-MM-dd}");
                    }
                    //========================================================================

                    // BƯỚC 3: Thay thế data vào XML template
                    var timestamp = DateTime.Now;
                    var fileName = $"SRAN_4G_cell_ind_off_{timestamp:yyyyMMdd}_{timestamp:HHmmss}_UPDATE.xml";
                    var planName = $"SRAN_4G_cell_ind_off_{timestamp:yyyyMMdd}_{timestamp:HHmmss}_UPDATE";
                    //========================================================================

                    // Lấy cmData element
                    // var cmData = templateDoc.Descendants("cmData").FirstOrDefault();
                    var cmData = templateDoc.Root?.Element("cmData");
                    if (cmData == null)
                    {
                        cmData = templateDoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "cmData");
                    }
                    //========================================================================


                    if (cmData == null)
                    {
                        throw new Exception("cmData element not found in template");
                    }

                    // Cập nhật attributes của cmData
                    cmData.SetAttributeValue("name", planName);
                    //========================================================================

                    // Cập nhật dateTime trong header
                    var logElement = cmData.Descendants("log").FirstOrDefault();
                    if (logElement != null)
                    {
                        logElement.SetAttributeValue("dateTime", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff+07:00"));
                    }
                    //========================================================================

                    // XÓA tất cả managedObject cũ
                    cmData.Descendants("managedObject").Remove();
                    //========================================================================

                    // THÊM managedObject mới từ database
                    foreach (var record in records)
                    {
                        var managedObject = new XElement("managedObject",
                            new XAttribute("class", "LNREL"),
                            new XAttribute("version", "FLF23R1_2221_11_2221_10"),
                            new XAttribute("distName", record.DnMrbtsSran),
                            new XAttribute("id", "54408173"),
                            new XAttribute("operation", "update"),

                            new XElement("p",
                                new XAttribute("name", "cellIndOffNeigh"),
                                record.CellIndOffNeighNew
                            ),
                            new XElement("p",
                                new XAttribute("name", "nrControl"),
                                "automatic"
                            )
                        );
                        //========================================================================

                        cmData.Add(managedObject);
                    }
                    //========================================================================


                    // BƯỚC 5: Lưu file XML vào folder local
                    // var timestamp = DateTime.Now;
                    // var subfolderName = timestamp.ToString("yyyyMMdd_HHmmss"); // Ví dụ: 20251107_143052
                    var subfolderName = timestamp.ToString("ddMMyyyy_HHmmss"); // Ví dụ: 20251107_143052
                    // Đường dẫn folder PRBscellIndOffValueFileCR (cùng thư mục với API)
                    // var basePath = Directory.GetCurrentDirectory(); // Thư mục chứa API
                    // var outputBaseFolder = Path.Combine(basePath, "PRBscellIndOffValueFileCR");
                    // Lấy đường dẫn thư mục chứa file Controller hiện tại
                    var currentFilePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                    var controllerFolder = Path.GetDirectoryName(currentFilePath);
                    var outputBaseFolder = Path.Combine(controllerFolder, "PRBscellIndOffValueFileCR");
                    var outputSubfolder = Path.Combine(outputBaseFolder, subfolderName);

                    // Tạo folder nếu chưa có
                    if (!Directory.Exists(outputSubfolder))
                    {
                        Directory.CreateDirectory(outputSubfolder);
                    }
                    //========================================================================

                    // Đường dẫn file đầy đủ
                    var localFilePath = Path.Combine(outputSubfolder, fileName);

                    // BƯỚC 4: Generate XML string
                    var xmlContent = templateDoc.ToString();
                    // await File.WriteAllTextAsync(localFilePath, xmlContent);
                    await System.IO.File.WriteAllTextAsync(localFilePath, xmlContent);
                    //========================================================================

                    return (xmlContent, fileName);
                }
                //========================================================================

            }
            //========================================================================

            catch (Exception ex)
            {
                throw new Exception($"GenerateXML failed: {ex.Message}", ex);
            }
            //========================================================================




        }

        //========================================================================
        // HELPER CLASS
        //========================================================================

        private class CellIndOffNeighValueImportCRFileClass
        {
            public string DnMrbtsSran { get; set; }
            public string CellIndOffNeighNew { get; set; }
            public string NrControl { get; set; }
            public string Operation { get; set; }
        }









    }
    // public class PRBsLoadCellFunctProcessingApiController : ControllerBase
    //========================================================================
}
