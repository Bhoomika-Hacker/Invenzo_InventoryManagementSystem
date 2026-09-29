using InventoryManagementSystem.Models;
using InventoryManagementSystem.Repository.SaleItemRepo;

namespace InventoryManagementSystem.Services.SaleItemService
{
    public class SaleItemService : ISaleItemService
    {
        private readonly ISaleItemRepository _saleItemRepository;

        public SaleItemService(
            ISaleItemRepository saleItemRepository)
        {
            _saleItemRepository = saleItemRepository;
        }

        public async Task<IEnumerable<SaleItem>> GetAllAsync()
        {
            return await _saleItemRepository.GetAllAsync();
        }

        public async Task<SaleItem> GetByIdAsync(int id)
        {
            return await _saleItemRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(SaleItem saleItem)
        {
            await _saleItemRepository.AddAsync(saleItem);
        }

        public async Task UpdateAsync(SaleItem saleItem)
        {
            await _saleItemRepository.UpdateAsync(saleItem);
        }

        public async Task DeleteAsync(int id)
        {
            await _saleItemRepository.DeleteAsync(id);
        }
    }
}