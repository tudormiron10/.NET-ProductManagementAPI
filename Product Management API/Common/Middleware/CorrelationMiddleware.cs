using Microsoft.Extensions.Primitives;

namespace Product_Management_API.Common.Middleware
{
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";
        private const string CorrelationIdLogKey = "CorrelationId";

        public CorrelationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<CorrelationMiddleware> logger)
        {
            string correlationId = GetCorrelationId(context);
            
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeaderName] = new StringValues(correlationId);
                return Task.CompletedTask;
            });

            using (logger.BeginScope(new Dictionary<string, object>
                   {
                       [CorrelationIdLogKey] = correlationId
                   }))
            {
                await _next(context);
            }
        }

        private static string GetCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
            {
                return correlationId.ToString();
            }

            return Guid.NewGuid().ToString();
        }
    }
}