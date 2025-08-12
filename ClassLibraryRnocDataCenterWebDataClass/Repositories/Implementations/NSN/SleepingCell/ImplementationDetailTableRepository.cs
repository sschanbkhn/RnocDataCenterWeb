using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationDetailTableRepository : InterfaceDetailTableRepository
    {
        private readonly ConnectionsInformationSleepingCellDbContext _context;

        public ImplementationDetailTableRepository(ConnectionsInformationSleepingCellDbContext context)
        {
            _context = context;
        }

        public async Task<Objtable4gkpireportresultdetail> GetOrCreateDetailRecordAsync(string cellName)
        {
            try
            {
                // Try to get existing detail record
                var existing = await _context.Objtable4gkpireportresultdetails
                    .FirstOrDefaultAsync(x => x.LncelName == cellName);

                if (existing != null)
                {
                    return existing;
                }

                // Get from result table to copy data
                /// var resultRecord = await _context.Objtable4gkpireportresult
                /// 
                var filterRecord = await _context.Objtablefilterltekpireports
                    .FirstOrDefaultAsync(x => x.LncelName == cellName);

                if (filterRecord == null)
                {
                    throw new Exception($"Cell {cellName} not found in result table");
                }



                // ✅ THÊM TRƯỚC KHI TẠO DETAIL RECORD:
                var mrbtsInfo = await _context.ObjtablemrbtsInfors
                    .Where(x => x.Enodebname == filterRecord.LnbtsName)
                    .FirstOrDefaultAsync();

                // 		filterRecord.MrbtsName	"MRBTS-4G-SMA009M-SLA"	string



                // Create new detail record copying from result table
                var detailRecord = new Objtable4gkpireportresultdetail
                {
                    Id = filterRecord.Id,
                    // OriginalId = filterRecord.OriginalId,
                    OriginalId = filterRecord.Id,
                    PeriodStartTime = filterRecord.PeriodStartTime,
                    MrbtsName = filterRecord.MrbtsName,
                    LnbtsName = filterRecord.LnbtsName,
                    LncelName = filterRecord.LncelName,
                    DnMrbtsSite = filterRecord.DnMrbtsSite,
                    PdcpVolumeDl = filterRecord.PdcpVolumeDl,
                    PdcpVolumeUl = filterRecord.PdcpVolumeUl,
                    CellAvail = filterRecord.CellAvail,
                    MaxUes = filterRecord.MaxUes,
                    /// MaxPdcpDl = filterRecord.MaxPdcpDl,
                    /// MaxPdcpUl = filterRecord.MaxPdcpUl,
                    /// 

                    // Cast decimal to long
                    MaxPdcpDl = filterRecord.MaxPdcpDl != null ? (long?)Convert.ToInt64(filterRecord.MaxPdcpDl) : null,
                    MaxPdcpUl = filterRecord.MaxPdcpUl != null ? (long?)Convert.ToInt64(filterRecord.MaxPdcpUl) : null,



                    // 2. PROVINCE/DISTRICT/REGION - Extract từ MrbtsName
                    Province = ExtractProvinceFromMrbtsName(filterRecord.MrbtsName),
                    District = ExtractDistrictFromMrbtsName(filterRecord.LnbtsName),
                    Region = MapProvinceToRegion(ExtractProvinceFromMrbtsName(filterRecord.MrbtsName)),


                    // 3. TIME FIELDS - Tính từ current time hoặc period_start_time
                    /// DataDate = ParseDateFromPeriodStartTime(filterRecord.PeriodStartTime),

                    /// DataDate = DateOnly.FromDateTime(ParseDateFromPeriodStartTime(filterRecord.PeriodStartTime)),
                    ///
                    ///


                    DataDate = DateOnly.FromDateTime(ParseDateFromPeriodStartTime(filterRecord.PeriodStartTime)),


                    DataYear = ParseDateFromPeriodStartTime(filterRecord.PeriodStartTime).Year,
                    DataMonth = ParseDateFromPeriodStartTime(filterRecord.PeriodStartTime).Month,
                    DataDay = ParseDateFromPeriodStartTime(filterRecord.PeriodStartTime).Day,
                    DataQuarter = GetQuarterFromDate(ParseDateFromPeriodStartTime(filterRecord.PeriodStartTime)),
                    DataWeek = GetWeekOfYear(ParseDateFromPeriodStartTime(filterRecord.PeriodStartTime)),






                    /// Vendor = filterRecord.Vendor,
                    /// // 4. VENDOR - Extract từ MrbtsName pattern
                    Vendor = ExtractVendorFromMrbtsName(filterRecord.MrbtsName),

                    ArchivedAt = DateTime.Now,
                    /// OriginalCreatedAt = filterRecord.OriginalCreatedAt,
                    /// 
                    OriginalCreatedAt = filterRecord.CreatedAt ?? DateTime.Now,
                    ArchivedBy = "SYSTEM_N8N_RESET",



                    // ✅ THÊM DÒNG NÀY:
                    /// MrbtsInforId = (int) mrbtsInfo?.MrbtsId,
                    /// MrbtsInforId = mrbtsInfo?.MrbtsId,
                    // ✅ SIMPLE AND WORKING:

                

                    // ✅ THÊM USER_NOTES LUÔN:
                    /// UserNotes = mrbtsInfo?.Note,
                    /// 
                    /// 

                    /// UserNotes = mrbtsInfo != null ? mrbtsInfo.Note : $"MRBTS not found: {filterRecord.MrbtsName}",
                    UserNotes = mrbtsInfo != null ? mrbtsInfo.Note : $"MRBTS not found: {filterRecord.MrbtsName}",

                    //
                    ResetPermission = mrbtsInfo?.Reset,

                    // Initialize tracking fields
                    ResetCount = 0,
                    ResetEnabled = true,
                    ExecutionStatus = "not_started",
                    LastSyncedAt = DateTime.Now
                };

                _context.Objtable4gkpireportresultdetails.Add(detailRecord);
                await _context.SaveChangesAsync();

                return detailRecord;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating detail record for {cellName}: {ex.Message}", ex);
            }

            


        }
        // ket thuc ham public async Task<Objtable4gkpireportresultdetail> GetOrCreateDetailRecordAsync(string cellName)


        public async Task<Objtable4gkpireportresult> CreateResultFromDetailAsync(long detailId)
        {
            try
            {
                // 1. Lấy record từ detail table
                var detailRecord = await _context.Objtable4gkpireportresultdetails
                    .FirstOrDefaultAsync(x => x.Id == detailId);

                if (detailRecord == null)
                {
                    throw new Exception($"Detail record with ID {detailId} not found");
                }

                // 2. Copy tất cả 26 cột trùng tên
                var resultRecord = new Objtable4gkpireportresult
                {
                    Id = detailRecord.Id,
                    OriginalId = detailRecord.OriginalId ?? 0,
                    PeriodStartTime = detailRecord.PeriodStartTime,
                    MrbtsName = detailRecord.MrbtsName,
                    LnbtsName = detailRecord.LnbtsName,
                    LncelName = detailRecord.LncelName,
                    DnMrbtsSite = detailRecord.DnMrbtsSite,
                    PdcpVolumeDl = detailRecord.PdcpVolumeDl,
                    PdcpVolumeUl = detailRecord.PdcpVolumeUl,
                    CellAvail = detailRecord.CellAvail,
                    MaxUes = detailRecord.MaxUes,
                    MaxPdcpDl = detailRecord.MaxPdcpDl,
                    MaxPdcpUl = detailRecord.MaxPdcpUl,
                    Province = detailRecord.Province,
                    District = detailRecord.District,
                    Region = detailRecord.Region,
                    // DataDate = detailRecord.DataDate,
                    DataDate = detailRecord.DataDate ?? DateOnly.FromDateTime(DateTime.Today),
                    DataYear = detailRecord.DataYear,
                    DataMonth = detailRecord.DataMonth,
                    DataDay = detailRecord.DataDay,
                    DataQuarter = detailRecord.DataQuarter,
                    DataWeek = detailRecord.DataWeek,
                    Vendor = detailRecord.Vendor,
                    ArchivedAt = detailRecord.ArchivedAt,
                    OriginalCreatedAt = detailRecord.OriginalCreatedAt,
                    ArchivedBy = detailRecord.ArchivedBy,
                    ExecutionStatus = detailRecord.ExecutionStatus
                };

                _context.Objtable4gkpireportresults.Add(resultRecord);
                await _context.SaveChangesAsync();

                return resultRecord;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating result record: {ex.Message}", ex);
            }
        }
        // ket thuc ham public async Task<Objtable4gkpireportresult> CreateResultFromDetailAsync(long detailId)





        public async Task<Objtable4gkpireportresultarchive> ArchiveResultRecordAsync(long resultId)
        {
            try
            {
                // 1. Lấy record từ result table
                var resultRecord = await _context.Objtable4gkpireportresults
                    .FirstOrDefaultAsync(x => x.Id == resultId);

                if (resultRecord == null)
                {
                    throw new Exception($"Result record with ID {resultId} not found");
                }

                // 2. Copy tất cả 26 cột sang archive table
                var archiveRecord = new Objtable4gkpireportresultarchive
                {
                    // Id = resultRecord.Id, ← Bỏ dòng này, de tu dong
                    OriginalId = resultRecord.OriginalId,
                    PeriodStartTime = resultRecord.PeriodStartTime,
                    MrbtsName = resultRecord.MrbtsName,
                    LnbtsName = resultRecord.LnbtsName,
                    LncelName = resultRecord.LncelName,
                    DnMrbtsSite = resultRecord.DnMrbtsSite,
                    PdcpVolumeDl = resultRecord.PdcpVolumeDl,
                    PdcpVolumeUl = resultRecord.PdcpVolumeUl,
                    CellAvail = resultRecord.CellAvail,
                    MaxUes = resultRecord.MaxUes,
                    MaxPdcpDl = resultRecord.MaxPdcpDl,
                    MaxPdcpUl = resultRecord.MaxPdcpUl,
                    Province = resultRecord.Province,
                    District = resultRecord.District,
                    Region = resultRecord.Region,
                    DataDate = resultRecord.DataDate,
                    DataYear = resultRecord.DataYear,
                    DataMonth = resultRecord.DataMonth,
                    DataDay = resultRecord.DataDay,
                    DataQuarter = resultRecord.DataQuarter,
                    DataWeek = resultRecord.DataWeek,
                    Vendor = resultRecord.Vendor,
                    ArchivedAt = DateTime.Now, // Update timestamp khi archive
                    OriginalCreatedAt = resultRecord.OriginalCreatedAt,
                    ArchivedBy = "SYSTEM_ARCHIVE", // Hoặc current user
                    ExecutionStatus = resultRecord.ExecutionStatus
                };

                _context.Objtable4gkpireportresultarchives.Add(archiveRecord);
                await _context.SaveChangesAsync();

                return archiveRecord;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error archiving result record: {ex.Message}", ex);
            }
        }
        // ket thuc ham public async Task<Objtable4gkpireportresultarchive> ArchiveResultRecordAsync(long resultId)




        public async Task<Objtable4gkpireportresultdetailarchive> ArchiveDetailRecordAsync(long detailId)
        {
            try
            {
                // 1. Lấy record từ detail table
                var detailRecord = await _context.Objtable4gkpireportresultdetails
                    .FirstOrDefaultAsync(x => x.Id == detailId);

                if (detailRecord == null)
                {
                    throw new Exception($"Detail record with ID {detailId} not found");
                }

                // 2. Copy tất cả fields sang archive table (detail archive có 56 columns)
                var archiveRecord = new Objtable4gkpireportresultdetailarchive
                {
                    Id = detailRecord.Id,
                    OriginalId = detailRecord.OriginalId,
                    PeriodStartTime = detailRecord.PeriodStartTime,
                    MrbtsName = detailRecord.MrbtsName,
                    LnbtsName = detailRecord.LnbtsName,
                    LncelName = detailRecord.LncelName,
                    DnMrbtsSite = detailRecord.DnMrbtsSite,
                    PdcpVolumeDl = detailRecord.PdcpVolumeDl,
                    PdcpVolumeUl = detailRecord.PdcpVolumeUl,
                    CellAvail = detailRecord.CellAvail,
                    MaxUes = detailRecord.MaxUes,
                    MaxPdcpDl = detailRecord.MaxPdcpDl,
                    MaxPdcpUl = detailRecord.MaxPdcpUl,
                    Province = detailRecord.Province,
                    District = detailRecord.District,
                    Region = detailRecord.Region,
                    DataDate = detailRecord.DataDate ?? DateOnly.FromDateTime(DateTime.Today),
                    DataYear = detailRecord.DataYear,
                    DataMonth = detailRecord.DataMonth,
                    DataDay = detailRecord.DataDay,
                    DataQuarter = detailRecord.DataQuarter,
                    DataWeek = detailRecord.DataWeek,
                    Vendor = detailRecord.Vendor,
                    ArchivedAt = DateTime.Now, // Update archive timestamp
                    OriginalCreatedAt = detailRecord.OriginalCreatedAt,
                    ArchivedBy = "SYSTEM_ARCHIVE",
                    ActionBlacklist = detailRecord.ActionBlacklist,
                    UserNotes = detailRecord.UserNotes,
                    ResetPermission = detailRecord.ResetPermission,
                    MrbtsInforId = detailRecord.MrbtsInforId,
                    LastSyncedAt = detailRecord.LastSyncedAt,
                    ResetCount = detailRecord.ResetCount,
                    LastResetAt = detailRecord.LastResetAt,
                    LastResetBy = detailRecord.LastResetBy,
                    LastResetSuccess = detailRecord.LastResetSuccess,
                    TotalSuccessRate = detailRecord.TotalSuccessRate,
                    ResetEnabled = detailRecord.ResetEnabled,
                    ResetHistory = detailRecord.ResetHistory,
                    ExecutionNotes = detailRecord.ExecutionNotes,
                    ExecutionStatus = detailRecord.ExecutionStatus,
                    ExecutionStartedAt = detailRecord.ExecutionStartedAt,
                    ExecutionCompletedAt = detailRecord.ExecutionCompletedAt,
                    ExecutionDuration = detailRecord.ExecutionDuration,
                    SshHost = detailRecord.SshHost,
                    SshConnectionStatus = detailRecord.SshConnectionStatus,
                    ExecutionLog = detailRecord.ExecutionLog,
                    PingTestBefore = detailRecord.PingTestBefore,
                    PingTestAfter = detailRecord.PingTestAfter,
                    SshConnectStartedAt = detailRecord.SshConnectStartedAt,
                    SshConnectCompletedAt = detailRecord.SshConnectCompletedAt,
                    CommandSentAt = detailRecord.CommandSentAt,
                    CommandResponseReceivedAt = detailRecord.CommandResponseReceivedAt
                };

                _context.Objtable4gkpireportresultdetailarchives.Add(archiveRecord);
                await _context.SaveChangesAsync();

                return archiveRecord;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error archiving detail record: {ex.Message}", ex);
            }
        }



        public async Task UpdateExecutionStatusAsync(long detailId, string status)
        {
            try
            {
                var record = await _context.Objtable4gkpireportresultdetails.FindAsync(detailId);
                if (record != null)
                {
                    record.ExecutionStatus = status;
                    record.ExecutionStartedAt = DateTime.Now;
                    record.LastSyncedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating execution status: {ex.Message}", ex);
            }
        }

        public async Task UpdateSshConnectionAsync(long detailId, string status, string host)
        {
            try
            {
                /// var record = await _context.Objtable4gkpireportresultdetail.FindAsync(detailId);
                var record = await _context.Objtable4gkpireportresultdetails
            .FirstOrDefaultAsync(x => x.Id == detailId);

                if (record != null)
                {
                    record.SshConnectionStatus = status;
                    record.SshHost = host;
                    record.SshConnectStartedAt = DateTime.Now;
                    record.LastSyncedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating SSH connection: {ex.Message}", ex);
            }
        }

        /// public async Task UpdateResetResultAsync(long detailId, bool success, string output, string executedBy, bool boolPingTestBefore)
        public async Task UpdateResetResultAsync(long detailId, bool success, string output, string executedBy, Dictionary<string, object> updatecellResult)
        {
            try
            {
                var record = await _context.Objtable4gkpireportresultdetails.FindAsync(detailId);
                if (record != null)
                {
                    record.ResetCount = (record.ResetCount ?? 0) + 1;
                    record.LastResetAt = DateTime.Now;
                    record.LastResetBy = executedBy;
                    record.LastResetSuccess = success;
                    record.ExecutionStatus = success ? "completed" : "failed";
                    record.ExecutionCompletedAt = DateTime.Now;
                    record.LastSyncedAt = DateTime.Now;

                    record.PingTestBefore = (bool) updatecellResult["PingTestBefore"];
                    record.SshConnectionStatus = (string) updatecellResult["SshConnectionStatus"];
                    record.SshHost = (string)updatecellResult["SshHost"];
                    record.SshConnectStartedAt = (DateTime)updatecellResult["SshConnectStartedAt"];
                    record.SshConnectCompletedAt = (DateTime)updatecellResult["SshConnectCompletedAt"];
                    record.CommandSentAt = (DateTime)updatecellResult["CommandSentAt"];
                    
                    record.CommandResponseReceivedAt = (DateTime)updatecellResult["CommandResponseReceivedAt"];

                    // Update execution notes
                    record.ExecutionNotes = success ?
                        $"Reset successful: {output}" :
                        $"Reset failed: {output}";

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating reset result: {ex.Message}", ex);
            }
        }

        public async Task UpdateExecutionLogAsync(long detailId, string logData)
        {
            try
            {
                var record = await _context.Objtable4gkpireportresultdetails.FindAsync(detailId);
                if (record != null)
                {
                    record.ExecutionLog = logData;
                    record.LastSyncedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating execution log: {ex.Message}", ex);
            }
        }



        public async Task UpdateSshTimingAsync(int detailId, DateTime connectStarted, DateTime connectCompleted)
        {
            try
            {
                var record = await _context.Objtable4gkpireportresultdetails
                    .FirstOrDefaultAsync(x => x.Id == detailId);

                if (record != null)
                {
                    record.SshConnectStartedAt = connectStarted;
                    record.SshConnectCompletedAt = connectCompleted;
                    record.LastSyncedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating SSH timing for detail {detailId}: {ex.Message}", ex);
            }
        }




        private string ExtractProvinceFromMrbtsName(string mrbtsName)
        {
            // Logic extract province từ MRBTS name
            // VD: "MRBTS-4G-SMA009M-SLA" -> "SLA"
            if (string.IsNullOrEmpty(mrbtsName)) return "Unknown";

            // var parts = mrbtsName.Split('-');
            // return parts.Length >= 4 ? parts[3] : "Unknown";
            return mrbtsName.Substring(mrbtsName.Length - 3, 3);
        }

        private string ExtractDistrictFromMrbtsName(string mrbtsName)
        {
            // Logic extract district từ MRBTS name
            // VD: "4G-SMA009M-SLA" -> "SMA009M"
            if (string.IsNullOrEmpty(mrbtsName)) return "Unknown";

            var parts = mrbtsName.Split('-');
            return parts.Length >= 2 ? parts[1].Substring(0, 3) : "Unknown";
        }

        private string MapProvinceToRegion(string province)
        {
            // Map province to region
            return province switch
            {
                "HNI" or "DBN" or "LCU" => "KV1",
                "LCI" or "LSN" or "HGG" => "KV1",
                "CBG" or "SLA" or "QNH" or "HPG" => "KV1",
                _ => "Unknown"
            };
        }

        private DateTime ParseDateFromPeriodStartTime(string periodStartTime)
        {
            // Parse date từ period_start_time string
            if (DateTime.TryParse(periodStartTime, out var date))
                return date;

            return DateTime.Now;
        }

        private int GetQuarterFromDate(DateTime date)
        {
            return ((date.Month - 1) / 3) + 1;
        }

        private int GetWeekOfYear(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date,
                culture.DateTimeFormat.CalendarWeekRule,
                culture.DateTimeFormat.FirstDayOfWeek);
        }

        private string ExtractVendorFromMrbtsName(string mrbtsName)
        {
            // Extract vendor từ MRBTS name pattern
            // Default NSN for this system
            return "NSN";
        }




        public async Task UpdatePingTestAsync(int detailId, bool pingBefore, bool pingAfter)
        {
            try
            {
                var record = await _context.Objtable4gkpireportresultdetails
                    .FirstOrDefaultAsync(x => x.Id == detailId);

                if (record != null)
                {
                    record.PingTestBefore = pingBefore;
                    record.PingTestAfter = pingAfter;
                    record.LastSyncedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating ping test for detail {detailId}: {ex.Message}", ex);
            }
        }









    }
}