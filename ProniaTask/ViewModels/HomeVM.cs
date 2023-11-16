using System;
using System.Reflection.Metadata;
using ProniaTask.Models;

namespace ProniaTask.ViewModels
{
    public class HomeVM
	{
        public List<Product> Products { get; set; }
        public List<Slide> Slides { get; set; }
    }
}

