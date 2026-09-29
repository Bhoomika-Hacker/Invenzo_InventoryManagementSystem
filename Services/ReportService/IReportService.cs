using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.Services.ReportService
{
    public interface IReportService
    {
        Task<ReportVM> GetReportDataAsync(
            DateTime? fromDate,
            DateTime? toDate);
    }
}