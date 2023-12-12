using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaTask.Areas.ProniaAdmin.ViewModels;
using ProniaTask.DAL;
using ProniaTask.Models;


namespace ProniaTask.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class TagController : Controller
    {
        private readonly AppDbContext _context;
        public TagController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Tags.CountAsync();

            List<Tag> tags = await _context.Tags.Skip(page * 5).Take(5)
                .Include(t => t.ProductTags).ToListAsync();

            PaginationVM<Tag> paginationVM = new PaginationVM<Tag>
            {
                TotalPage = Math.Ceiling(count / 5),
                CurrentPage = page + 1,
                Items = tags
            };

            return View(paginationVM);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM tagVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = _context.Tags.Any(t => t.Name.ToLower().Trim() == tagVM.Name.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Tag already exists");
                return View();
            }

            Tag tag = new Tag
            {
                Name=tagVM.Name
            };

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Tag existed = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);

            if (existed is null) return NotFound();

            UpdateTagVM tagVM = new UpdateTagVM
            {
                Name=existed.Name
            };

            return View(tagVM);
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateTagVM tagVM)
        {
            if (!ModelState.IsValid)
            {
                return View(tagVM);
            }

            Tag existed = await _context.Tags.FirstOrDefaultAsync(e => e.Id == id);
            if (existed is null) return NotFound();

            bool result = _context.Tags.Any(t => t.Name == tagVM.Name && t.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "Tag already exists");
                return View();
            }


            existed.Name = tagVM.Name;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Tag existed = await _context.Tags.FirstOrDefaultAsync(c => c.Id == id);

            if (existed is null) return NotFound();

            _context.Tags.Remove(existed);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Detail(int id)
        {
            Tag tag = await _context.Tags.Include(c => c.ProductTags).ThenInclude(p => p.Product).ThenInclude(i=>i.ProductImages).FirstOrDefaultAsync(x => x.Id == id);
            if (tag is null) return NotFound();
            return View(tag);
        }
    }
}

