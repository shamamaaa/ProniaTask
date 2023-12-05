using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ProniaTask.Areas.ProniaAdmin.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    [Area("ProniaAdmin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

