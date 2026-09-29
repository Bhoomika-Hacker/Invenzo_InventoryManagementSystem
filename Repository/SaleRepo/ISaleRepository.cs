using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Repository.SaleRepo
{
    public interface ISaleRepository
    {
        Task<IEnumerable<Sale>> GetAllAsync();

        Task<Sale> GetByIdAsync(int id);

        Task AddAsync(Sale sale);

        Task UpdateAsync(Sale sale);

        Task DeleteAsync(int id);
    }
}