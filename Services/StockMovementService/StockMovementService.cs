using InventoryManagementSystem.Models;
using InventoryManagementSystem.Repository.StockMovementRepo;

namespace InventoryManagementSystem.Services.StockMovementService
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IStockMovementRepository _stockMovementRepository;

        public StockMovementService(
            IStockMovementRepository stockMovementRepository)
        {
            _stockMovementRepository = stockMovementRepository;
        }

        public async Task<IEnumerable<StockMovement>> GetAllAsync()
        {
            return await _stockMovementRepository.GetAllAsync();
        }

        public async Task<StockMovement> GetByIdAsync(int id)
        {
            return await _stockMovementRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(StockMovement stockMovement)
        {
            await _stockMovementRepository.AddAsync(stockMovement);
        }

        public async Task UpdateAsync(StockMovement stockMovement)
        {
            await _stockMovementRepository.UpdateAsync(stockMovement);
        }

        public async Task DeleteAsync(int id)
        {
            await _stockMovementRepository.DeleteAsync(id);
        }
    }
}