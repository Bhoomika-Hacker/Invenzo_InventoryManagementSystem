using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Services.SaleItemService
{
    public interface ISaleItemService
    {
        Task<IEnumerable<SaleItem>> GetAllAsync();

        Task<SaleItem> GetByIdAsync(int id);

        Task AddAsync(SaleItem saleItem);

        Task UpdateAsync(SaleItem saleItem);

        Task DeleteAsync(int id);
    }
}