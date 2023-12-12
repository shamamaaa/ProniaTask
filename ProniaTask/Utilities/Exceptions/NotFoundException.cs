using System;
namespace ProniaTask.Utilities.Exceptions
{
	public class NotFoundException:Exception
	{
		public NotFoundException(string message) : base(message)
        {
		}
	}
}

