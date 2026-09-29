using InventoryManagementSystem.DAL;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Repository.StockMovementRepo
{
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly InventoryDbContext _context;

        public StockMovementRepository(InventoryDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<StockMovement>> GetAllAsync()
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .ToListAsync();
        }


        // =========================
        // GET BY ID
        // =========================

        public async Task<StockMovement> GetByIdAsync(int id)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .FirstOrDefaultAsync(
                    sm => sm.StockMovementId == id);
        }


        // =========================
        // ADD
        // =========================

        public async Task AddAsync(
            StockMovement stockMovement)
        {
            _context.StockMovements.Add(stockMovement);

            await _context.SaveChangesAsync();
        }


        // =========================
        // UPDATE
        // =========================

        public async Task UpdateAsync(
            StockMovement stockMovement)
        {
            _context.StockMovements.Update(stockMovement);

            await _context.SaveChangesAsync();
        }


        // =========================
        // DELETE
        // =========================

        public async Task DeleteAsync(int id)
        {
            var stockMovement =
                await _context.StockMovements.FindAsync(id);

            if (stockMovement != null)
            {
                _context.StockMovements.Remove(stockMovement);

                await _context.SaveChangesAsync();
            }
        }
    }
}