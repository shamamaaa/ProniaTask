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
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products.Include(p=>p.Category).Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true)).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(productVM);
            }
            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("CategoryId", "Category not found, choose another one.");
                return View(productVM);
            }

            result = await _context.Products.AnyAsync(c => c.Name == productVM.Name);
            if (result)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("Name", "Product already exists");
                return View(productVM);
            }

            Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price,
                SKU = productVM.SKU,
                CategoryId = (int)productVM.CategoryId,
                Description = productVM.Description
            };
            ViewBag.Categories = await _context.Categories.ToListAsync();

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }


        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Product existed = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = existed.Name,
                Price=existed.Price,
                Description=existed.Description,
                SKU=existed.SKU,
                CategoryId = (int)existed.CategoryId
            };
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(productVM);
            }

            Product existed = await _context.Products.FirstOrDefaultAsync(e => e.Id == id);
            if (existed is null) return NotFound();

            bool result = _context.Products.Any(c => c.Name == productVM.Name && c.Id != id);
            if (result)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("Name", "Product already exists");
                return View(productVM);
            }

            bool result1 = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result1)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ModelState.AddModelError("CategoryId", "Category not found, choose another one.");
                return View(productVM);
            }

            existed.Name = productVM.Name;
            existed.Price = productVM.Price;
            existed.SKU = productVM.SKU;
            existed.CategoryId = (int)productVM.CategoryId;
            existed.Description = productVM.Description;


            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products
                .Include(x => x.ProductImages)
                .Include(x => x.ProductTags).ThenInclude(t => t.Tag)
                .Include(x => x.ProductColors).ThenInclude(c => c.Color)
                .Include(x => x.ProductSizes).ThenInclude(s => s.Size)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product is null) return NotFound();

            return View(product);
        }



    }
}

