using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.ProductService;
using InventoryManagementSystem.Services.SaleService;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public SaleController(
            ISaleService saleService,
            IProductService productService,
            IMapper mapper)
        {
            _saleService = saleService;
            _productService = productService;
            _mapper = mapper;
        }

        // =========================
        // INDEX
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sales =
                await _saleService.GetAllAsync();

            var saleVM =
                _mapper.Map<IEnumerable<SaleVM>>(sales);

            return View(saleVM);
        }

        // =========================
        // DETAILS
        // =========================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var sale =
                await _saleService.GetByIdAsync(id);

            if (sale == null)
                return NotFound();

            var model =
                _mapper.Map<SaleVM>(sale);

            return View(model);
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Products =
                await _productService.GetAllAsync();

            var model = new SaleVM
            {
                SaleDate = DateTime.Now,
                SaleItems = new List<SaleItemVM>()
            };

            return View(model);
        }

        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SaleVM model)
        {
            ModelState.Remove("TotalAmount");

            if (model.SaleItems != null)
            {
                for (int i = 0;
                     i < model.SaleItems.Count;
                     i++)
                {
                    ModelState.Remove(
                        $"SaleItems[{i}].SaleId");

                    ModelState.Remove(
                        $"SaleItems[{i}].TotalPrice");

                    ModelState.Remove(
                        $"SaleItems[{i}].Product");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(model);
            }

            try
            {
                var sale =
                    _mapper.Map<Sale>(model);

                await _saleService.AddAsync(sale);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(model);
            }
        }

        // =========================
        // EDIT - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sale =
                await _saleService.GetByIdAsync(id);

            if (sale == null)
                return NotFound();

            var model =
                _mapper.Map<SaleVM>(sale);

            ViewBag.Products =
                await _productService.GetAllAsync();

            return View(model);
        }

        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SaleVM model)
        {
            if (id != model.SaleId)
                return NotFound();

            ModelState.Remove("TotalAmount");

            if (model.SaleItems != null)
            {
                for (int i = 0;
                     i < model.SaleItems.Count;
                     i++)
                {
                    ModelState.Remove(
                        $"SaleItems[{i}].SaleId");

                    ModelState.Remove(
                        $"SaleItems[{i}].TotalPrice");

                    ModelState.Remove(
                        $"SaleItems[{i}].Product");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(model);
            }

            try
            {
                var sale =
                    _mapper.Map<Sale>(model);

                await _saleService.UpdateAsync(sale);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(model);
            }
        }

        // =========================
        // DELETE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var sale =
                await _saleService.GetByIdAsync(id);

            if (sale == null)
                return NotFound();

            var model =
                _mapper.Map<SaleVM>(sale);

            return View(model);
        }

        // =========================
        // DELETE - POST
        // =========================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            try
            {
                await _saleService.DeleteAsync(id);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                var sale =
                    await _saleService.GetByIdAsync(id);

                if (sale == null)
                    return NotFound();

                var model =
                    _mapper.Map<SaleVM>(sale);

                return View("Delete", model);
            }
        }
    }
}