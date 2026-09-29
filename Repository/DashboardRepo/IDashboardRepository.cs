using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.Repository.DashboardRepo
{
    public interface IDashboardRepository
    {
        Task<DashboardVM> GetDashboardDataAsync();
    }
}