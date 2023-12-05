using System;
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


        public HeaderViewcomponent(AppDbContext context)
        {
            _context = context;

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

            if (Request.Cookies["Basket"] is not null)
            {
                List<BasketCookieItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

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

            return basketVM;
        }

    }
}

