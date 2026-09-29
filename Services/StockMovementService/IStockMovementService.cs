using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Services.StockMovementService
{
    public interface IStockMovementService
    {
        Task<IEnumerable<StockMovement>> GetAllAsync();

        Task<StockMovement> GetByIdAsync(int id);

        Task AddAsync(StockMovement stockMovement);

        Task UpdateAsync(StockMovement stockMovement);

        Task DeleteAsync(int id);
    }
}