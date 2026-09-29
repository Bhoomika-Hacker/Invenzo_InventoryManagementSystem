using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.CategoryService;
using InventoryManagementSystem.Services.ProductService;
using InventoryManagementSystem.Services.SupplierService;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly IMapper _mapper;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            IMapper mapper)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _mapper = mapper;
        }

        // GET: Product
        // Admin + Seller
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();

            var productVM =
                _mapper.Map<IEnumerable<ProductVM>>(products);

            return View(productVM);
        }

        // GET: Product/Details/5
        // Admin + Seller
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product =
                await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            var productVM =
                _mapper.Map<ProductVM>(product);

            return View(productVM);
        }

        // GET: Product/Create
        // Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories =
                await _categoryService.GetAllAsync();

            ViewBag.Suppliers =
                await _supplierService.GetAllAsync();

            return View();
        }

        // POST: Product/Create
        // Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories =
                    await _categoryService.GetAllAsync();

                ViewBag.Suppliers =
                    await _supplierService.GetAllAsync();

                return View(productVM);
            }

            var product =
                _mapper.Map<Product>(productVM);

            await _productService.AddAsync(product);

            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Edit/5
        // Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product =
                await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            ViewBag.Categories =
                await _categoryService.GetAllAsync();

            ViewBag.Suppliers =
                await _supplierService.GetAllAsync();

            var productVM =
                _mapper.Map<ProductVM>(product);

            return View(productVM);
        }

        // POST: Product/Edit/5
        // Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            ProductVM productVM)
        {
            if (id != productVM.ProductId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories =
                    await _categoryService.GetAllAsync();

                ViewBag.Suppliers =
                    await _supplierService.GetAllAsync();

                return View(productVM);
            }

            var product =
                _mapper.Map<Product>(productVM);

            await _productService.UpdateAsync(product);

            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Delete/5
        // Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product =
                await _productService.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            var productVM =
                _mapper.Map<ProductVM>(product);

            return View(productVM);
        }

        // POST: Product/Delete/5
        // Admin only
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }

        // GET: Product/LowStock
        // Admin + Seller
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> LowStock()
        {
            var products =
                await _productService.GetLowStockProductsAsync();

            return View(products);
        }
    }
}