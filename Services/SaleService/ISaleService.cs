using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Services.SaleService
{
    public interface ISaleService
    {
        Task<IEnumerable<Sale>> GetAllAsync();

        Task<Sale> GetByIdAsync(int id);

        Task AddAsync(Sale sale);

        Task UpdateAsync(Sale sale);

        Task DeleteAsync(int id);
    }
}