using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EWP_API_WEB_APP.Models
{
    public class ApiLogger
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLogger> _logger;

        public ApiLogger(RequestDelegate next, ILogger<ApiLogger> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log the incoming request
            var request = context.Request;
            var requestBody = await ReadRequestBody(request);

            _logger.LogWarning("Request - Method: {Method}, Path: {Path}, QueryString: {QueryString}, Body: {Body}",
                request.Method, request.Path, request.QueryString, requestBody);

            // Call the next middleware
            await _next(context);

            // Log the outgoing response
            var response = context.Response;

            _logger.LogWarning("Response - StatusCode: {StatusCode}", response.StatusCode);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            var bodyStream = request.Body;
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            request.Body.Seek(0, SeekOrigin.Begin);
            var requestBody = Encoding.UTF8.GetString(buffer);
            request.Body = bodyStream;
            return requestBody;
        }



    }


}


