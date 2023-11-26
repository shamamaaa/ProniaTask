using System;
using System.ComponentModel.DataAnnotations;

namespace ProniaTask.Areas.ProniaAdmin.ViewModels
{
	public class UpdateSizeVM
	{
        [Required(ErrorMessage = "You must include name")]
        [MaxLength(25, ErrorMessage = "The name must be up to 25 characters")]
        [MinLength(1, ErrorMessage = "The name must be at least 3 characters")]
        public string Name { get; set; }
    }
}

