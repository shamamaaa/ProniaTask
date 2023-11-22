using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProniaTask.Models
{
	public class Slide
	{
        public int Id { get; set; }
        [Required(ErrorMessage = "You must include title")]
        [MaxLength(60, ErrorMessage = "The title must be up to 60 characters")]
        [MinLength(3, ErrorMessage = "The title must be at least 3 characters")]
        public string Title { get; set; }
        [MaxLength(25, ErrorMessage = "The subtitle must be up to 25 characters")]
        [MinLength(3, ErrorMessage = "The subtitle must be at least 3 characters")]
        public string Subtitle { get; set; }
        [Required(ErrorMessage = "You must include description")]
        [MaxLength(160, ErrorMessage = "The description must be up to 160 characters")]
        [MinLength(5, ErrorMessage = "The description must be at least 5 characters")]
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        [Required(ErrorMessage = "You must include order")]
        public int Order { get; set; }
        [NotMapped]
        public IFormFile Photo { get; set; }
    }
}

