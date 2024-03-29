﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProniaTask.Areas.ProniaAdmin.ViewModels
{
	public class CreateProductVM
	{
        [Required(ErrorMessage = "You must include name")]
        [MaxLength(25, ErrorMessage = "The name must be up to 25 characters")]
        [MinLength(3, ErrorMessage = "The name must be at least 3 characters")]
        public string Name { get; set; }
        public decimal Price { get; set; }
        [Required(ErrorMessage = "You must include description")]
        [MaxLength(320, ErrorMessage = "The description must be up to 320 characters")]
        [MinLength(5, ErrorMessage = "The description must be at least 5 characters")]
        public string Description { get; set; }
        [Required(ErrorMessage = "You must include SKU")]
        [MaxLength(64, ErrorMessage = "The sku must be up to 64 characters")]
        [MinLength(3, ErrorMessage = "The description must be at least 3 characters")]
        public string SKU { get; set; }

        public IFormFile MainPhoto { get; set; }
        public IFormFile HoverPhoto { get; set; } 
        public List<IFormFile> Photos { get; set; }

        [Required]
        public int? CategoryId { get; set; }

        public List<int> TagIds { get; set; }
        public List<int> ColorIds { get; set; }
        public List<int> SizeIds { get; set; }

        public SelectList? Categories { get; set; }
        public SelectList? Colors { get; set; }
        public SelectList? Sizes { get; set; }
        public SelectList? Tags { get; set; }


    }
}

