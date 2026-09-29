using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.ProductService;
using InventoryManagementSystem.Services.SaleItemService;
using InventoryManagementSystem.Services.SaleService;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class SaleItemController : Controller
    {
        private readonly ISaleItemService _saleItemService;
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public SaleItemController(
            ISaleItemService saleItemService,
            ISaleService saleService,
            IProductService productService,
            IMapper mapper)
        {
            _saleItemService = saleItemService;
            _saleService = saleService;
            _productService = productService;
            _mapper = mapper;
        }

        // GET: SaleItem
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _saleItemService.GetAllAsync();

            var itemVM =
                _mapper.Map<IEnumerable<SaleItemVM>>(items);

            return View(itemVM);
        }

        // GET: SaleItem/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _saleItemService.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            return View(
                _mapper.Map<SaleItemVM>(item));
        }

        // GET: SaleItem/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Sales =
                await _saleService.GetAllAsync();

            ViewBag.Products =
                await _productService.GetAllAsync();

            return View();
        }

        // POST: SaleItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SaleItemVM itemVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sales =
                    await _saleService.GetAllAsync();

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(itemVM);
            }

            var item =
                _mapper.Map<SaleItem>(itemVM);

            await _saleItemService.AddAsync(item);

            return RedirectToAction(nameof(Index));
        }

        // GET: SaleItem/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item =
                await _saleItemService.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            ViewBag.Sales =
                await _saleService.GetAllAsync();

            ViewBag.Products =
                await _productService.GetAllAsync();

            return View(
                _mapper.Map<SaleItemVM>(item));
        }

        // POST: SaleItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SaleItemVM itemVM)
        {
            if (id != itemVM.SaleItemId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Sales =
                    await _saleService.GetAllAsync();

                ViewBag.Products =
                    await _productService.GetAllAsync();

                return View(itemVM);
            }

            var item =
                _mapper.Map<SaleItem>(itemVM);

            await _saleItemService.UpdateAsync(item);

            return RedirectToAction(nameof(Index));
        }

        // GET: SaleItem/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item =
                await _saleItemService.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            return View(
                _mapper.Map<SaleItemVM>(item));
        }

        // POST: SaleItem/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            await _saleItemService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}