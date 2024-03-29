﻿using System;
using System.ComponentModel.DataAnnotations;

namespace ProniaTask.Models
{
	public class Product
	{
		public int Id { get; set; }
        [Required(ErrorMessage = "You must include name")]
        [MaxLength(25, ErrorMessage = "The name must be up to 25 characters")]
        [MinLength(3, ErrorMessage = "The name must be at least 3 characters")]
        public string Name { get; set; }
        [Range(1, 2147483647, ErrorMessage = "The price must be at least 0.")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "You must include description")]
        [MaxLength(320, ErrorMessage = "The description must be up to 320 characters")]
        [MinLength(5, ErrorMessage = "The description must be at least 5 characters")]
        public string Description { get; set; }
        [Required(ErrorMessage = "You must include SKU")]
        [MaxLength(64, ErrorMessage = "The sku must be up to 64 characters")]
        [MinLength(3, ErrorMessage = "The description must be at least 3 characters")]
        public string SKU { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
        public List<ProductTag>? ProductTags { get; set; }
        public List<ProductColor>? ProductColors { get; set; }
        public List<ProductSize>? ProductSizes { get; set; }

    }

}
