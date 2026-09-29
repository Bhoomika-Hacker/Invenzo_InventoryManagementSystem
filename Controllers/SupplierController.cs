using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.SupplierService;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly IMapper _mapper;

        public SupplierController(
            ISupplierService supplierService,
            IMapper mapper)
        {
            _supplierService = supplierService;
            _mapper = mapper;
        }

        // GET: Supplier
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var suppliers =
                await _supplierService.GetAllAsync();

            var supplierVM =
                _mapper.Map<IEnumerable<SupplierVM>>(suppliers);

            return View(supplierVM);
        }

        // GET: Supplier/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var supplier =
                await _supplierService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            return View(
                _mapper.Map<SupplierVM>(supplier));
        }

        // GET: Supplier/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Supplier/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SupplierVM supplierVM)
        {
            if (!ModelState.IsValid)
                return View(supplierVM);

            var supplier =
                _mapper.Map<Supplier>(supplierVM);

            await _supplierService.AddAsync(supplier);

            return RedirectToAction(nameof(Index));
        }

        // GET: Supplier/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier =
                await _supplierService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            return View(
                _mapper.Map<SupplierVM>(supplier));
        }

        // POST: Supplier/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SupplierVM supplierVM)
        {
            if (id != supplierVM.SupplierId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(supplierVM);

            var supplier =
                _mapper.Map<Supplier>(supplierVM);

            await _supplierService.UpdateAsync(supplier);

            return RedirectToAction(nameof(Index));
        }

        // GET: Supplier/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier =
                await _supplierService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            return View(
                _mapper.Map<SupplierVM>(supplier));
        }

        // POST: Supplier/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _supplierService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}