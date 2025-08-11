using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Models.DTOs.SleepingCell.Services;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;

namespace ClassLibraryRnocDataCenterWebBusiness.Services.Implementations.NSN.SleepingCell
{
    public class ImplementationSleepingCellService : InterfaceSleepingCellService
    {
        private readonly InterfaceSleepingCellKpiRepository _kpiRepository;
        private readonly InterfaceBtsInfoRepository _btsRepository;
        private readonly InterfaceResetService _resetService;
        private readonly InterfaceValidationService _validationService;

        public ImplementationSleepingCellService(
            InterfaceSleepingCellKpiRepository kpiRepository,
            InterfaceBtsInfoRepository btsRepository,
            InterfaceResetService resetService,
            InterfaceValidationService validationService)
        {
            _kpiRepository = kpiRepository;
            _btsRepository = btsRepository;
            _resetService = resetService;
            _validationService = validationService;
        }

        public async Task<IEnumerable<SleepingCellDto>> GetSleepingCellsAsync()
        {
            var sleepingCells = await _kpiRepository.GetSleepingCellsAsync();
            var result = new List<SleepingCellDto>();

            foreach (var cell in sleepingCells)
            {
                var btsInfo = await _btsRepository.GetBtsByMrbtsNameAsync(cell.MrbtsName);
                var isEligible = await _validationService.IsCellConfigurationValidAsync(cell.LncelName ?? "");

                result.Add(new SleepingCellDto
                {
                    CellName = cell.LncelName ?? "",
                    BtsName = cell.MrbtsName ?? "",
                    Province = cell.MrbtsName?.Length >=3 ? cell.MrbtsName.Substring(cell.MrbtsName.Length - 3).ToUpper() : "Undefined",
                    Vendor = btsInfo?.Vendor ?? "NSN",
                    TrafficDl = cell.PdcpVolumeDl ?? 0,
                    TrafficUl = cell.PdcpVolumeUl ?? 0,
                    Availability = cell.CellAvail ?? 0,
                    LastDetected = DateTime.Parse(cell.PeriodStartTime ?? DateTime.Now.ToString()),
                    IsEligibleForReset = isEligible && !(btsInfo?.Blacklist ?? false)
                });
            }

            return result;
        }

        // Continue với các methods khác...




        public async Task<IEnumerable<SleepingCellDto>> GetSleepingCellsByProvinceAsync(string province)
        {
            var allCells = await GetSleepingCellsAsync();
            return allCells.Where(c => c.Province.Equals(province, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<SleepingCellDto>> GetSleepingCellsByVendorAsync(string vendor)
        {
            var allCells = await GetSleepingCellsAsync();
            return allCells.Where(c => c.Vendor.Equals(vendor, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<SleepingCellStatsDto> GetSleepingCellStatsAsync()
        {
            var allCells = await GetSleepingCellsAsync();
            var cellsList = allCells.ToList();

            return new SleepingCellStatsDto
            {
                TotalSleepingCells = cellsList.Count,
                TotalCells = cellsList.Count, // TODO: Get actual total from repository
                SleepingCellRate = 100, // TODO: Calculate actual rate
                ByProvince = cellsList.GroupBy(c => c.Province).ToDictionary(g => g.Key, g => g.Count()),
                ByVendor = cellsList.GroupBy(c => c.Vendor).ToDictionary(g => g.Key, g => g.Count()),
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<CellStatusDto> GetCellStatusAsync(string cellName)
        {
            var cells = await GetSleepingCellsAsync();
            var cell = cells.FirstOrDefault(c => c.CellName == cellName);

            return new CellStatusDto
            {
                CellName = cellName,
                Exists = cell != null,
                IsSleeping = cell != null,
                IsBlacklisted = !(cell?.IsEligibleForReset ?? true),
                IsResetAllowed = cell?.IsEligibleForReset ?? false,
                CurrentStatus = cell != null ? "Sleeping" : "Not Found",
                StatusMessage = cell != null ? "Cell is sleeping" : "Cell not found"
            };
        }

        public async Task<bool> IsCellEligibleForResetAsync(string cellName)
        {
            var status = await GetCellStatusAsync(cellName);
            return status.IsResetAllowed;
        }

        public async Task<ResetResultDto> ResetSingleCellAsync(string cellName)
        {
            return await _resetService.ResetSingleCellAsync(cellName);
        }

        public async Task<BatchResetResultDto> ResetBatchCellsAsync(IEnumerable<string> cellNames)
        {
            return await _resetService.ResetBatchCellsAsync(cellNames);
        }

        public async Task<ResetValidationDto> ValidateResetRequestAsync(string cellName)
        {
            return await _resetService.ValidateResetRequestAsync(cellName);
        }






    }
}