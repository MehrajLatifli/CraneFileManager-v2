using CraneFileManager.Application.Exceptions;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Newtonsoft.Json.Linq;

namespace CraneFileManager.File.API.Middlewares
{

    public class GeoLocationMiddleware : IMiddleware
    {
        private readonly string _dbPath = "wwwroot/GeoLite/GeoLite2Country.mmdb"; private readonly ILogger<GeoLocationMiddleware> _logger;

        public GeoLocationMiddleware(ILogger<GeoLocationMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();

            if (ip != null)
            {
                var country = GetCountryFromIp(/*ip*/"185.146.112.020");
                _logger.LogInformation("Incoming request from IP: {IP}, Country: {Country}", ip, country);

                if (country != "AZ" && country != "TR")
                {
                    _logger.LogWarning("Access denied for IP: {IP}, Country: {Country}", ip, country);
                    throw new ForbiddenException($"You cannot enter from {country}");
                }
            }

            await next(context);
        }

        private string GetCountryFromIp(string ip)
        {
            try
            {
                using (var reader = new DatabaseReader(_dbPath))
                {
                    CountryResponse response = reader.Country(ip);
                    return response.Country.IsoCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving country information for IP: {IP}. Error: {Error}", ip, ex.Message);
                return "Unknown";
            }
        }
    }

}
