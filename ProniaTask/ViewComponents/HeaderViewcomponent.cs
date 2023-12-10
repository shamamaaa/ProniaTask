using System;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaTask.DAL;
using ProniaTask.Models;
using ProniaTask.ViewModels;

namespace ProniaTask.ViewComponents
{
	public class HeaderViewcomponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _usermanager;
        private readonly IHttpContextAccessor _accessor;


        public HeaderViewcomponent(AppDbContext context, UserManager<AppUser> usermanager, IHttpContextAccessor accessor)
        {
            _context = context;
            _usermanager = usermanager;
            _accessor = accessor;

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            List<BasketItemVM> basketvm = await CartAsync();

            var viewModel = new HeaderVM
            {
                Settings = settings,
                BasketItems=basketvm
            };

            return View(viewModel);

        }

        public async Task<List<BasketItemVM>> CartAsync()
        {
            List<BasketItemVM> basketVM = new List<BasketItemVM>();

            if (_accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                AppUser? user = await _usermanager.Users.Include(u => u.BasketItems).ThenInclude(bi => bi.Product).ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(u => u.Id == _accessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                foreach (BasketItem item in user.BasketItems)
                {
                    basketVM.Add(new BasketItemVM()
                    {
                        Id = item.Product.Id,
                        Name = item.Product.Name,
                        Image = item.Product.ProductImages.FirstOrDefault().Url,
                        Price = item.Product.Price,
                        Count = item.Count,
                        Subtotal = item.Count * item.Product.Price
                    });
                }
            }
            else
            {
                if (_accessor.HttpContext.Request.Cookies["Basket"] is not null)
                {
                    List<BasketCookieItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(_accessor.HttpContext.Request.Cookies["Basket"]);

                    foreach (var basketcookieitem in basket)
                    {
                        Product product = await _context.Products.Include(x => x.ProductImages.Where(xi => xi.IsPrimary == true)).FirstOrDefaultAsync(p => p.Id == basketcookieitem.Id);
                        if (product is not null)
                        {
                            BasketItemVM basketItemVM = new BasketItemVM
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Image = product.ProductImages.FirstOrDefault().Url,
                                Price = product.Price,
                                Count = basketcookieitem.Count,
                                Subtotal = basketcookieitem.Count * product.Price
                            };
                            basketVM.Add(basketItemVM);
                        }
                    }
                }
            }

            return basketVM;
        }

    }
}

