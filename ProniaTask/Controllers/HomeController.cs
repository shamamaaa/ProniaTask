using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProniaTask.DAL;
using ProniaTask.Models;
using ProniaTask.ViewModels;

namespace ProniaTask.Controllers
{
    public class HomeController : Controller
    {

        private readonly AppDbContext _context;


        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<Product> products = _context.Products.OrderByDescending(p=>p.Id).Take(8).ToList();
            List<Slide> slides = _context.Slides.OrderBy(p => p.Id).ToList();


            HomeVM homeVM = new()
            {
                Products = products,
                Slides = slides
            };

            return View(homeVM);
        }
    }
}

