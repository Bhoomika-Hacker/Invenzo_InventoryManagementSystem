using AutoMapper;
using InventoryManagementSystem.Services.StockMovementService;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class StockMovementController : Controller
    {
        private readonly IStockMovementService _stockMovementService;
        private readonly IMapper _mapper;

        public StockMovementController(
            IStockMovementService stockMovementService,
            IMapper mapper)
        {
            _stockMovementService = stockMovementService;
            _mapper = mapper;
        }

        // =========================
        // INDEX
        // Admin + Seller
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var movements =
                await _stockMovementService.GetAllAsync();

            var movementVM =
                _mapper.Map<IEnumerable<StockMovementVM>>(movements);

            return View(movementVM);
        }

        // =========================
        // DETAILS
        // Admin + Seller
        // =========================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var movement =
                await _stockMovementService.GetByIdAsync(id);

            if (movement == null)
                return NotFound();

            return View(
                _mapper.Map<StockMovementVM>(movement));
        }
    }
}