using System.Net;

namespace NZWalks.API.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        private readonly RequestDelegate _nextRequest;

        public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger,
            RequestDelegate nextRequest)
        {
            this._logger = logger;
            this._nextRequest = nextRequest;
        }

        // This method is called by the runtime. Use this method to configure the HTTP request pipeline.
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _nextRequest(httpContext);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();

                // Log the error message
                _logger.LogError(ex, $"{errorId} : {ex.Message}");

                // Return A custom error response to the client
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var errorResponse = new
                {
                    Id = errorId,
                    StatusCode = httpContext.Response.StatusCode,
                    ExceptionMessage = "Something went wrong! We are looking into resolving this."
                };


                await httpContext.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
