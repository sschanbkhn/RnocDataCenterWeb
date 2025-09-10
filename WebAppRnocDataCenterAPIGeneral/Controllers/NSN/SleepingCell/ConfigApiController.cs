using Microsoft.AspNetCore.Mvc;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Common;
using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{
    [ApiController]
    [Route("api/sleeping-cell/configuration")]
    [Produces("application/json")]
    public class ConfigApiController : ControllerBase
    {

        private readonly ConnectionsInformationSleepingCellDbContext _context;

        public ConfigApiController(ConnectionsInformationSleepingCellDbContext context)
        {
            _context = context;
        }

        // ===========================
        // 1. OUTLOOK - Email Configuration
        // ===========================
        [HttpGet("get-outlook")]
        public async Task<IActionResult> GetOutlookConfigs()
        {
            try
            {
                var configs = await _context.Outlooks.ToListAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("get-outlook/{id}")]
        public async Task<IActionResult> GetOutlookConfig(int id)
        {
            try
            {
                var config = await _context.Outlooks.FindAsync(id);
                if (config == null) return NotFound();
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("add-outlook")]
        // public async Task<IActionResult> CreateOutlookConfig([FromBody] OutlookDto dto)
        public async Task<IActionResult> CreateOutlookConfig([FromBody] Outlook config) // ✅ Entity trực tiếp

        {
            try
            {
                try
                {
                    _context.Outlooks.Add(config);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetOutlookConfig), new { id = config.Id }, config);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("update-outlook/{id}")]
        public async Task<IActionResult> UpdateOutlookConfig(int id, [FromBody] Outlook config)
        {
            try
            {
                var existingConfig = await _context.Outlooks.FindAsync(id);
                if (existingConfig == null) return NotFound();

                existingConfig.SttEmail = config.SttEmail;
                existingConfig.Email = config.Email;
                existingConfig.Password = config.Password;
                existingConfig.Active = config.Active;

                await _context.SaveChangesAsync();
                return Ok(existingConfig);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("destroy-outlook/{id}")]
        public async Task<IActionResult> DeleteOutlookConfig(int id)
        {
            try
            {
                var config = await _context.Outlooks.FindAsync(id);
                if (config == null) return NotFound();

                _context.Outlooks.Remove(config);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ===========================
        // 2. TABLEFILEPATH - File Path Management
        // ===========================
        [HttpGet("get-filepath")]
        public async Task<IActionResult> GetFilePathConfigs()
        {
            try
            {
                var configs = await _context.Tablefilepaths.ToListAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("get-filepath/{id}")]
        public async Task<IActionResult> GetFilePathConfig(int id)
        {
            try
            {
                var config = await _context.Tablefilepaths.FindAsync(id);
                if (config == null) return NotFound();
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("add-filepath")]
        public async Task<IActionResult> CreateFilePathConfig([FromBody] Tablefilepath config)
        {
            try
            {


                _context.Tablefilepaths.Add(config);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetOutlookConfig), new { id = config.Id }, config);


            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("update-filepath/{id}")]
        public async Task<IActionResult> UpdateFilePathConfig(int id, [FromBody] Tablefilepath config)
        {
            try
            {
                var existingConfig = await _context.Tablefilepaths.FindAsync(id);
                if (existingConfig == null) return NotFound();

                existingConfig.SttFilePath = config.SttFilePath;
                existingConfig.Oss = config.Oss;
                existingConfig.Username = config.Username;
                existingConfig.Password = config.Password;
                existingConfig.Host = config.Host;
                existingConfig.Port = config.Port;
                existingConfig.Filepath = config.Filepath;
                existingConfig.Protocol = config.Protocol;
                existingConfig.Active = config.Active;

                await _context.SaveChangesAsync();
                return Ok(existingConfig);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("delete-filepath/{id}")]
        public async Task<IActionResult> DeleteFilePathConfig(int id)
        {
            try
            {
                var config = await _context.Tablefilepaths.FindAsync(id);
                if (config == null) return NotFound();

                _context.Tablefilepaths.Remove(config);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ===========================
        // 3. MRBTS INFORMATION
        // ===========================
        [HttpGet("get-mrbts")]
        public async Task<IActionResult> GetMrbtsConfigs()
        {
            try
            {
                var configs = await _context.ObjtablemrbtsInfors.ToListAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("get-mrbts/{id}")]
        public async Task<IActionResult> GetMrbtsConfig(int id)
        {
            try
            {
                var config = await _context.ObjtablemrbtsInfors.FindAsync(id);
                if (config == null) return NotFound();
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("create-mrbts")]
        // public async Task<IActionResult> CreateMrbtsConfig([FromBody] MrbtsInforDto dto)
        public async Task<IActionResult> CreateMrbtsConfig([FromBody] ObjtablemrbtsInfor config)
        {
            try
            {
                // config_.CreatedAt = DateTime.UtcNow;
                _context.ObjtablemrbtsInfors.Add(config);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetMrbtsConfig), new { id = config.Id }, config);



            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("update-mrbts/{id}")]
        public async Task<IActionResult> UpdateMrbtsConfig(int id, [FromBody] ObjtablemrbtsInfor config)
        {
            try
            {
                var existingConfig = await _context.ObjtablemrbtsInfors.FindAsync(id);
                if (existingConfig == null) return NotFound();



                existingConfig.Stt = config.Stt;
                existingConfig.Mrbtsname = config.Mrbtsname;
                existingConfig.Oam = config.Oam;
                existingConfig.MrbtsId = config.MrbtsId;
                existingConfig.Enodebname = config.Enodebname;
                existingConfig.Note = config.Note;
                existingConfig.Reset = config.Reset;
                existingConfig.Blacklist = config.Blacklist;
                existingConfig.Vendor = config.Vendor;

                await _context.SaveChangesAsync();
                return Ok(existingConfig);


            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("delete-mrbts/{id}")]
        public async Task<IActionResult> DeleteMrbtsConfig(int id)
        {
            try
            {
                var config = await _context.ObjtablemrbtsInfors.FindAsync(id);
                if (config == null) return NotFound();

                _context.ObjtablemrbtsInfors.Remove(config);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ===========================
        // 4. RESET SITE LIMITS
        // ===========================
        [HttpGet("get-resetlimits")]
        public async Task<IActionResult> GetResetLimitsConfigs()
        {
            try
            {
                var configs = await _context.Objtableresetsitecountlimits.ToListAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("get-resetlimits/{id}")]
        public async Task<IActionResult> GetResetLimitsConfig(int id)
        {
            try
            {
                var config = await _context.Objtableresetsitecountlimits.FindAsync(id);
                if (config == null) return NotFound();
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("add-resetlimits")]
        public async Task<IActionResult> CreateResetLimitsConfig([FromBody] Objtableresetsitecountlimit config_)
        {
            try
            {
                config_.CreatedAt = DateTime.UtcNow;

                _context.Objtableresetsitecountlimits.Add(config_);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetResetLimitsConfig), new { id = config_.Id }, config_);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("update-resetlimits/{id}")]
        public async Task<IActionResult> UpdateResetLimitsConfig(int id, [FromBody] Objtableresetsitecountlimit config)
        {
            try
            {
                var existingConfig = await _context.Objtableresetsitecountlimits.FindAsync(id);
                if (existingConfig == null) return NotFound();

                existingConfig.LimitDate = config.LimitDate;
                existingConfig.MaxSitesPerDay = config.MaxSitesPerDay;
                existingConfig.SitesResetToday = config.SitesResetToday;
                existingConfig.LastResetTime = config.LastResetTime;
                existingConfig.MaxNsnSites = config.MaxNsnSites;
                existingConfig.MaxEricssonSites = config.MaxEricssonSites;
                existingConfig.MaxHuaweiSites = config.MaxHuaweiSites;
                existingConfig.NsnSitesReset = config.NsnSitesReset;
                existingConfig.EricssonSitesReset = config.EricssonSitesReset;
                existingConfig.HuaweiSitesReset = config.HuaweiSitesReset;
                existingConfig.AutoResetEnabled = config.AutoResetEnabled;
                existingConfig.EmergencyOverride = config.EmergencyOverride;

                existingConfig.UpdatedBy = config.UpdatedBy;
                existingConfig.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("delete-resetlimits/{id}")]
        public async Task<IActionResult> DeleteResetLimitsConfig(int id)
        {
            try
            {
                var config = await _context.Objtableresetsitecountlimits.FindAsync(id);
                if (config == null) return NotFound();

                _context.Objtableresetsitecountlimits.Remove(config);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ===========================
        // 5. SCHEDULER CONFIGURATION
        // ===========================
        [HttpGet("get-scheduler")]
        public async Task<IActionResult> GetSchedulerConfigs()
        {
            try
            {
                var configs = await _context.Objtableschedulers.ToListAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("get-scheduler/{id}")]
        public async Task<IActionResult> GetSchedulerConfig(int id)
        {
            try
            {
                var config = await _context.Objtableschedulers.FindAsync(id);
                if (config == null) return NotFound();
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("add-scheduler")]
        public async Task<IActionResult> CreateSchedulerConfig([FromBody] Objtablescheduler config)
        {
            try
            {



                _context.Objtableschedulers.Add(config);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetSchedulerConfig), new { id = config.Id }, config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("update-scheduler/{id}")]
        public async Task<IActionResult> UpdateSchedulerConfig(int id, [FromBody] Objtablescheduler config)
        {
            try
            {
                var existingConfig = await _context.Objtableschedulers.FindAsync(id);
                if (existingConfig == null) return NotFound();

                existingConfig.Sttschedulertime = config.Sttschedulertime;
                existingConfig.Starttime = config.Starttime;
                existingConfig.Endtime = config.Endtime;
                existingConfig.Active = config.Active;

                await _context.SaveChangesAsync();
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("delete-scheduler/{id}")]
        public async Task<IActionResult> DeleteSchedulerConfig(int id)
        {
            try
            {
                var config = await _context.Objtableschedulers.FindAsync(id);
                if (config == null) return NotFound();

                _context.Objtableschedulers.Remove(config);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ===========================
        // 6. database ACCOUNT SETTINGS
        // ===========================
        [HttpGet("get-database")]
        public async Task<IActionResult> GetAccountDatabaseConfigs()
        {
            try
            {
                var configs = await _context.Objtabledatabaseinfors.ToListAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("get-database/{id}")]
        public async Task<IActionResult> GetAccountDatabaseConfig(int id)
        {
            try
            {
                var config = await _context.Objtabledatabaseinfors.FindAsync(id);
                if (config == null) return NotFound();
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("add-database")]
        public async Task<IActionResult> CreateAccountDatabaseConfig([FromBody] Objtabledatabaseinfor config)
        {
            try
            {
                _context.Objtabledatabaseinfors.Add(config);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(CreateAccountDatabaseConfig), new { id = config.Id }, config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("Update-database/{id}")]
        public async Task<IActionResult> UpdateAccDatabaseConfig(int id, [FromBody] Objtabledatabaseinfor config)
        {
            try
            {
                var existingConfig = await _context.Objtabledatabaseinfors.FindAsync(id); // ✅ Database context
                if (existingConfig == null) return NotFound();

                existingConfig.SttDatabaseConfig = config.SttDatabaseConfig;
                existingConfig.ConnectionName = config.ConnectionName;
                existingConfig.Host = config.Host;
                existingConfig.Port = config.Port;
                existingConfig.DatabaseName = config.DatabaseName;
                existingConfig.Username = config.Username;
                existingConfig.Password = config.Password;
                existingConfig.SslMode = config.SslMode;
                existingConfig.TrustServerCertificate = config.TrustServerCertificate;

                await _context.SaveChangesAsync();
                return Ok(existingConfig);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("delete-database/{id}")]
        public async Task<IActionResult> DeleteAccountDatabaseConfig(int id)
        {
            try
            {
                var config = await _context.Objtabledatabaseinfors.FindAsync(id);
                if (config == null) return NotFound();

                _context.Objtabledatabaseinfors.Remove(config);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ===========================
        // 7. SSH ACCOUNT MANAGEMENT (Card mới)
        // ===========================
        [HttpGet("ssh-accounts")]
        public async Task<IActionResult> GetSshAccountsManagement()
        {
            try
            {
                // Có thể dùng cùng bảng nhưng với logic khác hoặc view khác
                var configs = await _context.Objtableaccountsshes
                    .Where(x => x.Active == true) // Ví dụ: chỉ lấy active accounts
                    .ToListAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ===========================
        // 8. ARCHIVE REPORTS
        // ===========================
        [HttpGet("archive-reports")]
        public async Task<IActionResult> GetArchiveReports([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.Objtable4gkpireportresultdetailarchives.AsQueryable();

                var totalRecords = await query.CountAsync();
                var reports = await query
                    .OrderByDescending(x => x.ArchivedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    data = reports,
                    totalRecords,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("archive-reports/{id}")]
        public async Task<IActionResult> GetArchiveReport(long id)
        {
            try
            {
                var report = await _context.Objtable4gkpireportresultdetailarchives.FindAsync(id);
                if (report == null) return NotFound();
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /*

        [HttpDelete("archive-reports/{id}")]
        public async Task<IActionResult> DeleteArchiveReport(long id)
        {
            try
            {
                var report = await _context.Objtable4gkpireportresultdetailarchives.FindAsync(id);
                if (report == null) return NotFound();

                _context.Objtable4gkpireportresultdetailarchives.Remove(report);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        */

        // ===========================
        // SEARCH ENDPOINTS
        // ===========================
        [HttpGet("search/{tableName}")]
        public async Task<IActionResult> SearchConfigs(string tableName, [FromQuery] string searchTerm = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                IQueryable<object> query = tableName.ToLower() switch
                {
                    "outlook" => _context.Outlooks.Where(x =>
                        string.IsNullOrEmpty(searchTerm) ||
                        x.Email.Contains(searchTerm)),
                    "filepath" => _context.Tablefilepaths.Where(x =>
                        string.IsNullOrEmpty(searchTerm) ||
                        x.Host.Contains(searchTerm) ||
                        x.Username.Contains(searchTerm)),
                    "mrbts" => _context.ObjtablemrbtsInfors.Where(x =>
                        string.IsNullOrEmpty(searchTerm) ||
                        x.Mrbtsname.Contains(searchTerm) ||
                        x.Enodebname.Contains(searchTerm)),
                    "resetlimits" => _context.Objtableresetsitecountlimits.Where(x =>
                        string.IsNullOrEmpty(searchTerm) ||
                        x.UpdatedBy.Contains(searchTerm)),
                    "scheduler" => _context.Objtableschedulers.Where(x =>
                        string.IsNullOrEmpty(searchTerm) ||
                        x.Starttime.Contains(searchTerm) ||
                        x.Endtime.Contains(searchTerm)),
                    "ssh" => _context.Objtableaccountsshes.Where(x =>
                        string.IsNullOrEmpty(searchTerm) ||
                        x.System.Contains(searchTerm) ||
                        x.Usename.Contains(searchTerm)),
                    "archive-reports" => _context.Objtable4gkpireportresultdetailarchives.Where(x =>
                        string.IsNullOrEmpty(searchTerm) ||
                        x.MrbtsName.Contains(searchTerm) ||
                        x.Province.Contains(searchTerm)),
                    _ => throw new ArgumentException("Invalid table name")
                };

                var totalRecords = await query.CountAsync();
                var results = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    data = results,
                    totalRecords,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }






    }

}