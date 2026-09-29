using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Repository.PurchaseRepo
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<Purchase>> GetAllAsync();

        Task<Purchase> GetByIdAsync(int id);

        Task AddAsync(Purchase purchase);

        Task UpdateAsync(Purchase purchase);

        Task DeleteAsync(int id);
    }
}