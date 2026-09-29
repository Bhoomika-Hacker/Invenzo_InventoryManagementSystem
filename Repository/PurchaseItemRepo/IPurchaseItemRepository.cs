using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Repository.PurchaseItemRepo
{
    public interface IPurchaseItemRepository
    {
        Task<IEnumerable<PurchaseItem>> GetAllAsync();

        Task<PurchaseItem> GetByIdAsync(int id);

        Task AddAsync(PurchaseItem purchaseItem);

        Task UpdateAsync(PurchaseItem purchaseItem);

        Task DeleteAsync(int id);
    }
}