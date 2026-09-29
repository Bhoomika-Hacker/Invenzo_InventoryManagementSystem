using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.ProductService;
using InventoryManagementSystem.Services.PurchaseService;
using InventoryManagementSystem.Services.SupplierService;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class PurchaseController : Controller
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public PurchaseController(
            IPurchaseService purchaseService,
            ISupplierService supplierService,
            IProductService productService,
            IMapper mapper)
        {
            _purchaseService = purchaseService;
            _supplierService = supplierService;
            _productService = productService;
            _mapper = mapper;
        }

        // =========================
        // INDEX
        // Admin + Seller
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var purchases =
                await _purchaseService.GetAllAsync();

            var purchaseVM =
                _mapper.Map<IEnumerable<PurchaseVM>>(purchases);

            return View(purchaseVM);
        }

        // =========================
        // DETAILS
        // Admin + Seller
        // =========================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var purchase =
                await _purchaseService.GetByIdAsync(id);

            if (purchase == null)
                return NotFound();

            var purchaseVM =
                _mapper.Map<PurchaseVM>(purchase);

            return View(purchaseVM);
        }

        // =========================
        // CREATE - GET
        // Admin only
        // =========================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers =
                await _supplierService.GetAllAsync();

            ViewBag.Products =
                await _productService.GetAllAsync();

            return View();
        }

        // =========================
        // CREATE - POST
        // Admin only
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            PurchaseVM purchaseVM)
        {
            // Remove generated/navigation fields
            ModelState.Remove("PurchaseId");
            ModelState.Remove("Supplier");

            if (purchaseVM.PurchaseItems != null)
            {
                for (int i = 0;
                     i < purchaseVM.PurchaseItems.Count;
                     i++)
                {
                    ModelState.Remove(
                        $"PurchaseItems[{i}].PurchaseItemId");

                    ModelState.Remove(
                        $"PurchaseItems[{i}].PurchaseId");

                    ModelState.Remove(
                        $"PurchaseItems[{i}].Purchase");

                    ModelState.Remove(
                        $"PurchaseItems[{i}].Product");

                    ModelState.Remove(
                        $"PurchaseItems[{i}].UnitPrice");

                    ModelState.Remove(
                        $"PurchaseItems[{i}].TotalPrice");
                }
            }

            // At least one item
            if (purchaseVM.PurchaseItems == null ||
                purchaseVM.PurchaseItems.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Please add at least one product.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers =
                    await _supplierService.GetAllAsync();

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(purchaseVM);
            }

            // ViewModel -> Model
            var purchase =
                _mapper.Map<Purchase>(purchaseVM);

            try
            {
                // Business logic is inside Service
                await _purchaseService.AddAsync(purchase);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);

                ViewBag.Suppliers =
                    await _supplierService.GetAllAsync();

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(purchaseVM);
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT - GET
        // Admin only
        // =========================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var purchase =
                await _purchaseService.GetByIdAsync(id);

            if (purchase == null)
                return NotFound();

            ViewBag.Suppliers =
                await _supplierService.GetAllAsync();

            ViewBag.Products =
                await _productService.GetAllAsync();

            var purchaseVM =
                _mapper.Map<PurchaseVM>(purchase);

            return View(purchaseVM);
        }

        // =========================
        // EDIT - POST
        // Admin only
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            PurchaseVM purchaseVM)
        {
            if (id != purchaseVM.PurchaseId)
                return NotFound();

            // Remove navigation fields
            ModelState.Remove("Supplier");

            if (purchaseVM.PurchaseItems != null)
            {
                for (int i = 0;
                     i < purchaseVM.PurchaseItems.Count;
                     i++)
                {
                    ModelState.Remove(
                        $"PurchaseItems[{i}].Purchase");

                    ModelState.Remove(
                        $"PurchaseItems[{i}].Product");

                    ModelState.Remove(
                        $"PurchaseItems[{i}].UnitPrice");

                    ModelState.Remove(
                        $"PurchaseItems[{i}].TotalPrice");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers =
                    await _supplierService.GetAllAsync();

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(purchaseVM);
            }

            // ViewModel -> Model
            var purchase =
                _mapper.Map<Purchase>(purchaseVM);

            try
            {
                // Business logic is inside Service
                await _purchaseService.UpdateAsync(purchase);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);

                ViewBag.Suppliers =
                    await _supplierService.GetAllAsync();

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(purchaseVM);
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE - GET
        // Admin only
        // =========================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var purchase =
                await _purchaseService.GetByIdAsync(id);

            if (purchase == null)
                return NotFound();

            var purchaseVM =
                _mapper.Map<PurchaseVM>(purchase);

            return View(purchaseVM);
        }

        // =========================
        // DELETE - POST
        // Admin only
        // =========================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            await _purchaseService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}