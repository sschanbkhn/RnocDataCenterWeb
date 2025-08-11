using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell
{
    public class ImplementationFilterTableRepository : InterfaceFilterTableRepository
    {
        private readonly ConnectionsInformationSleepingCellDbContext _context;

        public ImplementationFilterTableRepository(ConnectionsInformationSleepingCellDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Objtablefilterltekpireport>> GetAllSleepingCellsAsync()
        {
            try
            {
                return await _context.Objtablefilterltekpireports.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sleeping cells: {ex.Message}", ex);
            }
        }

        public async Task<int> GetSleepingCellCountAsync()
        {
            try
            {
                return await _context.Objtablefilterltekpireports.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error counting sleeping cells: {ex.Message}", ex);
            }
        }

        public async Task<Objtablefilterltekpireport> GetSleepingCellByNameAsync(string cellName)
        {
            try
            {
                return await _context.Objtablefilterltekpireports
                    .Where(x => x.LncelName == cellName)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving cell {cellName}: {ex.Message}", ex);
            }
        }

        public async Task<bool> HasSleepingCellsAsync()
        {
            try
            {
                return await _context.Objtablefilterltekpireports.AnyAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking sleeping cells existence: {ex.Message}", ex);
            }
        }
    }
}