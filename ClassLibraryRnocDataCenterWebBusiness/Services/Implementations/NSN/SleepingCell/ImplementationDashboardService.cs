




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;





namespace ClassLibraryRnocDataCenterWebBusiness.Services.Implementations.NSN.SleepingCell
{
    public class ImplementationDashboardService : InterfaceDashboardService
    {
        private readonly InterfaceSleepingCellService _sleepingCellService;
        private readonly InterfaceSleepingCellResultRepository _resultRepository;
        private readonly InterfaceDashboardRepository _dashboardRepository;
        // private readonly ConnectionsInformationSleepingCellDbContext _context;
        private readonly ConnectionsInformationSleepingCellDbContext _context;


        public ImplementationDashboardService(
            InterfaceSleepingCellService sleepingCellService,
            InterfaceSleepingCellResultRepository resultRepository,
            InterfaceDashboardRepository dashboardRepository,
            ConnectionsInformationSleepingCellDbContext context
            )
        {
            _sleepingCellService = sleepingCellService;
            _resultRepository = resultRepository;
            _dashboardRepository = dashboardRepository;
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var sleepingCellStats = await _sleepingCellService.GetSleepingCellStatsAsync();
            var networkHealth = await GetNetworkHealthAsync();
            var activeAlerts = await GetActiveAlertsAsync();

            return new DashboardSummaryDto
            {
                SleepingCellStats = sleepingCellStats,
                ActiveAlerts = activeAlerts,
                NetworkHealth = networkHealth,
                LastUpdated = DateTime.UtcNow,
                SystemStatus = true
            };
        }

        public async Task<NetworkHealthDto> GetNetworkHealthAsync()
        {
            return await Task.FromResult(new NetworkHealthDto
            {
                OverallHealthScore = 85.5m,
                VendorHealthScores = new Dictionary<string, decimal>
                {
                    { "NSN", 90.0m },
                    { "Ericsson", 80.0m },
                    { "Huawei", 85.0m }
                },
                ProvinceHealthScores = new Dictionary<string, decimal>
                {
                    { "HNI", 88.0m },
                    { "HCM", 92.0m },
                    { "DNA", 85.0m }
                },
                CriticalIssues = new List<string>(),
                LastCalculated = DateTime.UtcNow
            });
        }

        public async Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate)
        {
            return await Task.FromResult(new PerformanceMetricsDto
            {
                TotalResetOperations = 150,
                SuccessRate = 95.5m,
                AverageResetTime = TimeSpan.FromMinutes(2.5),
                ResetsByVendor = new Dictionary<string, int>
                {
                    { "NSN", 80 },
                    { "Ericsson", 45 },
                    { "Huawei", 25 }
                },
                ResetsByProvince = new Dictionary<string, int>
                {
                    { "HNI", 60 },
                    { "HCM", 50 },
                    { "DNA", 40 }
                },
                MetricsPeriodStart = fromDate,
                MetricsPeriodEnd = toDate
            });
        }

        public async Task<TrendAnalysisDto> GetTrendAnalysisAsync(int days = 30)
        {
            return await Task.FromResult(new TrendAnalysisDto
            {
                DailyTrends = new Dictionary<string, decimal>
                {
                    { "2025-07-15", 45.2m },
                    { "2025-07-14", 42.8m },
                    { "2025-07-13", 48.1m }
                },
                TrendDirection = "Improving",
                ChangePercentage = 5.5m,
                AnalysisDate = DateTime.UtcNow
            });
        }

        public async Task<IEnumerable<AlertDto>> GetActiveAlertsAsync()
        {
            return await Task.FromResult(new List<AlertDto>
            {
                new AlertDto
                {
                    Id = "ALT001",
                    Type = "HIGH_SLEEPING_CELL_COUNT",
                    Title = "High Sleeping Cell Count",
                    Message = "Sleeping cell count exceeded threshold in HCM region",
                    Severity = "WARNING",
                    Source = "System Monitor",
                    CreatedTime = DateTime.UtcNow.AddHours(-2),
                    IsAcknowledged = false
                }
            });
        }

        public async Task<IEnumerable<AlertDto>> GetAlertHistoryAsync(int days = 7)
        {
            return await Task.FromResult(new List<AlertDto>());
        }

        public async Task<string> GenerateReportAsync(ReportRequestDto request)
        {
            return await Task.FromResult($"Report generated for {request.ReportType} from {request.FromDate} to {request.ToDate}");
        }

        public async Task<byte[]> GenerateReportFileAsync(ReportRequestDto request)
        {
            var reportContent = await GenerateReportAsync(request);
            return await Task.FromResult(System.Text.Encoding.UTF8.GetBytes(reportContent));
        }

        public async Task<SleepingCellStatsDto> GetRealTimeStatsAsync()
        {
            return await _sleepingCellService.GetSleepingCellStatsAsync();
        }

        public async Task<Dictionary<string, object>> GetSystemStatusAsync()
        {
            return await Task.FromResult(new Dictionary<string, object>
            {
                { "Status", "Healthy" },
                { "Uptime", "99.9%" },
                { "LastCheck", DateTime.UtcNow },
                { "ActiveConnections", 25 }
            });
        }

        public async Task<IEnumerable<string>> GetSystemIssuesAsync()
        {
            return await Task.FromResult(new List<string>());
        }

        public async Task<bool> IsSystemHealthyAsync()
        {
            return await Task.FromResult(true);
        }

        // phan tren chua dung






        public async Task<Zone1Response> GetSummaryAsync(DateOnly selectedDate)
        {



            var isCurrentDate = selectedDate == DateOnly.FromDateTime(DateTime.Today);

            // Today's Analysis Count
            var todayAnalysis = await GetTodayAnalysisCount(selectedDate, isCurrentDate);

            // Sleeping Cells Count  
            var sleepingCells = await GetSleepingCellsCount(selectedDate, isCurrentDate);

            // Sleeping Cells Count  
            var processCells_ = await GetProcessing_CellsCount(selectedDate, isCurrentDate);


            // Execution Cells Count
            var executionCells = await GetExecutionCellsCount(selectedDate);

            // Recheck Cells Count
            var recheckCells = await GetRecheckCellsCount(selectedDate);

            return new Zone1Response
            {
                Date = selectedDate.ToString("yyyy-MM-dd"),
                TodayAnalysis = todayAnalysis,
                SleepingCells = sleepingCells,
                ExecutionCells = executionCells,
                RecheckCells = recheckCells,
                ProcessCells_ = processCells_
            };
        }








        private async Task<int> GetTodayAnalysisCount(DateOnly selectedDate, bool isCurrentDate)
        {
            // var dateString = selectedDate.ToString("yyyy-MM-dd");
            var dateString = selectedDate.ToString("MM.dd.yyyy");  // "07.29.2025"

            // var count = await _context.Objtable4gkpireportresultdetailarchives  // Thêm 's'
            var count = await _context.Objtable4gkpireportlogs  // Thêm 's'  
            .Where(x => x.PeriodStartTime.Contains(dateString))
                .CountAsync();

            return count;
        }

        private async Task<int> GetSleepingCellsCount(DateOnly selectedDate, bool isCurrentDate)
        {


            var dateString = selectedDate.ToString("MM.dd.yyyy");  // "07.29.2025"

            // var count = await _context.Objtablefilterltekpireportstores
            /*
            var count = await _context.Objtable4gkpireportresultdetailarchives  // Thêm 's'
                .Where(x => x.PeriodStartTime == dateString)
                .CountAsync();

            return count;

            */

            var _count_ = await _context.Objtablefilterltekpireportstores  // Thêm 's'
                .Where(x => x.PeriodStartTime == dateString)
                .CountAsync();

            return _count_;


        }

        private async Task<int> GetProcessing_CellsCount(DateOnly selectedDate, bool isCurrentDate)
        {


            var dateString = selectedDate.ToString("MM.dd.yyyy");  // "07.29.2025"

            // var count = await _context.Objtablefilterltekpireportstores
            var count = await _context.Objtable4gkpireportresultdetailarchives  // Thêm 's'
                .Where(x => x.PeriodStartTime == dateString)
                .CountAsync();

            return count;


        }

        private async Task<int> GetExecutionCellsCount(DateOnly selectedDate)
        {
            var count = await _context.Objtable4gkpireportresultdetailarchives  // Thêm 's'
                .Where(x => x.DataDate == selectedDate)
                .Where(x => x.ExecutionStatus == "completed")
                .CountAsync();

            return count;
        }



        private async Task<int> GetRecheckCellsCount(DateOnly selectedDate)
        {
            var count = await _context.Objtable4gkpireportresultdetailarchives  // Thêm 's'
                .Where(x => x.DataDate == selectedDate)
                .Where(x => (x.ExecutionStatus == "failed") || (x.ExecutionStatus == "NULL") || (x.ExecutionStatus == null) || (x.ExecutionStatus == "starting_reset"))
                .CountAsync();

            return count;
        }


        // public async Task<ActionResult> funDashboardServiceGetProvinceSummary()
        // public async Task<object> funDashboardServiceGetProvinceSummary()  // object thay vì ActionResult
        public async Task<object> funDashboardServiceGetProvinceSummary(string date)  // Thêm date parameter
        {
            // Parse date
            var targetDate = DateOnly.Parse(date);

            try
            {
                // Lấy data từ bảng có province field
                var provinceData = await _context.Objtable4gkpireportresultdetailarchives
                    .Where(x => !string.IsNullOrEmpty(x.Province) &&
                       x.DataDate.HasValue &&
                       x.DataDate.Value == targetDate)  // ✅ Filter theo ngày
                    .GroupBy(x => x.Province)
                    .Select(g => new {
                        Province = g.Key,
                        TotalCells = g.Count(),
                        SuccessfulCells = g.Count(x => x.LastResetSuccess == true),
                        ProcessedCells = g.Count(x => !string.IsNullOrEmpty(x.ExecutionStatus)),
                        Districts = g.Select(x => x.District).Distinct().Count()
                    })
                    .ToListAsync();

                // Tính tổng districts từ tất cả provinces
                /*
                var totalDistricts = await _context.Objtable4gkpireportresultdetailarchives
                    .Where(x => !string.IsNullOrEmpty(x.District))
                    .Select(x => x.District)
                    .Distinct()
                    .CountAsync();
                */

                // ✅ SỬA: Tính tổng districts CHỈ từ ngày được chọn, không phải toàn bộ database
                var totalDistricts = await _context.Objtable4gkpireportresultdetailarchives
                    .Where(x => !string.IsNullOrEmpty(x.District) &&
                               x.DataDate.HasValue &&
                               x.DataDate.Value == targetDate)  // ✅ THÊM filter theo ngày
                    .Select(x => x.District)
                    .Distinct()
                    .CountAsync();


                // Tính average success rate
                var totalProcessed = provinceData.Sum(x => x.ProcessedCells);
                var totalSuccess = provinceData.Sum(x => x.SuccessfulCells);
                var avgSuccessRate = totalProcessed > 0 ? Math.Round((double)totalSuccess / totalProcessed * 100, 1) : 0;

                return new
                {
                    success = true,
                    totalProvinces = provinceData.Count,
                    totalDistricts = totalDistricts,
                    avgSuccessRate = avgSuccessRate,
                    data = provinceData
                };
            }
            catch (Exception ex)
            {


                // Service throw exception hoặc return error object
                return new
                {
                    success = false,
                    message = "Error retrieving province summary",
                    error = ex.Message
                };


            }


        }
        // ket thuc ham public async Task<object> funDashboardServiceGetProvinceSummary(string date)  // Thêm date parameter




        public async Task<object> funDashboardServiceGetZone4SummaryAsync(string date)
        {
            var targetDate = DateOnly.Parse(date);
            var dateString = targetDate.ToString("MM.dd.yyyy"); // "08.07.2025"

            try
            {
                // 1. Get Total Cells từ objtable4gkpireportlogs (theo PeriodStartTime)
                // var totalCellsData = _context.Objtable4gkpireportlogs
                // var totalTask = _context.Objtable4gkpireportlogs
                var totalRaw = await _context.Objtable4gkpireportlogs
        .Where(x => x.PeriodStartTime.Contains(dateString) &&
           !string.IsNullOrEmpty(x.LnbtsName))
        .ToListAsync(); // ← Chỉ ToListAsync thôi, bỏ AsEnumerable + GroupBy


                // 2. Get Sleeping Cells từ objtablefilterltekpireportstores  
                // var sleepingCellsData =  _context.Objtablefilterltekpireportstores
                // var sleepingTask = _context.Objtablefilterltekpireportstores
                var sleepingRaw = await _context.Objtablefilterltekpireportstores
        .Where(x => x.PeriodStartTime == dateString &&
           !string.IsNullOrEmpty(x.LnbtsName))
        .ToListAsync(); // ← Chỉ ToListAsync thôi

                // 3. Get detail data từ archive table (Process/Success/Fail)
                // var detailData = _context.Objtable4gkpireportresultdetailarchives
                // var detailTask = _context.Objtable4gkpireportresultdetailarchives
                var detailRaw = await _context.Objtable4gkpireportresultdetailarchives
        .Where(x => !string.IsNullOrEmpty(x.LnbtsName) &&
           !string.IsNullOrEmpty(x.District) &&
           x.DataDate.HasValue &&
           x.DataDate.Value == targetDate)
        .ToListAsync(); // ← Chỉ ToListAsync thôi

                /*

                var totalRaw = await totalTask;
                var sleepingRaw = await sleepingTask;
                var detailRaw = await detailTask;

                */


                // Process data
                var totalCellsData = totalRaw.AsEnumerable()
                    .GroupBy(x => x.LnbtsName.Length >= 3 ? x.LnbtsName.Substring(x.LnbtsName.Length - 3) : "")
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .Select(g => new { Province = g.Key, TotalCells = g.Count() })
                    .ToList();

                var sleepingCellsData = sleepingRaw.AsEnumerable()
                    .GroupBy(x => x.LnbtsName.Length >= 3 ? x.LnbtsName.Substring(x.LnbtsName.Length - 3) : "")
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .Select(g => new { Province = g.Key, SleepingCells = g.Count() })
                    .ToList();


                var detailData = detailRaw.AsEnumerable()
        .GroupBy(x => x.LnbtsName.Length >= 3 ? x.LnbtsName.Substring(x.LnbtsName.Length - 3) : "")
        .Where(g => !string.IsNullOrEmpty(g.Key))
        .Select(provinceGroup => new {
            Province = provinceGroup.Key,
            Districts = provinceGroup.Select(x => x.District).Distinct().Count(),
            ProcessCells = provinceGroup.Count(x => !string.IsNullOrEmpty(x.ExecutionStatus)),
            SuccessCells = provinceGroup.Count(x => x.LastResetSuccess == true),
            FailCells = provinceGroup.Count(x => x.LastResetSuccess == false),

            DistrictDetails = provinceGroup.GroupBy(x => x.District)
        .Select(districtGroup => {
            var districtName = districtGroup.Key;

            // Get total cells cho district này
            var districtTotalInfo = totalRaw
                .Where(t => t.LnbtsName.Length >= 6 &&
                           t.LnbtsName.Substring(3, 3) == districtName &&
                           t.LnbtsName.Substring(t.LnbtsName.Length - 3) == provinceGroup.Key) // ✅ THÊM điều kiện Province
                .Count();
            // Get sleeping cells cho district này  
            var districtSleepingInfo = sleepingRaw
            .Where(s => s.LnbtsName.Length >= 6 &&
                       s.LnbtsName.Substring(3, 3) == districtName &&
                       s.LnbtsName.Substring(s.LnbtsName.Length - 3) == provinceGroup.Key // ✅ THÊM điều kiện Province
                       )
            .Count();

            return new
            {
                District = districtName,
                TotalCells = districtTotalInfo,              // ✅ THÊM
                SleepingCells = districtSleepingInfo,        // ✅ THÊM
                ProcessCells = districtGroup.Count(x => !string.IsNullOrEmpty(x.ExecutionStatus)),
                SuccessCells = districtGroup.Count(x => x.LastResetSuccess == true),
                FailCells = districtGroup.Count(x => x.LastResetSuccess == false)
            };
        }).ToList()
        })
        .ToList(); // ✅ THÊM .ToList() và dấu chấm phẩy










                // 4. Combine all data
                var result = detailData.Select(detail => {
                    var totalInfo = totalCellsData.FirstOrDefault(t => t.Province == detail.Province);
                    var sleepingInfo = sleepingCellsData.FirstOrDefault(s => s.Province == detail.Province);

                    return new
                    {
                        detail.Province,
                        detail.Districts,
                        TotalCells = totalInfo?.TotalCells ?? 0,        // ← Từ objtable4gkpireportlogs
                        SleepingCells = sleepingInfo?.SleepingCells ?? 0, // ← Từ objtablefilterltekpireportstores  
                        detail.ProcessCells,                             // ← Từ archive
                        detail.SuccessCells,                            // ← Từ archive
                        detail.FailCells,                               // ← Từ archive
                        SuccessRate = detail.ProcessCells > 0 ? Math.Round((double)detail.SuccessCells / detail.ProcessCells * 100, 1) : 0,
                        detail.DistrictDetails
                    };
                }).ToList();

                return new
                {
                    success = true,
                    data = result
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    error = ex.Message
                };
            }
        }
        // ket thuc public async Task<object> funDashboardServiceGetZone4SummaryAsync(string date)

        public async Task<SleepingCellTrendResponse> funDashboardServiceGetTrendAsync(string? endDate = null)
        {
            try
            {
                Console.WriteLine($"🔍 API called with endDate: {endDate}");

                DateOnly actualEndDate;
                if (!string.IsNullOrEmpty(endDate))
                {
                    if (!DateOnly.TryParse(endDate, out actualEndDate))
                    {
                        Console.WriteLine($"❌ Failed to parse endDate: {endDate}");
                        return new SleepingCellTrendResponse
                        {
                            Success = false,
                            Data = new List<SleepingCellDayData>(),
                            Summary = null
                        };
                    }
                }
                else
                {
                    actualEndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
                }

                var actualStartDate = actualEndDate.AddDays(-13);
                Console.WriteLine($"📅 Query range: {actualStartDate} to {actualEndDate}");

                // ✅ SỬA: Thay vì query Objtablesleepingcelllogs, 
                // dùng lại logic từ các methods có sẵn
                var data = new List<SleepingCellDayData>();

                for (int i = 0; i < 14; i++)
                {
                    var currentDate = actualStartDate.AddDays(i);
                    var isCurrentDate = currentDate == DateOnly.FromDateTime(DateTime.Today);

                    Console.WriteLine($"📅 Processing date: {currentDate}");

                    try
                    {
                        // ✅ REUSE: Dùng lại existing methods
                        var todayAnalysis = await GetTodayAnalysisCount(currentDate, isCurrentDate);
                        var sleepingCells = await GetSleepingCellsCount(currentDate, isCurrentDate);
                        var processCells = await GetProcessing_CellsCount(currentDate, isCurrentDate);
                        var executionCells = await GetExecutionCellsCount(currentDate);
                        var recheckCells = await GetRecheckCellsCount(currentDate);

                        // Calculate success rate
                        var successRate = processCells > 0 ?
                            Math.Round((double)executionCells / processCells * 100.0, 1) : 0.0;

                        var dayData = new SleepingCellDayData
                        {
                            Date = currentDate.ToString("yyyy-MM-dd"),
                            TodayAnalysis = todayAnalysis,
                            SleepingCells = sleepingCells,
                            ProcessCells_ = processCells,
                            ExecutionCells = executionCells,
                            RecheckCells = recheckCells,
                            SuccessRate = successRate
                        };

                        data.Add(dayData);
                        Console.WriteLine($"✅ {currentDate}: Sleeping={sleepingCells}, Process={processCells}, Execution={executionCells}");
                    }
                    catch (Exception dayEx)
                    {
                        Console.WriteLine($"⚠️ Error processing {currentDate}: {dayEx.Message}");

                        // Add zero data for failed days instead of crash
                        data.Add(new SleepingCellDayData
                        {
                            Date = currentDate.ToString("yyyy-MM-dd"),
                            TodayAnalysis = 0,
                            SleepingCells = 0,
                            ProcessCells_ = 0,
                            ExecutionCells = 0,
                            RecheckCells = 0,
                            SuccessRate = 0
                        });
                    }
                }

                // Calculate summary
                var validData = data.Where(x => x.SleepingCells > 0 || x.ProcessCells_ > 0).ToList();
                var summary = new SleepingCellSummary
                {
                    TotalDays = validData.Count,
                    AvgSleepingCells = validData.Count > 0 ? Math.Round(validData.Average(x => x.SleepingCells), 1) : 0,
                    AvgSuccessRate = validData.Count > 0 ? Math.Round(validData.Average(x => x.SuccessRate), 1) : 0
                };

                Console.WriteLine($"✅ Successfully processed {data.Count} days, {validData.Count} valid");
                Console.WriteLine($"📊 Summary: Avg Sleeping={summary.AvgSleepingCells}, Avg Success Rate={summary.AvgSuccessRate}%");

                return new SleepingCellTrendResponse
                {
                    Success = true,
                    Data = data,
                    Summary = summary
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error in funDashboardServiceGetTrendAsync: {ex.Message}");
                return new SleepingCellTrendResponse
                {
                    Success = false,
                    Data = new List<SleepingCellDayData>(),
                    Summary = null
                };
            }
        }

        /*

        public async Task<SleepingCellTrendResponse> funDashboardServiceGetTrendAsync(string? endDate = null)
        {
            try
            {
                // Lấy 14 ngày gần nhất
                // var endDate = DateTime.Today;
                // var startDate = endDate.AddDays(-13); // 14 ngày = today + 13 ngày trước

                // var dateString = selectedDate.ToString("yyyy-MM-dd");

                Console.WriteLine($"🔍 API called with endDate: {endDate}");

                // var result = await _dashboardService.AggregateAndSave14DaysAsync();
                DateOnly actualEndDate;

                // Nếu có endDate thì parse, không thì dùng yesterday
                if (!string.IsNullOrEmpty(endDate))
                {
                    if (!DateOnly.TryParse(endDate, out actualEndDate))
                    {
                        Console.WriteLine($"❌ Failed to parse endDate: {endDate}");
                        return new SleepingCellTrendResponse
                        {
                            Success = false,
                            Data = new List<SleepingCellDayData>(),
                            Summary = null
                        };
                    }
                }
                else
                {
                    actualEndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
                }

                var actualStartDate = actualEndDate.AddDays(-13);

                Console.WriteLine($"📅 Query range: {actualStartDate} to {actualEndDate}");

                
                var rawData = await _context.Objtablesleepingcelllogs
           .Where(x => x.Date >= actualStartDate && x.Date <= actualEndDate)
           .OrderBy(x => x.Date)
           .ToListAsync();

                

        Console.WriteLine($"📊 Found {rawData.Count} records in database");

                var data = new List<SleepingCellDayData>();

                var allDates = Enumerable.Range(0, 14)
                    .Select(i => actualStartDate.AddDays(i))
                    .ToList();

                var data = allDates.Select(date =>
                {
                    var dayData = rawData.FirstOrDefault(x => x.Date == date);

                    if (dayData == null)
                    {
                        return new SleepingCellDayData
                        {
                            Date = date.ToString("yyyy-MM-dd"),
                            TodayAnalysis = 0,
                            SleepingCells = 0,
                            ProcessCells_ = 0,
                            ExecutionCells = 0,
                            RecheckCells = 0,
                            SuccessRate = 0
                        };
                    }

                    var processCount = dayData.Processcells.GetValueOrDefault(0);
                    var executionCount = dayData.Executioncells.GetValueOrDefault(0);
                    var successRate = processCount > 0 ? Math.Round((double)executionCount / (double)processCount * 100.0, 1) : 0.0;

                    return new SleepingCellDayData
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        TodayAnalysis = dayData.Todayanalysis ?? 0,
                        SleepingCells = dayData.Sleepingcells ?? 0,
                        ProcessCells_ = dayData.Processcells ?? 0,
                        ExecutionCells = dayData.Executioncells ?? 0,
                        RecheckCells = dayData.Recheckcells ?? 0,
                        SuccessRate = successRate
                    };
                }).ToList();

                var summary = new SleepingCellSummary
                {
                    TotalDays = 14,
                    AvgSleepingCells = data.Count > 0 ? Math.Round(data.Average(x => x.SleepingCells), 1) : 0,
                    AvgSuccessRate = data.Count > 0 ? Math.Round(data.Average(x => x.SuccessRate), 1) : 0
                };

                return new SleepingCellTrendResponse
                {
                    Success = true,
                    Data = data,
                    Summary = summary
                };



            }
            catch (Exception ex)
            {
                return new SleepingCellTrendResponse
                {
                    Success = false,
                    Data = new List<SleepingCellDayData>(),
                    Summary = null
                };
            }


        }
        // ket thuc public async Task<SleepingCellTrendResponse> funDashboardServiceGetTrendAsync(string? endDate = null)

        */

        // Thêm vào ImplementationDashboardService.cs:

        public async Task<object> funDashboardServiceGetSleepingCellsAsync(string date)
        {
            try
            {
                var targetDate = DateOnly.Parse(date);

                var sleepingCells = await _context.Objtablefilterltekpireportstores
                    .Where(x => x.PeriodStartTime == targetDate.ToString("MM.dd.yyyy") &&
                               !string.IsNullOrEmpty(x.LncelName))
                    .Select(x => new {
                        lncel_name = x.LncelName,
                        lnbts_name = x.LnbtsName,

                        province = !string.IsNullOrEmpty(x.LncelName) && x.LncelName.Length >= 3
        ? x.LncelName.Substring(x.LncelName.Length - 3, 3)
        : "N/A",
                        district = !string.IsNullOrEmpty(x.LncelName) && x.LncelName.Length >= 6
        ? x.LncelName.Substring(3, 3)
        : "N/A",





                        period_start_time = x.PeriodStartTime,
                        created_at = x.CreatedAt
                    })
                    .ToListAsync();

                return new { success = true, data = sleepingCells };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }


        public async Task<object> funDashboardServiceGetProcessCellsAsync(string date)
        {
            try
            {
                var targetDate = DateOnly.Parse(date);

                var processCells = await _context.Objtable4gkpireportresultdetailarchives
                    .Where(x => x.DataDate == targetDate &&
                               !string.IsNullOrEmpty(x.ExecutionStatus))
                    .Select(x => new {
                        lncel_name = x.LncelName,
                        lnbts_name = x.LnbtsName,
                        province = x.Province,
                        district = x.District,
                        execution_status = x.ExecutionStatus,
                        period_start_time = x.PeriodStartTime,
                        ssh_host = x.SshHost
                    })
                    .ToListAsync();

                return new { success = true, data = processCells };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        public async Task<object> funDashboardServiceGetExecutionCellsAsync(string date)
        {
            try
            {
                var targetDate = DateOnly.Parse(date);

                var executionCells = await _context.Objtable4gkpireportresultdetailarchives
                    .Where(x => x.DataDate == targetDate &&
                               x.ExecutionStatus == "completed")
                    .Select(x => new {
                        lncel_name = x.LncelName,
                        lnbts_name = x.LnbtsName,
                        province = x.Province,
                        district = x.District,
                        execution_status = x.ExecutionStatus,
                        period_start_time = x.PeriodStartTime,
                        ssh_host = x.SshHost
                    })
                    .ToListAsync();

                return new { success = true, data = executionCells };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        public async Task<object> funDashboardServiceGetRecheckCellsAsync(string date)
        {
            try
            {
                var targetDate = DateOnly.Parse(date);

                var recheckCells = await _context.Objtable4gkpireportresultdetailarchives
                    .Where(x => x.DataDate == targetDate &&
                               (x.ExecutionStatus == "starting_reset" ||
                                x.ExecutionStatus == "failed"))
                    .Select(x => new {
                        lncel_name = x.LncelName,
                        lnbts_name = x.LnbtsName,
                        province = x.Province,
                        district = x.District,
                        execution_status = x.ExecutionStatus,
                        period_start_time = x.PeriodStartTime,
                        ssh_host = x.SshHost
                    })
                    .ToListAsync();

                return new { success = true, data = recheckCells };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

    }
}