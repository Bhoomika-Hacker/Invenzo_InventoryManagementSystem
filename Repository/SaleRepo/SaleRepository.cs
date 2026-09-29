using InventoryManagementSystem.DAL;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Repository.SaleRepo
{
    public class SaleRepository : ISaleRepository
    {
        private readonly InventoryDbContext _context;

        public SaleRepository(InventoryDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL SALES
        // =========================

        public async Task<IEnumerable<Sale>> GetAllAsync()
        {
            return await _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();
        }

        // =========================
        // GET SALE BY ID
        // =========================

        public async Task<Sale> GetByIdAsync(int id)
        {
            return await _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.SaleId == id);
        }

        // =========================
        // ADD SALE
        // =========================

        public async Task AddAsync(Sale sale)
        {
            _context.Sales.Add(sale);

            await _context.SaveChangesAsync();
        }

        // =========================
        // UPDATE SALE
        // =========================

        public async Task UpdateAsync(Sale sale)
        {
            _context.Sales.Update(sale);

            await _context.SaveChangesAsync();
        }

        // =========================
        // DELETE SALE
        // =========================

        public async Task DeleteAsync(int id)
        {
            var sale =
                await _context.Sales
                    .Include(s => s.SaleItems)
                    .FirstOrDefaultAsync(
                        s => s.SaleId == id);

            if (sale != null)
            {
                _context.SaleItems.RemoveRange(
                    sale.SaleItems);

                _context.Sales.Remove(sale);

                await _context.SaveChangesAsync();
            }
        }
    }
}