using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.Services.DashboardService
{
    public interface IDashboardService
    {
        Task<DashboardVM> GetDashboardDataAsync();
    }
}