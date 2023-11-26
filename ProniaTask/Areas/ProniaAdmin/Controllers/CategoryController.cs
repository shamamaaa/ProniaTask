using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaTask.Areas.ProniaAdmin.ViewModels;
using ProniaTask.DAL;
using ProniaTask.Models;
using ProniaTask.ViewModels;

namespace ProniaTask.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _context.Categories.Include(c => c.Products).ToListAsync();

            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Categories.Any(c => c.Name.ToLower().Trim() == categoryVM.Name.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Category already exists");
                return View();
            }

            Category category = new Category
            {
                Name = categoryVM.Name
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Category existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null) return NotFound();

            UpdateCategoryVM categoryVM = new UpdateCategoryVM
            {
                Name=existed.Name
            };

            return View(categoryVM);
        }


        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
            {
                return View(categoryVM);
            }

            Category existed= await _context.Categories.FirstOrDefaultAsync(e => e.Id == id);
            if (existed is null) return NotFound();

            bool result = _context.Categories.Any(c => c.Name == categoryVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "Category already exists");
                return View(categoryVM);
            }


            existed.Name = categoryVM.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Category existed = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (existed is null) return NotFound();

            _context.Categories.Remove(existed);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            var category = await _context.Categories.Include(c=>c.Products).ThenInclude(p=>p.ProductImages).FirstOrDefaultAsync(x => x.Id == id);
            if (category is null) return NotFound();
            return View(category);
        }

    }
}

