using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProniaTask.Models;
using ProniaTask.ViewModels;
using ProniaTask.Utilities.Extensions;
using ProniaTask.Utilities.Enum;

namespace ProniaTask.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _manager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> manager, SignInManager<AppUser> signIn, RoleManager<IdentityRole> roleManager)
        {
            _manager = manager;
            _signInManager = signIn;
            _roleManager = roleManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM userVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            AppUser user = new AppUser
            {
                Name = userVM.Name.Capitalize(),
                Surname = userVM.Surname.Capitalize(),
                UserName = userVM.Username,
                Email = userVM.Email,
                Gender = userVM.Gender
            };

            var result = await _manager.CreateAsync(user,userVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);
                }
                return View();
            }

            await _manager.AddToRoleAsync(user, Role.Member.ToString());
            await _signInManager.SignInAsync(user,false);
            return RedirectToAction("Index","Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM, string? returnUrl)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }

            AppUser user = await _manager.FindByNameAsync(loginVM.UsernameOrEmail);
            if (user is null)
            {
                user = await _manager.FindByEmailAsync(loginVM.UsernameOrEmail);
                if (user is null)
                {
                    ModelState.AddModelError(String.Empty,"Username, email or password is incorrect");
                    return View();
                }
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsRemembered, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(String.Empty, "Your account is temporary locked, please try later.");
                return View();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(String.Empty, "Username, email or password is incorrect");
                return View();
            }

            if (returnUrl is null)
            {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(returnUrl);

        }

        //public async Task<IActionResult> CreateRole()
        //{
        //    foreach (var role in Enum.GetValues(typeof(Role)))
        //    {
        //        if (!(await _roleManager.RoleExistsAsync(role.ToString())))
        //        {
        //            await _roleManager.CreateAsync(new IdentityRole
        //            {
        //                Name = role.ToString()
        //            });
        //        }
        //    }

        //    return RedirectToAction("Index", "Home");
        //}



    }
}

