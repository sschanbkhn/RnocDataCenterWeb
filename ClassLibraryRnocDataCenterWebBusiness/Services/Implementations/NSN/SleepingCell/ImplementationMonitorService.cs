using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using Microsoft.EntityFrameworkCore;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Implementations.NSN.SleepingCell
{
    public class ImplementationKpiMonitorService : InterfaceMonitorService
    {
        private readonly ConnectionsInformationSleepingCellDbContext _context;

        public ImplementationKpiMonitorService(ConnectionsInformationSleepingCellDbContext context)
        {
            _context = context;
        }

        /*
        public async Task<object> funMonitorServiceGetKpiMonitorDataAsync(KpiMonitorRequest request)
        {
            try
            {
                // Parse date
                if (!DateOnly.TryParse(request.Date, out var targetDate))
                {
                    return new
                    {
                        success = false,
                        error = "Invalid date format. Use YYYY-MM-DD"
                    };
                }

                // Base query from archive table
                var query = _context.Objtable4gkpireportresultarchives
                    .Where(x => x.DataDate == targetDate);

                // Apply filters
                if (!string.IsNullOrEmpty(request.Province))
                    query = query.Where(x => x.Province == request.Province);

                if (!string.IsNullOrEmpty(request.District))
                    query = query.Where(x => x.District == request.District);

                if (!string.IsNullOrEmpty(request.Region))
                    query = query.Where(x => x.Region == request.Region);

                if (!string.IsNullOrEmpty(request.Vendor))
                    query = query.Where(x => x.Vendor == request.Vendor);

                // Search across multiple fields
                if (!string.IsNullOrEmpty(request.Search))
                {
                    var searchTerm = request.Search.ToLower();
                    query = query.Where(x =>
                        (x.Province != null && x.Province.ToLower().Contains(searchTerm)) ||
                        (x.District != null && x.District.ToLower().Contains(searchTerm)) ||
                        (x.MrbtsName != null && x.MrbtsName.ToLower().Contains(searchTerm)) ||
                        (x.LnbtsName != null && x.LnbtsName.ToLower().Contains(searchTerm)) ||
                        (x.LncelName != null && x.LncelName.ToLower().Contains(searchTerm)) ||
                        (x.Vendor != null && x.Vendor.ToLower().Contains(searchTerm))
                    );
                }

                // Get total count before pagination
                var totalRecords = await query.CountAsync();

                // Apply sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    switch (request.SortBy.ToLower())
                    {
                        case "province":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.Province)
                                : query.OrderBy(x => x.Province);
                            break;
                        case "cellavail":
                        case "cell_avail":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.CellAvail)
                                : query.OrderBy(x => x.CellAvail);
                            break;
                        case "pdcpvolumedl":
                        case "pdcp_volume_dl":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.PdcpVolumeDl)
                                : query.OrderBy(x => x.PdcpVolumeDl);
                            break;
                        default:
                            query = query.OrderByDescending(x => x.Id); // Default sort
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.Id); // Default sort
                }

                // Apply pagination
                var pagedQuery = query
                    .Skip((request.Page - 1) * request.PageSize)
                    // .Take(request.PageSize);
                var allRecords = await query.ToListAsync();  // ← LẤY ALL

                // Execute query and map to DTO
                var records = await pagedQuery
                    .Select(x => new KpiMonitorRecord
                    {
                        Id = x.Id,
                        OriginalId = x.OriginalId,
                        PeriodStartTime = x.PeriodStartTime,
                        MrbtsName = x.MrbtsName,
                        LnbtsName = x.LnbtsName,
                        LncelName = x.LncelName,
                        DnMrbtsSite = x.DnMrbtsSite,
                        PdcpVolumeDl = x.PdcpVolumeDl,
                        PdcpVolumeUl = x.PdcpVolumeUl,
                        CellAvail = x.CellAvail,
                        MaxUes = x.MaxUes,
                        MaxPdcpDl = x.MaxPdcpDl,
                        MaxPdcpUl = x.MaxPdcpUl,
                        Province = x.Province,
                        District = x.District,
                        Region = x.Region,
                        Vendor = x.Vendor,
                        // DataDate = x.DataDate,
                        // DataDate = x.DataDate.HasValue ? x.DataDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                        DataDate = x.DataDate.ToDateTime(TimeOnly.MinValue),

                        DataYear = x.DataYear,
                        DataMonth = x.DataMonth,
                        DataDay = x.DataDay,
                        DataQuarter = x.DataQuarter,
                        DataWeek = x.DataWeek,
                        OriginalCreatedAt = x.OriginalCreatedAt,
                        ArchivedAt = x.ArchivedAt,
                        ArchivedBy = x.ArchivedBy
                    })
                    .ToListAsync();

                // Map to snake_case for frontend
                var mappedRecords = records.Select(x => new
                {
                    id = x.Id,
                    original_id = x.OriginalId,
                    period_start_time = x.PeriodStartTime,
                    mrbts_name = x.MrbtsName,
                    lnbts_name = x.LnbtsName,
                    lncel_name = x.LncelName,
                    dn_mrbts_site = x.DnMrbtsSite,
                    pdcp_volume_dl = x.PdcpVolumeDl,
                    pdcp_volume_ul = x.PdcpVolumeUl,
                    cell_avail = x.CellAvail,
                    max_ues = x.MaxUes,
                    max_pdcp_dl = x.MaxPdcpDl,
                    max_pdcp_ul = x.MaxPdcpUl,
                    province = x.Province,
                    district = x.District,
                    region = x.Region,
                    vendor = x.Vendor,
                    data_date = x.DataDate,
                    data_year = x.DataYear,
                    data_month = x.DataMonth,
                    data_day = x.DataDay,
                    data_quarter = x.DataQuarter,
                    data_week = x.DataWeek,
                    original_created_at = x.OriginalCreatedAt,
                    archived_at = x.ArchivedAt,
                    archived_by = x.ArchivedBy
                }).ToList();





                // Calculate pagination info
                var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

                return new
                {
                    success = true,
                    data = new
                    {
                        // records = records,
                        records = mappedRecords,
                        totalRecords = totalRecords,
                        totalPages = totalPages,
                        currentPage = request.Page,
                        pageSize = request.PageSize
                    }
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

        */

        public async Task<object> funMonitorServiceGetFiltersAsync()
        {
            try
            {
                // Get unique values for filters from the most recent 30 days
                var cutoffDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));

                var provinces = await _context.Objtable4gkpireportresultarchives
                    .Where(x => x.DataDate >= cutoffDate && !string.IsNullOrEmpty(x.Province))
                    .Select(x => x.Province)
                .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();

                var districts = await _context.Objtable4gkpireportresultarchives
                    .Where(x => x.DataDate >= cutoffDate && !string.IsNullOrEmpty(x.District))
                    .Select(x => x.District)
                .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();

                var regions = await _context.Objtable4gkpireportresultarchives
                    .Where(x => x.DataDate >= cutoffDate && !string.IsNullOrEmpty(x.Region))
                    .Select(x => x.Region)
                .Distinct()
                .OrderBy(x => x)
                    .ToListAsync();

                var vendors = await _context.Objtable4gkpireportresultarchives
                    .Where(x => x.DataDate >= cutoffDate && !string.IsNullOrEmpty(x.Vendor))
                    .Select(x => x.Vendor)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();

                return new
                {
                    success = true,
                    data = new
                    {
                        provinces = provinces,
                        districts = districts,
                        regions = regions,
                        vendors = vendors
                    }
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

        public async Task<byte[]> funMonitorServiceExportToExcelAsync(string date, KpiMonitorRequest? filters = null)
        {
            try
            {
                // For now, return a simple placeholder
                // TODO: Implement actual Excel export using EPPlus or similar library
                var content = "Excel export functionality will be implemented here";
                return System.Text.Encoding.UTF8.GetBytes(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Export failed: {ex.Message}");
            }
        }



        public async Task<object> funMonitorServiceGetKpiMonitorDataAsync(KpiMonitorRequest request)
        {
            try
            {
                // Parse date
                if (!DateOnly.TryParse(request.Date, out var targetDate))
                {
                    return new
                    {
                        success = false,
                        error = "Invalid date format. Use YYYY-MM-DD"
                    };
                }

                // Base query from archive table
                var query = _context.Objtable4gkpireportresultarchives
                    .Where(x => x.DataDate == targetDate);

                // Apply filters
                if (!string.IsNullOrEmpty(request.Province))
                    query = query.Where(x => x.Province == request.Province);

                if (!string.IsNullOrEmpty(request.District))
                    query = query.Where(x => x.District == request.District);

                if (!string.IsNullOrEmpty(request.Region))
                    query = query.Where(x => x.Region == request.Region);

                if (!string.IsNullOrEmpty(request.Vendor))
                    query = query.Where(x => x.Vendor == request.Vendor);

                // Search across multiple fields
                if (!string.IsNullOrEmpty(request.Search))
                {
                    var searchTerm = request.Search.ToLower();
                    query = query.Where(x =>
                        (x.Province != null && x.Province.ToLower().Contains(searchTerm)) ||
                        (x.District != null && x.District.ToLower().Contains(searchTerm)) ||
                        (x.MrbtsName != null && x.MrbtsName.ToLower().Contains(searchTerm)) ||
                        (x.LnbtsName != null && x.LnbtsName.ToLower().Contains(searchTerm)) ||
                        (x.LncelName != null && x.LncelName.ToLower().Contains(searchTerm)) ||
                        (x.Vendor != null && x.Vendor.ToLower().Contains(searchTerm))
                    );
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    switch (request.SortBy.ToLower())
                    {
                        case "province":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.Province)
                                : query.OrderBy(x => x.Province);
                            break;
                        case "cellavail":
                        case "cell_avail":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.CellAvail)
                                : query.OrderBy(x => x.CellAvail);
                            break;
                        case "pdcpvolumedl":
                        case "pdcp_volume_dl":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.PdcpVolumeDl)
                                : query.OrderBy(x => x.PdcpVolumeDl);
                            break;
                        default:
                            query = query.OrderByDescending(x => x.Id); // Default sort
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.Id); // Default sort
                }

                // ✅ LẤY TẤT CẢ RECORDS - Không pagination ở server
                var allRecords = await query
                    .Select(x => new KpiMonitorRecord
                    {
                        Id = x.Id,
                        OriginalId = x.OriginalId,
                        PeriodStartTime = x.PeriodStartTime,
                        MrbtsName = x.MrbtsName,
                        LnbtsName = x.LnbtsName,
                        LncelName = x.LncelName,
                        DnMrbtsSite = x.DnMrbtsSite,
                        PdcpVolumeDl = x.PdcpVolumeDl,
                        PdcpVolumeUl = x.PdcpVolumeUl,
                        CellAvail = x.CellAvail,
                        MaxUes = x.MaxUes,
                        MaxPdcpDl = x.MaxPdcpDl,
                        MaxPdcpUl = x.MaxPdcpUl,
                        Province = x.Province,
                        District = x.District,
                        Region = x.Region,
                        Vendor = x.Vendor,
                        DataDate = x.DataDate.ToDateTime(TimeOnly.MinValue),
                        DataYear = x.DataYear,
                        DataMonth = x.DataMonth,
                        DataDay = x.DataDay,
                        DataQuarter = x.DataQuarter,
                        DataWeek = x.DataWeek,
                        OriginalCreatedAt = x.OriginalCreatedAt,
                        ArchivedAt = x.ArchivedAt,
                        ArchivedBy = x.ArchivedBy,
                        ExecutionStatus = x.ExecutionStatus  // ← THÊM DÒNG NÀY
                    })
                    .ToListAsync();

                // Get total count
                var totalRecords = allRecords.Count;

                // Map to snake_case for frontend
                var mappedRecords = allRecords.Select(x => new
                {
                    id = x.Id,
                    original_id = x.OriginalId,
                    period_start_time = x.PeriodStartTime,
                    mrbts_name = x.MrbtsName,
                    lnbts_name = x.LnbtsName,
                    lncel_name = x.LncelName,
                    dn_mrbts_site = x.DnMrbtsSite,
                    pdcp_volume_dl = x.PdcpVolumeDl,
                    pdcp_volume_ul = x.PdcpVolumeUl,
                    cell_avail = x.CellAvail,
                    max_ues = x.MaxUes,
                    max_pdcp_dl = x.MaxPdcpDl,
                    max_pdcp_ul = x.MaxPdcpUl,
                    province = x.Province,
                    district = x.District,
                    region = x.Region,
                    vendor = x.Vendor,
                    data_date = x.DataDate,
                    data_year = x.DataYear,
                    data_month = x.DataMonth,
                    data_day = x.DataDay,
                    data_quarter = x.DataQuarter,
                    data_week = x.DataWeek,
                    original_created_at = x.OriginalCreatedAt,
                    archived_at = x.ArchivedAt,
                    archived_by = x.ArchivedBy,
                    execution_status = x.ExecutionStatus  // ← THÊM DÒNG NÀY
                }).ToList();

                return new
                {
                    success = true,
                    data = new
                    {
                        records = mappedRecords,
                        totalRecords = totalRecords,
                        // Bỏ pagination info vì frontend tự handle
                    }
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



        public async Task<object> funMonitorServiceGetKpiMonitorDataRangeAsync(KpiMonitorDateRangeRequest request)
        {
            try
            {
                // Parse start date
                if (!DateOnly.TryParse(request.StartDate, out var startDate))
                {
                    return new
                    {
                        success = false,
                        error = "Invalid start date format. Use YYYY-MM-DD"
                    };
                }

                // Parse end date
                if (!DateOnly.TryParse(request.EndDate, out var endDate))
                {
                    return new
                    {
                        success = false,
                        error = "Invalid end date format. Use YYYY-MM-DD"
                    };
                }

                // Validate date range
                if (endDate < startDate)
                {
                    return new
                    {
                        success = false,
                        error = "End date cannot be earlier than start date"
                    };
                }

                // Base query from archive table with date range
                var query = _context.Objtable4gkpireportresultarchives
                    .Where(x => x.DataDate >= startDate && x.DataDate <= endDate);

                // Apply filters
                if (!string.IsNullOrEmpty(request.Province))
                    query = query.Where(x => x.Province == request.Province);

                if (!string.IsNullOrEmpty(request.District))
                    query = query.Where(x => x.District == request.District);

                if (!string.IsNullOrEmpty(request.Region))
                    query = query.Where(x => x.Region == request.Region);

                if (!string.IsNullOrEmpty(request.Vendor))
                    query = query.Where(x => x.Vendor == request.Vendor);

                // Search across multiple fields
                if (!string.IsNullOrEmpty(request.Search))
                {
                    var searchTerm = request.Search.ToLower();
                    query = query.Where(x =>
                        (x.Province != null && x.Province.ToLower().Contains(searchTerm)) ||
                        (x.District != null && x.District.ToLower().Contains(searchTerm)) ||
                        (x.MrbtsName != null && x.MrbtsName.ToLower().Contains(searchTerm)) ||
                        (x.LnbtsName != null && x.LnbtsName.ToLower().Contains(searchTerm)) ||
                        (x.LncelName != null && x.LncelName.ToLower().Contains(searchTerm)) ||
                        (x.Vendor != null && x.Vendor.ToLower().Contains(searchTerm))
                    );
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    switch (request.SortBy.ToLower())
                    {
                        case "province":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.Province)
                                : query.OrderBy(x => x.Province);
                            break;
                        case "cellavail":
                        case "cell_avail":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.CellAvail)
                                : query.OrderBy(x => x.CellAvail);
                            break;
                        case "pdcpvolumedl":
                        case "pdcp_volume_dl":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.PdcpVolumeDl)
                                : query.OrderBy(x => x.PdcpVolumeDl);
                            break;
                        case "datadate":
                        case "data_date":
                            query = request.SortDirection?.ToLower() == "desc"
                                ? query.OrderByDescending(x => x.DataDate)
                                : query.OrderBy(x => x.DataDate);
                            break;
                        default:
                            // Default sort by date desc, then by ID desc
                            query = query.OrderByDescending(x => x.DataDate).ThenByDescending(x => x.Id);
                            break;
                    }
                }
                else
                {
                    // Default sort by date desc, then by ID desc
                    query = query.OrderByDescending(x => x.DataDate).ThenByDescending(x => x.Id);
                }

                // ✅ LẤY TẤT CẢ RECORDS trong date range - Không pagination ở server
                var allRecords = await query
                    .Select(x => new KpiMonitorRecord
                    {
                        Id = x.Id,
                        OriginalId = x.OriginalId,
                        PeriodStartTime = x.PeriodStartTime,
                        MrbtsName = x.MrbtsName,
                        LnbtsName = x.LnbtsName,
                        LncelName = x.LncelName,
                        DnMrbtsSite = x.DnMrbtsSite,
                        PdcpVolumeDl = x.PdcpVolumeDl,
                        PdcpVolumeUl = x.PdcpVolumeUl,
                        CellAvail = x.CellAvail,
                        MaxUes = x.MaxUes,
                        MaxPdcpDl = x.MaxPdcpDl,
                        MaxPdcpUl = x.MaxPdcpUl,
                        Province = x.Province,
                        District = x.District,
                        Region = x.Region,
                        Vendor = x.Vendor,
                        DataDate = x.DataDate.ToDateTime(TimeOnly.MinValue),
                        DataYear = x.DataYear,
                        DataMonth = x.DataMonth,
                        DataDay = x.DataDay,
                        DataQuarter = x.DataQuarter,
                        DataWeek = x.DataWeek,
                        OriginalCreatedAt = x.OriginalCreatedAt,
                        ArchivedAt = x.ArchivedAt,
                        ArchivedBy = x.ArchivedBy,
                        ExecutionStatus = x.ExecutionStatus  // ← THÊM DÒNG NÀY
                    })
                    .ToListAsync();

                // Get total count
                var totalRecords = allRecords.Count;

                // Map to snake_case for frontend
                var mappedRecords = allRecords.Select(x => new
                {
                    id = x.Id,
                    original_id = x.OriginalId,
                    period_start_time = x.PeriodStartTime,
                    mrbts_name = x.MrbtsName,
                    lnbts_name = x.LnbtsName,
                    lncel_name = x.LncelName,
                    dn_mrbts_site = x.DnMrbtsSite,
                    pdcp_volume_dl = x.PdcpVolumeDl,
                    pdcp_volume_ul = x.PdcpVolumeUl,
                    cell_avail = x.CellAvail,
                    max_ues = x.MaxUes,
                    max_pdcp_dl = x.MaxPdcpDl,
                    max_pdcp_ul = x.MaxPdcpUl,
                    province = x.Province,
                    district = x.District,
                    region = x.Region,
                    vendor = x.Vendor,
                    data_date = x.DataDate,
                    data_year = x.DataYear,
                    data_month = x.DataMonth,
                    data_day = x.DataDay,
                    data_quarter = x.DataQuarter,
                    data_week = x.DataWeek,
                    original_created_at = x.OriginalCreatedAt,
                    archived_at = x.ArchivedAt,
                    archived_by = x.ArchivedBy,
                    execution_status = x.ExecutionStatus  // ← THÊM DÒNG NÀY
                }).ToList();

                // Get date range info for response
                var dateRangeInfo = new
                {
                    startDate = request.StartDate,
                    endDate = request.EndDate,
                    totalDays = (endDate.DayNumber - startDate.DayNumber) + 1
                };

                return new
                {
                    success = true,
                    data = new
                    {
                        records = mappedRecords,
                        totalRecords = totalRecords,
                        dateRange = dateRangeInfo
                    }
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




    }




}
