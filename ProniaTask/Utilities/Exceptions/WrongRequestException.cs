using System;
namespace ProniaTask.Utilities.Exceptions
{
    public class WrongRequestException : Exception
    {
        public WrongRequestException(string message = "Oops, wrong input :'(") : base(message)
        {

        }
    }
}

