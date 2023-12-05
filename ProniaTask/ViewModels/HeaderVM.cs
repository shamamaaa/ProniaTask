using System;
namespace ProniaTask.ViewModels
{
	public class HeaderVM
	{
        public Dictionary<string, string> Settings { get; set; }
        public List<BasketItemVM> BasketItems { get; set; }
    }
}

