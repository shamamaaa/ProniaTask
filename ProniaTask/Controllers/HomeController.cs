using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            List<Slide> slides = _context.Slides.OrderBy(p => p.Id).ToList();
            List<Product> productList = _context.Products.Include(x => x.ProductImages).ToList();

            HomeVM vm = new HomeVM
            {
                Products = productList,
                Slides = slides,
                LatestProducts = productList.OrderByDescending(p => p.Id).Take(8).ToList()
            };
            return View(vm);
        }
    }
}

