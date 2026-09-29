using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Services.PurchaseService
{
    public interface IPurchaseService
    {
        Task<IEnumerable<Purchase>> GetAllAsync();

        Task<Purchase> GetByIdAsync(int id);

        Task AddAsync(Purchase purchase);

        Task UpdateAsync(Purchase purchase);

        Task DeleteAsync(int id);
    }
}