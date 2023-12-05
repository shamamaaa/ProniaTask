using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProniaTask.Areas.ProniaAdmin.ViewModels;
using ProniaTask.DAL;
using ProniaTask.Models;
using ProniaTask.Utilities.Extensions;
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

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products.Include(p=>p.Category).Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true)).ToListAsync();
            return View(products);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new ();

            GetSelectList(ref productVM);

            return View(productVM);
        }

        [Authorize(Roles = "Admin,Moderator")]
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
                ModelState.AddModelError("CategoryId", "Category not found");
                return View();
            }


            result = await _context.Products.AnyAsync(c => c.Name == productVM.Name);
            if (result)
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("Name", "Product already exists");
                return View();
            }

            result = await _context.Products.AnyAsync(c => c.Price < 0);
            if (result)
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("Price", "Price can't be smaller than 0");
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

            foreach (int colorid in productVM.ColorIds)
            {
                bool colorresult = await _context.Colors.AnyAsync(x => x.Id == colorid);
                if (!colorresult)
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("ColorIds", "Wrong color information input.");
                    return View();
                }
            }

            foreach (int sizeid in productVM.SizeIds)
            {
                bool sizeresult = await _context.Sizes.AnyAsync(x => x.Id == sizeid);
                if (!sizeresult)
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("SizeIds", "Wrong size information input.");
                    return View();
                }
            }


            if (!productVM.MainPhoto.ValidateType())
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("MainPhoto", "Wrong file type.");
                return View();
            }
            if (!productVM.MainPhoto.ValidateSize(2*1024))
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("MainPhoto", "Wrong file size.You need to choose up to 2mb.");
                return View();
            }


            if (!productVM.HoverPhoto.ValidateType())
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("HoverPhoto", "Wrong file type.");
                return View();
            }
            if (!productVM.HoverPhoto.ValidateSize(2 * 1024))
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("HoverPhoto", "Wrong file size.You need to choose up to 2mb.");
                return View();
            }

            ProductImage image = new ProductImage
            {
                IsPrimary = true,
                Url = await productVM.MainPhoto.CreateFile(_env.WebRootPath, "assets", "images", "website-images")
            };

            ProductImage hoverimage = new ProductImage
            {
                IsPrimary = false,
                Url = await productVM.HoverPhoto.CreateFile(_env.WebRootPath, "assets", "images", "website-images")
            };

            Product product = new Product
            {
                Name = productVM.Name,
                Price = productVM.Price,
                SKU = productVM.SKU,
                CategoryId = (int)productVM.CategoryId,
                Description = productVM.Description,
                ProductTags = new List<ProductTag>(),
                ProductSizes = new List<ProductSize>(),
                ProductColors = new List<ProductColor>(),
                ProductImages = new List<ProductImage> {hoverimage,image }
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

            TempData["Message"] = "";

            foreach (IFormFile photo  in productVM.Photos)
            {
                if (!photo.ValidateType())
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName}'s  type is not suitable<p/>";
                    continue;
                }
                if (!photo.ValidateSize(2 * 1024))
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName}'s  size is not suitable<p/>";
                    continue;
                }

                product.ProductImages.Add(new ProductImage
                {
                    IsPrimary = null,
                    Url = await photo.CreateFile(_env.WebRootPath, "assets", "images", "website-images")
                });
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Product existed = await _context.Products.Include(x => x.ProductTags).Include(y => y.ProductColors).Include(z => z.ProductSizes).Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
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
                ProductImages=existed.ProductImages
            };
            GetSelectList(ref productVM);
            return View(productVM);
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
        {
            Product existed = await _context.Products
                .Include(p=>p.ProductTags)
                .Include(x=>x.ProductSizes)
                .Include(y=>y.ProductColors)
                .Include(pi=>pi.ProductImages)
                .FirstOrDefaultAsync(e => e.Id == id);

            productVM.ProductImages = existed.ProductImages;
            if (existed is null) return NotFound();

            if (!ModelState.IsValid)
            {
                GetSelectList(ref productVM);

                return View(productVM);
            }


            bool result = await _context.Products.AnyAsync(c => c.Name == productVM.Name && c.Id != id);
            if (result)
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("Name", "Product already exists");
                return View(productVM);
            }

            bool result1 = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result1)
            {
                GetSelectList(ref productVM);
                ModelState.AddModelError("CategoryId", "Category not found, choose another one.");
                return View(productVM);
            }

            ////////

            existed.ProductTags.RemoveAll(pt => !productVM.TagIds.Exists(tId => tId == pt.TagId));

            List<int> tagcreatable = productVM.TagIds.Where(tId => !existed.ProductTags.Exists(pt => pt.TagId == tId)).ToList();

            foreach (int tagid in tagcreatable)
            {
                bool tagresult = await _context.Tags.AnyAsync(t => t.Id == tagid);

                if (!tagresult)
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("TagIds", "Tag not found.");
                    return View();
                }
                existed.ProductTags.Add(new ProductTag
                {
                    TagId = tagid
                });
            }

            ///////

            existed.ProductColors.RemoveAll(pc => !productVM.ColorIds.Exists(cId => cId == pc.ColorId));

            List<int> colorcreatable = productVM.ColorIds.Where(cId => !existed.ProductColors.Exists(pc => pc.ColorId == cId)).ToList();

            foreach (int colorid in colorcreatable)
            {
                bool colorresult = await _context.Colors.AnyAsync(c => c.Id == colorid);

                if (!colorresult)
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("ColorIds", "Color not found.");
                    return View();
                }
                existed.ProductColors.Add(new ProductColor
                {
                    ColorId = colorid
                });
            }

            ///////
            existed.ProductSizes.RemoveAll(pc => !productVM.SizeIds.Exists(cId => cId == pc.SizeId));

            List<int> sizecreatable = productVM.SizeIds.Where(cId => !existed.ProductSizes.Exists(pc => pc.SizeId == cId)).ToList();

            foreach (int sizeid in sizecreatable)
            {
                bool sizeresult = await _context.Sizes.AnyAsync(c => c.Id == sizeid);

                if (!sizeresult)
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("SizeIds", "Size not found.");
                    return View();
                }
                existed.ProductSizes.Add(new ProductSize
                {
                    SizeId = sizeid
                });
            }


            ///////


            if (productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType())
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("MainPhoto", "File type is not valid");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.ValidateSize(2*1024))
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("MainPhoto", "Size is not valid, you need to choose up to 2MB");
                    return View(productVM);
                }
            }

            if (productVM.HoverPhoto is not null)
            {
                if (!productVM.HoverPhoto.ValidateType())
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("HoverPhoto", "File type is not valid");
                    return View(productVM);
                }
                if (!productVM.HoverPhoto.ValidateSize(2 * 1024))
                {
                    GetSelectList(ref productVM);
                    ModelState.AddModelError("HoverPhoto", "Size is not valid, you need to choose up to 2MB");
                    return View(productVM);
                }
            }

            if (productVM.MainPhoto is not null)
            {
                string filename = await productVM.MainPhoto.CreateFile(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage mainimg = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true);
                mainimg.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                _context.ProductImages.Remove(mainimg);

                existed.ProductImages.Add(new ProductImage
                {
                    IsPrimary = true,
                    Url = filename
                });
            }

            if (productVM.HoverPhoto is not null)
            {
                string filename = await productVM.HoverPhoto.CreateFile(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage hoverimg = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false);
                hoverimg.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                _context.ProductImages.Remove(hoverimg);

                existed.ProductImages.Add(new ProductImage
                {
                    IsPrimary = false,
                    Url = filename
                });
            }

            if (productVM.ImageIds is null)
            {
                productVM.ImageIds = new List<int>();
            }

            List<ProductImage> removable = existed.ProductImages.Where(pi => !productVM.ImageIds.Exists(imgid => imgid == pi.Id)&& pi.IsPrimary == null).ToList();
            foreach (var pig in removable)
            {
                pig.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(pig);
            }

            TempData["Message"] = "";

            if (productVM.Photos is not null)
            {
                foreach (IFormFile photo in productVM.Photos)
                {
                    if (!photo.ValidateType())
                    {
                        TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName}'s  type is not suitable<p/>";
                        continue;
                    }
                    if (!photo.ValidateSize(2 * 1024))
                    {
                        TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName}'s  size is not suitable<p/>";
                        continue;
                    }

                    existed.ProductImages.Add(new ProductImage
                    {
                        IsPrimary = null,
                        Url = await photo.CreateFile(_env.WebRootPath, "assets", "images", "website-images")
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

        [Authorize(Roles = "Admin,Moderator")]
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

        [Authorize(Roles = "Admin")]
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

            if (product is null) return NotFound();

            foreach (var item in product.ProductImages)
            {
                item.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
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

