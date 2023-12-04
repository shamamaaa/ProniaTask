using System;
using System.ComponentModel.DataAnnotations;
using ProniaTask.Utilities.Enum;

namespace ProniaTask.ViewModels
{
	public class RegisterVM
	{
        [Required]
        [MinLength(4, ErrorMessage = "Username must be at least 4 characters.")]
        [MaxLength(64, ErrorMessage = "Username cant't be more than 64 characters.")]
        public string Username { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters.")]
        [MaxLength(35, ErrorMessage = "Name cant't be more than 35 characters.")]
        public string Name { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Surname must be at least 3 characters.")]
        [MaxLength(35, ErrorMessage = "Surname cant't be more than 35 characters.")]
        public string Surname { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [MinLength(5, ErrorMessage = "Email must be at least 5 characters.")]
        [MaxLength(320, ErrorMessage = "Email cant't be more than 320 characters.")]
        [RegularExpression(@"^[\w]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$", ErrorMessage ="Email isn't suitable" )]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        [Required]
        public Gender Gender { get; set; }


    }
}

