using System;
using ProniaTask.Models;

namespace ProniaTask.ViewModels
{
	public class OrderVM
	{
        public string Address { get; set; }
        public List<BasketItem>? BasketItems { get; set; }
    }
}

