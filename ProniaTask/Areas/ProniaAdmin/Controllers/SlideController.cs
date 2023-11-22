using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaTask.DAL;
using ProniaTask.Models;

namespace ProniaTask.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        public SlideController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.ToListAsync();
            return View(slides);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Slide slide)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (slide.Photo is null)
            {
                ModelState.AddModelError("Photo", "You need to choose file.");
                return View();
            }
            if (!slide.Photo.ContentType.Contains("image"))
            {
                ModelState.AddModelError("Photo", "You need to choose image file.");
                return View();
            }
            if (slide.Photo.Length>2*1024*1024)
            {
                ModelState.AddModelError("Photo", "You need to choose up to 2MB.");
                return View();
            }

            string currentdirectory = Directory.GetCurrentDirectory();

            using (FileStream file = new FileStream(@$"{currentdirectory}/wwwroot/assets/images/slider/{slide.Photo.FileName}", FileMode.Create))
            {
                await slide.Photo.CopyToAsync(file);
            }

            slide.ImageUrl = slide.Photo.FileName ;

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Detail(int id)
        {
            Slide slide = await _context.Slides.FirstOrDefaultAsync(x => x.Id == id);
            return View(slide);
        }
    }
}

