﻿using System;
using Microsoft.AspNetCore.Identity;
using ProniaTask.Utilities.Enum;

namespace ProniaTask.Models
{
	public class AppUser : IdentityUser
	{
		public string Name { get; set; }

        public string Surname { get; set; }

        public Gender Gender { get; set; }

        public List<BasketItem> BasketItems { get; set; }

        public List<Order> Orders { get; set; }


    }
}

