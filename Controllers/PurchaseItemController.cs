using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.ProductService;
using InventoryManagementSystem.Services.PurchaseItemService;
using InventoryManagementSystem.Services.PurchaseService;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class PurchaseItemController : Controller
    {
        private readonly IPurchaseItemService _purchaseItemService;
        private readonly IPurchaseService _purchaseService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public PurchaseItemController(
            IPurchaseItemService purchaseItemService,
            IPurchaseService purchaseService,
            IProductService productService,
            IMapper mapper)
        {
            _purchaseItemService = purchaseItemService;
            _purchaseService = purchaseService;
            _productService = productService;
            _mapper = mapper;
        }

        // =========================
        // INDEX
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items =
                await _purchaseItemService.GetAllAsync();

            var itemVM =
                _mapper.Map<IEnumerable<PurchaseItemVM>>(items);

            return View(itemVM);
        }

        // =========================
        // DETAILS
        // =========================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item =
                await _purchaseItemService.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            return View(
                _mapper.Map<PurchaseItemVM>(item));
        }

        // =========================
        // CREATE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Purchases =
                await _purchaseService.GetAllAsync();

            ViewBag.Products =
                await _productService.GetAllAsync();

            return View();
        }

        // =========================
        // CREATE - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PurchaseItemVM itemVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Purchases =
                    await _purchaseService.GetAllAsync();

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(itemVM);
            }

            var item =
                _mapper.Map<PurchaseItem>(itemVM);

            await _purchaseItemService.AddAsync(item);

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item =
                await _purchaseItemService.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            ViewBag.Purchases =
                await _purchaseService.GetAllAsync();

            ViewBag.Products =
                await _productService.GetAllAsync();

            return View(
                _mapper.Map<PurchaseItemVM>(item));
        }

        // =========================
        // EDIT - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PurchaseItemVM itemVM)
        {
            if (id != itemVM.PurchaseItemId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Purchases =
                    await _purchaseService.GetAllAsync();

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(itemVM);
            }

            var item =
                _mapper.Map<PurchaseItem>(itemVM);

            await _purchaseItemService.UpdateAsync(item);

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item =
                await _purchaseItemService.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            return View(
                _mapper.Map<PurchaseItemVM>(item));
        }

        // =========================
        // DELETE - POST
        // =========================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _purchaseItemService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}