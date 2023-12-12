using System;
namespace ProniaTask.Middlewares
{
	public class GlobalExceptionHandlerMiddleware
	{
		private readonly RequestDelegate _next;

		public GlobalExceptionHandlerMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                string errorPath = Path.Combine("/Home", "ErrorPage", $"?error={ex.Message}");
                context.Response.Redirect(errorPath);
            }
        }

	}
}

