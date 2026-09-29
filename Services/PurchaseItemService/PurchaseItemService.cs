using InventoryManagementSystem.Models;
using InventoryManagementSystem.Repository.PurchaseItemRepo;

namespace InventoryManagementSystem.Services.PurchaseItemService
{
    public class PurchaseItemService : IPurchaseItemService
    {
        private readonly IPurchaseItemRepository _purchaseItemRepository;

        public PurchaseItemService(
            IPurchaseItemRepository purchaseItemRepository)
        {
            _purchaseItemRepository = purchaseItemRepository;
        }

        public async Task<IEnumerable<PurchaseItem>> GetAllAsync()
        {
            return await _purchaseItemRepository.GetAllAsync();
        }

        public async Task<PurchaseItem> GetByIdAsync(int id)
        {
            return await _purchaseItemRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(PurchaseItem purchaseItem)
        {
            await _purchaseItemRepository.AddAsync(purchaseItem);
        }

        public async Task UpdateAsync(PurchaseItem purchaseItem)
        {
            await _purchaseItemRepository.UpdateAsync(purchaseItem);
        }

        public async Task DeleteAsync(int id)
        {
            await _purchaseItemRepository.DeleteAsync(id);
        }
    }
}