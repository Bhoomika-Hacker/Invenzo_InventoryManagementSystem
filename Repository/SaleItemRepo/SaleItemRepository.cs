using InventoryManagementSystem.DAL;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Repository.SaleItemRepo
{
    public class SaleItemRepository : ISaleItemRepository
    {
        private readonly InventoryDbContext _context;

        public SaleItemRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SaleItem>> GetAllAsync()
        {
            return await _context.SaleItems
                .Include(si => si.Sale)
                .Include(si => si.Product)
                .ToListAsync();
        }

        public async Task<SaleItem> GetByIdAsync(int id)
        {
            return await _context.SaleItems
                .Include(si => si.Sale)
                .Include(si => si.Product)
                .FirstOrDefaultAsync(si => si.SaleItemId == id);
        }

        public async Task AddAsync(SaleItem saleItem)
        {
            _context.SaleItems.Add(saleItem);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SaleItem saleItem)
        {
            _context.SaleItems.Update(saleItem);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var saleItem = await _context.SaleItems
                .FirstOrDefaultAsync(si => si.SaleItemId == id);

            if (saleItem != null)
            {
                _context.SaleItems.Remove(saleItem);

                await _context.SaveChangesAsync();
            }
        }
    }
}