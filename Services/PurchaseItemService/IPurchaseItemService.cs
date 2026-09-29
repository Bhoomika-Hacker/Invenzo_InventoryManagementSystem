using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Services.PurchaseItemService
{
    public interface IPurchaseItemService
    {
        Task<IEnumerable<PurchaseItem>> GetAllAsync();

        Task<PurchaseItem> GetByIdAsync(int id);

        Task AddAsync(PurchaseItem purchaseItem);

        Task UpdateAsync(PurchaseItem purchaseItem);

        Task DeleteAsync(int id);
    }
}