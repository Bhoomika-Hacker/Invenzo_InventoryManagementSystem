using AutoMapper;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.CategoryService;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoryController(
            ICategoryService categoryService,
            IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        // GET: Category
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories =
                await _categoryService.GetAllAsync();

            var categoryVM =
                _mapper.Map<IEnumerable<CategoryVM>>(categories);

            return View(categoryVM);
        }

        // GET: Category/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category =
                await _categoryService.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            var categoryVM =
                _mapper.Map<CategoryVM>(category);

            return View(categoryVM);
        }

        // GET: Category/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
                return View(categoryVM);

            var category =
                _mapper.Map<Category>(categoryVM);

            await _categoryService.AddAsync(category);

            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category =
                await _categoryService.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            var categoryVM =
                _mapper.Map<CategoryVM>(category);

            return View(categoryVM);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CategoryVM categoryVM)
        {
            if (id != categoryVM.CategoryId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(categoryVM);

            var category =
                _mapper.Map<Category>(categoryVM);

            await _categoryService.UpdateAsync(category);

            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category =
                await _categoryService.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            var categoryVM =
                _mapper.Map<CategoryVM>(category);

            return View(categoryVM);
        }

        // POST: Category/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}