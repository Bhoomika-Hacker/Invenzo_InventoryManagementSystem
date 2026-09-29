using InventoryManagementSystem.Repository.ReportRepo;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.Services.ReportService
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(
            IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<ReportVM> GetReportDataAsync(
            DateTime? fromDate,
            DateTime? toDate)
        {
            return await _reportRepository.GetReportDataAsync(
                fromDate,
                toDate);
        }
    }
}