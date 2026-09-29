using InventoryManagementSystem.DAL;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Repository.PurchaseItemRepo
{
    public class PurchaseItemRepository : IPurchaseItemRepository
    {
        private readonly InventoryDbContext _context;

        public PurchaseItemRepository(InventoryDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================

        public async Task<IEnumerable<PurchaseItem>> GetAllAsync()
        {
            return await _context.PurchaseItems
                .Include(pi => pi.Purchase)
                .Include(pi => pi.Product)
                .ToListAsync();
        }


        // =========================
        // GET BY ID
        // =========================

        public async Task<PurchaseItem> GetByIdAsync(int id)
        {
            return await _context.PurchaseItems
                .Include(pi => pi.Purchase)
                .Include(pi => pi.Product)
                .FirstOrDefaultAsync(
                    pi => pi.PurchaseItemId == id);
        }


        // =========================
        // ADD
        // =========================

        public async Task AddAsync(PurchaseItem purchaseItem)
        {
            _context.PurchaseItems.Add(purchaseItem);

            await _context.SaveChangesAsync();
        }


        // =========================
        // UPDATE
        // =========================

        public async Task UpdateAsync(PurchaseItem purchaseItem)
        {
            _context.PurchaseItems.Update(purchaseItem);

            await _context.SaveChangesAsync();
        }


        // =========================
        // DELETE
        // =========================

        public async Task DeleteAsync(int id)
        {
            var purchaseItem =
                await _context.PurchaseItems.FindAsync(id);

            if (purchaseItem != null)
            {
                _context.PurchaseItems.Remove(purchaseItem);

                await _context.SaveChangesAsync();
            }
        }
    }
}