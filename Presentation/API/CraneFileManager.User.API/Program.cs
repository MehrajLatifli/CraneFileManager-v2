
using CraneFileManager.Application.Mapper;
using CraneFileManager.Persistence.ServiceExtensions;
using CraneFileManager.User.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text.Json;
using System.Text;
using System.Security.Claims;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;
using CraneFileManager.User.API.TextOutputFormatters;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using Autofac.Core;
using CraneFileManager.Infrastructure.SignalR;
using CraneFileManager.User.API.Controllers.Auth;
using Microsoft.Extensions.DependencyInjection;
using CraneFileManager.User.API.Controllers.Files.File;
using CraneFileManager.User.API.Controllers.Files.FileShare;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using HealthChecks.Azure.Storage.Blobs;
using HealthChecks.Azure.Storage.Files.Shares;
using HealthChecks.Azure.Storage.Queues;
using CraneFileManager.User.API.Events;
using CraneFileManager.Domain.Entities.Configurations;
using Microsoft.Extensions.Options;


namespace CraneFileManager.User.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<AppSettings>(builder.Configuration);

            // Register other services as needed
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);



            builder.AddRedisOutputCache("rediscashe", settings => settings.DisableHealthChecks = false, configureOptions: options => options.ConnectTimeout = 3000);



            builder.Services.AddMvcCore().AddApiExplorer();

            builder.Services.AddSwaggerGenServiceExtensionForUserAPI();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                              .SetIsOriginAllowed(hostName => true);

                });
            });


            builder.Services.AddAuthorization();

            builder.Services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            //builder.Services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();
            //builder.Services.AddSingleton< DefaultAuthorizationPolicyProvider>();




            builder.Services.AddSingleton<NotificationHubService>(provider => new NotificationHubService($"{builder.Configuration.GetValue<string>("NotificationHub")}"));





            builder.Services.AddControllers(options =>
            {
          
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

                options.OutputFormatters.Insert(0, new YamlOutputFormatter());
                options.OutputFormatters.Insert(0, new CsvOutputFormatter());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            })
            .AddNewtonsoftJson()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.DictionaryKeyPolicy = null;
            }).AddXmlSerializerFormatters();




            builder.Services.ApplicationParts("CraneFileManager.User.API");

            builder.Services.AddControllersForAssembly(typeof(AuthController).Assembly);
            builder.Services.AddControllersForAssembly(typeof(FileController).Assembly);
            builder.Services.AddControllersForAssembly(typeof(FileShareController).Assembly);





            builder.Services.AddConfigureFormOptions();
            builder.Services.AddConfigureKestrelServerOptions();



            builder.Services.AddHttpClient();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);

            builder.Services.AddMemoryCache();

            builder.Services.AddPersistenceServices();


            builder.Services.AddRedisConfiguration();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = builder.Configuration["JWT:ValidateAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidateIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
                    NameClaimType = ClaimTypes.Name

                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(accessToken) && accessToken.StartsWith("Bearer "))
                        {
                            context.Token = accessToken.Substring("Bearer ".Length).Trim();
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.APIVersion();


            //builder.WebHost.UseUrls("https://localhost:6000");

            //builder.WebHost.UseUrls("http://*:80", "https://*:443");








            builder.Services.AddRateLimiterServiceExtension();


            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(builder.Configuration["local-1:blob"]);
                clientBuilder.AddQueueServiceClient(builder.Configuration["local-1:queue"]);
            });



            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(builder.Configuration["AzureConnectionStrings:blob"]);
                clientBuilder.AddQueueServiceClient(builder.Configuration["AzureConnectionStrings:queue"]);
            });




            //// Disable when use ApiExceptionFilterAttribute
            builder.Services.AddTransient<ExceptionMiddleware>();
            builder.Services.AddTransient<ApiHealthCheckMiddleware>();
            builder.Services.AddTransient<GeoLocationMiddleware>();


            builder.Services.AddTransient<EventBuses>();


            builder.Host.UseSerilog(builder.Services.AddCustomSerilog());



            builder.Services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
                o.MaximumReceiveMessageSize = 102400000;
            });
            builder.Services.AddSignalRCore();


            builder.Services.AddHealthCheck();




            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    options.ListenAnyIP(6000); 
            //});

            var app = builder.Build();

       


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {

                app.UseHttpsRedirection();
                app.UseHsts();
                app.UseSwagger();

                app.ConfigureSwaggerUI(app.Services, 
                    "/swagger/v1/swagger.json",
                    "Swagger",
                    "CraneFileManager User API",
                    "/swagger-ui/custom.css",
                    "/swagger-ui/custom.js"
                );


            }
            if (app.Environment.IsProduction())
            {

                app.UseHttpsRedirection();
                app.UseHsts();
                app.UseSwagger();

                app.ConfigureSwaggerUI(app.Services,
                    "/swagger/v1/swagger.json",
                    "Swagger",
                    "CraneFileManager User API",
                    "/swagger-ui/custom.css",
                    "/swagger-ui/custom.js"
                );


            }
            else
            {
                app.UseHttpsRedirection();
                app.UseHsts();
                app.UseSwagger();
                app.ConfigureSwaggerUI(app.Services,
                    "/swagger/v1/swagger.json",
                    "Swagger",
                    "CraneFileManager User API",
                    "/swagger-ui/custom.css",
                    "/swagger-ui/custom.js"
                );
            }

            app.UseStaticFiles();

            app.UseHttpLogging();

            app.UseSerilogRequestLogging();



            app.UseRateLimiter();

            app.UseRouting();
            app.UseCors("AllowAllPolicy");



            app.UseAuthentication();
            app.UseAuthorization();




            //app.Use(async (context, next) =>
            //{
            //    var username = context.User?.Identity?.IsAuthenticated != null || true ? context.User.Identity.Name : null;

            //    LogContext.PushProperty("user_name", username);

            //    await next();

            //});


            // Disable when use ApiExceptionFilterAttribute
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<ApiHealthCheckMiddleware>();
            app.UseMiddleware<GeoLocationMiddleware>();


            app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());

            app.MapControllers();

            app.UseOutputCache();
            app.UseResponseCaching();

            app.MapControllerRoute(name: "default", pattern: "{CraneFileManager.User.API}/{action=Index}/{id?}");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>("/notificationHub");
                endpoints.MapHealthChecks("/HealthCheck", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI();
            });

            app.UseHealthChecksUI(config => config.UIPath = "/hc-ui");



            app.Run();
        }
    }
}
