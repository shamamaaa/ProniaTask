using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaTask.DAL;
using ProniaTask.Models;
using ProniaTask.Utilities.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace ProniaTask.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

            bool result = _context.Slides.Any(s => s.Order < 0);
            if (result)
            {
                ModelState.AddModelError("Order", "Order can't be smaller than 0.");
                return View();
            }

            if (slide.Photo is null)
            {
                ModelState.AddModelError("Photo", "You need to choose file.");
                return View();
            }
            if (!slide.Photo.ValidateType())
            {
                ModelState.AddModelError("Photo", "You need to choose image file.");
                return View();
            }
            if (!slide.Photo.ValidateSize(2*1024))
            {
                ModelState.AddModelError("Photo", "You need to choose up to 2MB.");
                return View();
            }


            slide.ImageUrl = await slide.Photo.CreateFile(_env.WebRootPath, "assets", "images", "slider");

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();


            return View(existed);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Slide slide)
        {

            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(existed);
            }

            if (slide.Photo is not null)
            {
                bool result = _context.Slides.Any(s => s.Order < 0);
                if (result)
                {
                    ModelState.AddModelError("Order", "Order can't be smaller than 0.");
                    return View(existed);
                }

                if (!slide.Photo.ValidateType())
                {
                    ModelState.AddModelError("Photo", "You need to choose image file.");
                    return View(existed);
                }
                if (!slide.Photo.ValidateSize(2 * 1024))
                {
                    ModelState.AddModelError("Photo", "You need to choose up to 2MB.");
                    return View(existed);
                }
                string newimage = await slide.Photo.CreateFile(_env.WebRootPath, "assets", "images", "slider");
                existed.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "slider");
                existed.ImageUrl = newimage;
            }

            existed.Title = slide.Title;
            existed.Subtitle = slide.Subtitle;
            existed.Description = slide.Description;
            existed.Order = slide.Order;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int id)
        {
            if (id <=0) return BadRequest();
            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide is null) return NotFound();

            slide.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "images", "slider");


            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            Slide slide = await _context.Slides.FirstOrDefaultAsync(x => x.Id == id);
            if (slide is null) return NotFound();
            return View(slide);
        }
    }
}

