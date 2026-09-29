using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Repository.SaleItemRepo
{
    public interface ISaleItemRepository
    {
        Task<IEnumerable<SaleItem>> GetAllAsync();

        Task<SaleItem> GetByIdAsync(int id);

        Task AddAsync(SaleItem saleItem);

        Task UpdateAsync(SaleItem saleItem);

        Task DeleteAsync(int id);
    }
}