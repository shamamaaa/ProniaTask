﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaTask.DAL;
using ProniaTask.Models;
using ProniaTask.Utilities.Exceptions;
using ProniaTask.ViewModels;

namespace ProniaTask.Controllers
{
    public class ProductController : Controller
    {

        private readonly AppDbContext _context;


        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail(int id)
        {
            if (id <= 0)
            {
                throw new WrongRequestException();
            }

            Product product = _context.Products
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .Include(x => x.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(x => x.ProductColors).ThenInclude(pt => pt.Color)
                .Include(x => x.ProductSizes).ThenInclude(pt => pt.Size)
                .FirstOrDefault(x => x.Id == id);

            if (product == null)
            {
                throw new NotFoundException("Oops, no product found :'(");
            }


            ProductVM vm = new ProductVM
            {
                Product = product,
                RelatedProducts = _context.Products.Where(p => p.Category.Id == product.CategoryId && p.Id != product.Id).Include(x => x.ProductImages).ToList(),
            };


            return View(vm);
        }
    }
}

