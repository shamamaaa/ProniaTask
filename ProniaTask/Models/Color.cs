using System;
using System.ComponentModel.DataAnnotations;

namespace ProniaTask.Models
{
	public class Color
	{
        public int Id { get; set; }
        [Required(ErrorMessage = "You must include name")]
        [MaxLength(25, ErrorMessage = "The name must be up to 25 characters")]
        [MinLength(3, ErrorMessage = "The name must be at least 3 characters")]
        public string Name { get; set; }
        public List<ProductColor>? ProductColors { get; set; }
    }
}

