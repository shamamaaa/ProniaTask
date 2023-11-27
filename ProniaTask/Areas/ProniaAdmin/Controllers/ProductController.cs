using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            CreateProductVM productVM = new ();

            GetSelectList(ref productVM);

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                GetSelectList(ref productVM);
                return View(productVM);
            }
            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("CategoryId", "Category not found, choose another one.");
                return View();
            }

            result = await _context.Products.AnyAsync(c => c.Name == productVM.Name);
            if (result)
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("Name", "Product already exists");
                return View();
            }

            foreach (int tagid in productVM.TagIds)
            {
                bool tagresult = await _context.Tags.AnyAsync(x=>x.Id==tagid);
                if (!tagresult)
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("TagIds", "Wrong tag information input.");
                    return View();
                }
            }

            Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price,
                SKU = productVM.SKU,
                CategoryId = (int)productVM.CategoryId,
                Description = productVM.Description,
                ProductTags = new List<ProductTag>(),
                ProductSizes = new List<ProductSize>(),
                ProductColors = new List<ProductColor>()
            };


            foreach (int tagid in productVM.TagIds)
            {
                bool isexist = await _context.Tags.AnyAsync(x => x.Id == tagid);
                if (!isexist) return BadRequest();
                
                ProductTag productTag = new ProductTag
                {
                    TagId=tagid,
                };
                product.ProductTags.Add(productTag);
            }

            foreach (int sizid in productVM.SizeIds)
            {
                bool isexist = await _context.Sizes.AnyAsync(x => x.Id == sizid);
                if (!isexist) return BadRequest();

                ProductSize productSize = new ProductSize
                {
                    SizeId = sizid,
                };
                product.ProductSizes.Add(productSize);
            }

            foreach (int colorid in productVM.ColorIds)
            {
                bool isexist = await _context.Colors.AnyAsync(x => x.Id == colorid);
                if (!isexist) return BadRequest();

                ProductColor productColor = new ProductColor
                {
                    ColorId = colorid,
                };
                product.ProductColors.Add(productColor);
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }


        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Product existed = await _context.Products.Include(x=>x.ProductTags).Include(y=>y.ProductColors).Include(z=>z.ProductSizes).FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = existed.Name,
                Price = existed.Price,
                Description = existed.Description,
                SKU = existed.SKU,
                CategoryId = existed.CategoryId,
                TagIds= existed.ProductTags.Select(pt=>pt.TagId).ToList(),
                ColorIds = existed.ProductColors.Select(pt => pt.ColorId).ToList(),
                SizeIds = existed.ProductSizes.Select(pt => pt.SizeId).ToList(),
            
            };
            GetSelectList(ref productVM);
            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                GetSelectList(ref productVM);

                return View();
            }

            Product existed = await _context.Products.Include(p=>p.ProductTags).Include(x=>x.ProductSizes).Include(y=>y.ProductColors).FirstOrDefaultAsync(e => e.Id == id);
            if (existed is null) return NotFound();

            bool result = await _context.Products.AnyAsync(c => c.Name == productVM.Name && c.Id != id);
            if (result)
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("Name", "Product already exists");
                return View();
            }

            bool result1 = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result1)
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("CategoryId", "Category not found, choose another one.");
                return View();
            }


            foreach (ProductTag pt in existed.ProductTags)
            {
                if (productVM.TagIds.Exists(t=>t==pt.TagId))
                {
                    _context.ProductTags.Remove(pt);
                }
            }


            foreach (int tagid in productVM.TagIds)
            {
                if (!existed.ProductTags.Any(pt=>pt.TagId == tagid))
                {
                    existed.ProductTags.Add(new ProductTag
                    {
                        TagId = tagid
                    });
                }
            }

            ///////

            foreach (ProductColor pc in existed.ProductColors)
            {
                if (productVM.ColorIds.Exists(t => t == pc.ColorId))
                {
                    _context.ProductColors.Remove(pc);
                }
            }


            foreach (int colorid in productVM.ColorIds)
            {
                if (!existed.ProductColors.Any(pt => pt.ColorId == colorid))
                {
                    existed.ProductColors.Add(new ProductColor
                    {
                        ColorId = colorid
                    });
                }
            }

            ///////

            foreach (ProductSize pt in existed.ProductSizes)
            {
                if (productVM.SizeIds.Exists(t => t == pt.SizeId))
                {
                    _context.ProductSizes.Remove(pt);
                }
            }


            foreach (int sizeid in productVM.SizeIds)
            {
                if (!existed.ProductSizes.Any(pt => pt.SizeId == sizeid))
                {
                    existed.ProductSizes.Add(new ProductSize
                    {
                        SizeId = sizeid
                    });
                }
            }


            existed.Name = productVM.Name;
            existed.Price = productVM.Price;
            existed.SKU = productVM.SKU;
            existed.Description = productVM.Description;
            existed.CategoryId = (int)productVM.CategoryId;


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

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            var product = await _context.Products
                .Include(x => x.ProductImages)
                .Include(x => x.Category)
                .Include(x => x.ProductImages)
                .Include(x => x.ProductTags)
                .ThenInclude(y => y.Tag)
                .Include(x => x.ProductSizes)
                .ThenInclude(s => s.Size)
                .Include(x => x.ProductColors)
                .ThenInclude(c => c.Color)
                .FirstOrDefaultAsync(x=>x.Id==id);

            if (product is null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        private void GetSelectList(ref CreateProductVM vm)
        {
            vm.Categories= new( _context.Categories, "Id", "Name");
            vm.Colors = new(_context.Colors, "Id", "Name");
            vm.Sizes = new(_context.Sizes, "Id", "Name");
            vm.Tags = new(_context.Tags, "Id", "Name");
            
        }


        private void GetSelectList(ref UpdateProductVM vm)
        {
            vm.Categories = new(_context.Categories, "Id", "Name");
            vm.Colors = new(_context.Colors, "Id", "Name");
            vm.Sizes = new(_context.Sizes, "Id", "Name");
            vm.Tags = new(_context.Tags, "Id", "Name");

        }
        

    }
}

