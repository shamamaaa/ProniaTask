using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaTask.DAL;
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

            var viewModel = new HeaderVM
            {
                Settings = settings
            };

            return View(viewModel);

        }
    }
}

