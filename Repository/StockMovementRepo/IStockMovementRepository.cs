using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Repository.StockMovementRepo
{
    public interface IStockMovementRepository
    {
        Task<IEnumerable<StockMovement>> GetAllAsync();

        Task<StockMovement> GetByIdAsync(int id);

        Task AddAsync(StockMovement stockMovement);

        Task UpdateAsync(StockMovement stockMovement);

        Task DeleteAsync(int id);
    }
}