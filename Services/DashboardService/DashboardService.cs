using InventoryManagementSystem.Repository.DashboardRepo;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.Services.DashboardService
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(
            IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<DashboardVM> GetDashboardDataAsync()
        {
            return await _dashboardRepository.GetDashboardDataAsync();
        }
    }
}