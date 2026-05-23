using System.Net.Sockets;

namespace CraneFileManager.File.API.Middlewares
{
    public class ApiHealthCheckMiddleware : IMiddleware
    {
        private readonly HttpClient _httpClient;
        private readonly string _notificationApi;
        private readonly ILogger<ApiHealthCheckMiddleware> _logger;

        public ApiHealthCheckMiddleware(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ApiHealthCheckMiddleware> logger)
        {
            _httpClient = httpClient;
            _notificationApi = configuration["NotificationAPICheck"];
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var response = await _httpClient.GetAsync(_notificationApi);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API responded with an error: {StatusCode}", response.StatusCode);
                    throw new InvalidOperationException("API responded with an error.");
                }

                _logger.LogInformation("API is healthy.");
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is SocketException socketEx)
                {
                    _logger.LogError("API connection refused or closed: {Message}", socketEx.Message);
                    throw new InvalidOperationException($"API connection refused or closed: {socketEx.Message}");
                }
                else
                {
                    _logger.LogError("An error occurred: {Message}", ex.Message);
                    throw new InvalidOperationException($"An error occurred: {ex.Message}");
                }
            }

            await next(context);
        }
    }
}


