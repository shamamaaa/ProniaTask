using System;
using System.ComponentModel.DataAnnotations;

namespace ProniaTask.ViewModels
{
	public class LoginVM
	{
		[Required]
        [MaxLength(320, ErrorMessage = "It cant't be more than 320 characters.")]
        public string UsernameOrEmail { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		public bool IsRemembered { get; set; }

	}
}
