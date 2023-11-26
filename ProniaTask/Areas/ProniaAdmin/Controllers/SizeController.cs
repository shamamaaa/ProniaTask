using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaTask.Areas.ProniaAdmin.ViewModels;
using ProniaTask.DAL;
using ProniaTask.Models;

namespace ProniaTask.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class SizeController : Controller
    {
        private readonly AppDbContext _context;
        public SizeController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            List<Size> sizes = await _context.Sizes.Include(s => s.ProductSizes).ThenInclude(p => p.Product).ToListAsync();

            return View(sizes);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSizeVM sizeVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Sizes.Any(s => s.Name.ToLower().Trim() == sizeVM.Name.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Size already exists");
                return View();
            }

            Size size = new Size
            {
                Name=sizeVM.Name
            };

            await _context.Sizes.AddAsync(size);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Size existed = await _context.Sizes.FirstOrDefaultAsync(s => s.Id == id);

            if (existed is null) return NotFound();

            UpdateSizeVM sizeVM = new UpdateSizeVM
            {
                Name= existed.Name
            };

            return View(sizeVM);
        }


        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateSizeVM sizeVM)
        {
            if (!ModelState.IsValid)
            {
                return View(sizeVM);
            }

            Size existed = await _context.Sizes.FirstOrDefaultAsync(e => e.Id == id);
            if (existed is null) return NotFound();

            bool result = _context.Sizes.Any(c => c.Name == sizeVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "Size already exists");
                return View();
            }


            existed.Name = sizeVM.Name;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Size existed = await _context.Sizes.FirstOrDefaultAsync(s => s.Id == id);

            if (existed is null) return NotFound();

            _context.Sizes.Remove(existed);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            var size = await _context.Sizes.Include(s => s.ProductSizes).ThenInclude(p=> p.Product).ThenInclude(pi=>pi.ProductImages).FirstOrDefaultAsync(x => x.Id == id);
            if (size is null) return NotFound();
            return View(size);
        }
    }
}

