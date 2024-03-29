﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using ProniaTask.DAL;
using ProniaTask.Interfaces;
using ProniaTask.Models;
using ProniaTask.Utilities.Exceptions;
using ProniaTask.ViewModels;


namespace ProniaTask.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser>  _usermanager;
        private readonly IEmailService _emailService;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService)
        {
            _context = context;
            _usermanager = userManager;
            _emailService = emailService;
        }


        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new List<BasketItemVM>();

            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _usermanager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(bi => bi.Product).ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            }


            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id)
        {
            if (id<=0)
            {
                throw new WrongRequestException();
            }

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new NotFoundException("Oops, no product found :'(");
            }

            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _usermanager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (user is null)
                {
                    throw new NotFoundException("Oops, no user found :'(");
                }

                var item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
                if (item is null)
                {
                    item = new()
                    {
                        AppUserId = user.Id,
                        ProductId = product.Id,
                        Price = product.Price,
                        Count = 1,
                    };
                    user.BasketItems.Add(item);
                }
                else
                {
                    item.Count++;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                List<BasketCookieItemVM> basket;

                if (Request.Cookies["Basket"] is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);
                    if (item is null)
                    {
                        BasketCookieItemVM basketCookieItemVM = new BasketCookieItemVM
                        {
                            Id = id,
                            Count = 1
                        };
                        basket.Add(basketCookieItemVM);
                    }
                    else
                    {
                        item.Count++;
                    }
                }
                else
                {
                    basket = new List<BasketCookieItemVM>();
                    BasketCookieItemVM basketCookieItemVM = new BasketCookieItemVM
                    {
                        Id = id,
                        Count = 1
                    };
                    basket.Add(basketCookieItemVM);
                }


                string json = JsonConvert.SerializeObject(basket);
                Response.Cookies.Append("Basket", json);
            }


            return RedirectToAction(nameof(Index), "Home");
        }

        public async Task<IActionResult> Remove(int id)
        {
            if (id <= 0)
            {
                throw new WrongRequestException();
            };

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new NotFoundException("Oops, no product found :'(");
            };
            List<BasketCookieItemVM> basket = new List<BasketCookieItemVM>();

            if (Request.Cookies["Basket"] is not null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                BasketCookieItemVM basketCookieItemVM = basket.FirstOrDefault(item => item.Id == id);

                basket.Remove(basketCookieItemVM);

            }


            string json = JsonConvert.SerializeObject(basket);

            Response.Cookies.Append("Basket", json);
            return RedirectToAction(nameof(Index), "Basket");
        }

        public async Task<IActionResult> GetBasket()
        {
            return Content(Request.Cookies["Basket"]);
        }

        public async Task<IActionResult> Decrement(int id)
        {
                if (id <= 0)
                {
                throw new WrongRequestException();
                };

                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (product is null)
                {
                throw new NotFoundException("Oops, no product found :'(");
                };
                List<BasketCookieItemVM> basket;

                if (User.Identity.IsAuthenticated)
                {
                    AppUser user = await _usermanager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    if (user is null) return NotFound();
                    var basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
                    if (basketItem is not null)
                    {
                        basketItem.Count--;
                        if (basketItem.Count == 0)
                        {
                            user.BasketItems.Remove(basketItem);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {

                    if (Request.Cookies["Basket"] is not null)
                    {
                        basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);
                        BasketCookieItemVM basketCookieItemVM = basket.FirstOrDefault(item => item.Id == id);
                        if (basketCookieItemVM is not null)
                        {
                            basketCookieItemVM.Count--;
                            if (basketCookieItemVM.Count == 0)
                            {
                                basket.Remove(basketCookieItemVM);
                            }
                            string json = JsonConvert.SerializeObject(basket);
                            Response.Cookies.Append("Basket", json);
                        }
                    }
                }

                return RedirectToAction(nameof(Index), "Basket");
        }

        public async Task<IActionResult> Increment(int id)
        {
            if (id <= 0)
            {
                throw new WrongRequestException();
            }

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new NotFoundException("Oops, no product found :'(");
            }

            if (User.Identity.IsAuthenticated)
            {
                AppUser user = await _usermanager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (user is null)
                {
                    throw new NotFoundException("Oops, no user found :'(");
                }

                var item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
                if (item is null)
                {
                    item = new()
                    {
                        AppUserId = user.Id,
                        ProductId = product.Id,
                        Price = product.Price,
                        Count = 1,
                    };
                    user.BasketItems.Add(item);
                }
                else
                {
                    item.Count++;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                List<BasketCookieItemVM> basket;

                if (Request.Cookies["Basket"] is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);
                    if (item is null)
                    {
                        BasketCookieItemVM basketCookieItemVM = new BasketCookieItemVM
                        {
                            Id = id,
                            Count = 1
                        };
                        basket.Add(basketCookieItemVM);
                    }
                    else
                    {
                        item.Count++;
                    }
                }
                else
                {
                    basket = new List<BasketCookieItemVM>();
                    BasketCookieItemVM basketCookieItemVM = new BasketCookieItemVM
                    {
                        Id = id,
                        Count = 1
                    };
                    basket.Add(basketCookieItemVM);
                }


                string json = JsonConvert.SerializeObject(basket);
                Response.Cookies.Append("Basket", json);
            }


            return RedirectToAction(nameof(Index), "Basket");
        }

        public async Task<IActionResult> Checkout()
        {
            AppUser user = await _usermanager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(pi => pi.Product).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
            OrderVM orderVM = new OrderVM
            {
                BasketItems = user.BasketItems
            };
            return View(orderVM);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(OrderVM orderVM)
        {
            AppUser user = await _usermanager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(pi => pi.Product).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                orderVM.BasketItems = user.BasketItems;
                return View(orderVM);
            }

            decimal total = 0;

            foreach (BasketItem item in user.BasketItems)
            {
                item.Price = item.Product.Price;
                total += item.Count * item.Price;
            }

            Order order = new Order
            {
                Status = null,
                Address = orderVM.Address,
                PurchaseAt = DateTime.UtcNow,
                AppUserId = user.Id,
                BasketItems = user.BasketItems,
                TotalPrice = total
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            string body = @"
                            <html>
                            <head>
                                <style>
                                    table {
                                        border-collapse: collapse;
                                        width: 100%;
                                    }
                                    th, td {
                                        border: 1px solid #dddddd;
                                        text-align: left;
                                        padding: 8px;
                                    }
                                    th {
                                        background-color: #f2f2f2;
                                    }
                                </style>
                            </head>
                            <body>
                                <h2>Order Details</h2>
                                <h4>Thanks for your order!</h4>
                                <table>
                                    <thead>
                                        <tr>
                                            <th>Name</th>
                                            <th>Price</th>
                                            <th>Count</th>
                                        </tr>
                                    </thead>
                                    <tbody>";

            foreach (var item in order.BasketItems)
            {
                body += $@"
                            <tr>
                                <td>{item.Product.Name}</td>
                                <td>{item.Price}</td>
                                <td>{item.Count}</td>
                            </tr>";
            }

            body += @"
                        </tbody>
                    </table>
                </body>
                </html>";


            await _emailService.SendMailAsync(user.Email, "Your Order", body, true);
            return RedirectToAction("Index", "Home");
        }

    }
}



