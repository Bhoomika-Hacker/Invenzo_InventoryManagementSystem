using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.Repository.ReportRepo
{
    public interface IReportRepository
    {
        Task<ReportVM> GetReportDataAsync(
            DateTime? fromDate,
            DateTime? toDate);
    }
}