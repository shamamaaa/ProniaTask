using System;
using ProniaTask.Models;

namespace ProniaTask.ViewModels
{
	public class ProductVM
	{
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
    }
}

