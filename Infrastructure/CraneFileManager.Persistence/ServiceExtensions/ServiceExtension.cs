using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using ConfigurationManager = Microsoft.Extensions.Configuration.ConfigurationManager;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpLogging;
using System.Collections.ObjectModel;
using System.Data;
using Serilog.Core;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.Extensions.Options;
using Serilog.Sinks.PostgreSQL;
using CraneFileManager.Persistence.LogSettings.ColumnWriters;
using CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext;
using CraneFileManager.Application.Repositories.Custom;
using CraneFileManager.Persistence.Repositories.Custom;
using Microsoft.Data.SqlClient;
using CraneFileManager.Application.Services.Abstract;
using CraneFileManager.Application.Services.Concrete;
using StackExchange.Redis;
using Humanizer.Configuration;
using NuGet.Configuration;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using CraneFileManager.Domain.Entities.IdentityAuth;
using CraneFileManager.Application.Mapper.DTO.UserDTO;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using System.Net.Mime;
using System.Text.Json;
using CraneFileManager.Application.Cache.RedisCachePatterns.Abstract;
using CraneFileManager.Application.RedisCachePatterns.Concrete;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using CraneFileManager.Application.Mapper.DTO.FileDTO;
using CraneFileManager.Application.Mapper.DTO.FileShareDTO;
using Azure.Storage.Blobs;
using HealthChecks.Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using CraneFileManager.Infrastructure.RabbitMQPattern;
using CraneFileManager.Application.MessageBroker.RabbitMQ.Custom;
using RabbitMQ.Client;
using CraneFileManager.Infrastructure;
using CraneFileManager.Domain.Entities.Configurations;
using CraneFileManager.Application.Mapper.DTO.FileTrashCanDTO;

namespace CraneFileManager.Persistence.ServiceExtensions
{
    public static class ServiceExtension
    {

        public static string GetCustomDbConnectionString(this IOptions<AppSettings> appSettings)
        {
            var connectionString = appSettings.Value.ConnectionStrings.CustomDbConnection;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "CustomDbConnection string cannot be null or empty.");
            }

            return connectionString;
        }

        public static string GetConnectionStringAzure(this IOptions<AppSettings> appSettings)
        {
            var connectionStringAzure = appSettings.Value.ConnectionAzureStorage;

            if (string.IsNullOrEmpty(connectionStringAzure))
            {
                throw new ArgumentNullException(nameof(connectionStringAzure), "Azure Storage connection string cannot be null or empty.");
            }

            return connectionStringAzure;
        }

        public static string GetRedisConnectionString(this IOptions<AppSettings> appSettings)
        {
            var redisConnectionString = appSettings.Value.ConnectionStrings.RedisConnection;

            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new ArgumentNullException(nameof(redisConnectionString), "Redis connection string cannot be null or empty.");
            }

            return redisConnectionString;
        }

        public static string GetRabbitmqConnectionString(this IOptions<AppSettings> appSettings)
        {
            var hostName = appSettings.Value.RabbitMQ.HostName;
            var userName = appSettings.Value.RabbitMQ.UserName;
            var password = appSettings.Value.RabbitMQ.Password;
            var port = appSettings.Value.RabbitMQ.Port;

            // Ensure all necessary values are present
            if (string.IsNullOrEmpty(hostName))
            {
                throw new ArgumentNullException(nameof(hostName), "RabbitMQ HostName cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName), "RabbitMQ UserName cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "RabbitMQ Password cannot be null or empty.");
            }

            if (port <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "RabbitMQ Port must be a positive number.");
            }

            // Build the RabbitMQ connection string
            var rabbitMqConnectionString = $"amqp://{userName}:{password}@{hostName}:{port}/";

            return rabbitMqConnectionString;
        }

        public static string GetLogConnectionString(this IOptions<AppSettings> appSettings)
        {
            var logConnectionString = appSettings.Value.ConnectionStrings.LogConnection;

            if (string.IsNullOrEmpty(logConnectionString))
            {
                throw new ArgumentNullException(nameof(logConnectionString), "Seg connection string cannot be null or empty.");
            }

            return logConnectionString;
        }

        public static string GetSeqConnectionConnectionString(this IOptions<AppSettings> appSettings)
        {
            var segConnectionString = appSettings.Value.Seq.SeqConnection;

            if (string.IsNullOrEmpty(segConnectionString))
            {
                throw new ArgumentNullException(nameof(segConnectionString), "Seg connection string cannot be null or empty.");
            }

            return segConnectionString;
        }



        public static void AddRedisConfiguration(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {

                var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();
                var redisConnectionString = appSettings.GetRedisConnectionString();

                if (string.IsNullOrEmpty(redisConnectionString))
                {
                    throw new ArgumentNullException(nameof(redisConnectionString), "Redis connection string cannot be null or empty.");
                }

                var options = ConfigurationOptions.Parse(redisConnectionString);

                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 3000;
                options.ResponseTimeout = 3000;



                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options));

                services.AddScoped<IAuthCacheService<UserDTOforGetandGetAll>, AuthCacheService<UserDTOforGetandGetAll>>();
                services.AddScoped<IAuthCacheService<UserDTOforUpdate>, AuthCacheService<UserDTOforUpdate>>();
                services.AddScoped<IAuthCacheService<UserDTOforCreate>, AuthCacheService<UserDTOforCreate>>();
                services.AddScoped<IAuthCacheService<GetUserDTOModel>, AuthCacheService<GetUserDTOModel>>();

                services.AddScoped<IFileCacheService<FileDTOforGetandGetAll>, FileCacheService<FileDTOforGetandGetAll>>();
                services.AddScoped<IFileCacheService<FileDTOforUpdate>, FileCacheService<FileDTOforUpdate>>();
                services.AddScoped<IFileCacheService<FileDTOforCreate>, FileCacheService<FileDTOforCreate>>();

                services.AddScoped<IFileShareCacheService<FileShareDTOforGetandGetAll>, FileShareCacheService<FileShareDTOforGetandGetAll>>();
                services.AddScoped<IFileShareCacheService<FileShareDTOforUpdate>, FileShareCacheService<FileShareDTOforUpdate>>();
                services.AddScoped<IFileShareCacheService<FileShareDTOforCreate>, FileShareCacheService<FileShareDTOforCreate>>();

                services.AddScoped<IFileTrashCanCacheService<FileTrashCanDTOforGetandGetAll>, FileTrashCanCacheService<FileTrashCanDTOforGetandGetAll>>();
                services.AddScoped<IFileTrashCanCacheService<FileTrashCanDTOforUpdate>, FileTrashCanCacheService<FileTrashCanDTOforUpdate>>();
                services.AddScoped<IFileTrashCanCacheService<FileTrashCanDTOforCreate>, FileTrashCanCacheService<FileTrashCanDTOforCreate>>();
            }
        }

        public static string GetUserAPIConnectionString(this IOptions<AppSettings> appSettings)
        {
            var aPIConnectionString = appSettings.Value.UserAPI;

            if (string.IsNullOrEmpty(aPIConnectionString))
            {
                throw new ArgumentNullException(nameof(aPIConnectionString), "UserAPI connection string cannot be null or empty.");
            }

            return aPIConnectionString;
        }

        public static string GetFileAPIConnectionString(this IOptions<AppSettings> appSettings)
        {
            var aPIConnectionString = appSettings.Value.FileAPI;

            if (string.IsNullOrEmpty(aPIConnectionString))
            {
                throw new ArgumentNullException(nameof(aPIConnectionString), "FileAPI connection string cannot be null or empty.");
            }

            return aPIConnectionString;
        }

        public static string GetNotificationAPIConnectionString(this IOptions<AppSettings> appSettings)
        {
            var aPIConnectionString = appSettings.Value.NotificationAPI;

            if (string.IsNullOrEmpty(aPIConnectionString))
            {
                throw new ArgumentNullException(nameof(aPIConnectionString), "NotificationAPI connection string cannot be null or empty.");
            }

            return aPIConnectionString;
        }


        private static string AppendRedisOptions(string connectionString)
        {
            if (!connectionString.Contains("abortConnect="))
            {
                connectionString += ";abortConnect=false";
            }

            if (!connectionString.Contains("connectTimeout="))
            {
                connectionString += ";connectTimeout=30000";
            }

            if (!connectionString.Contains("responseTimeout="))
            {
                connectionString += ";responseTimeout=30000";
            }

            return connectionString;
        }



        public static void AddPersistenceServices(this IServiceCollection services)
        {

            using (var serviceProvider = services.BuildServiceProvider())
            {

                var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();

                // Get connection strings using IOptions
                var customDbConnectionString = appSettings.GetCustomDbConnectionString();
                var azureConnectionString = appSettings.GetConnectionStringAzure();  // Fetch Azure connection string
                var redisConnectionString = appSettings.GetRedisConnectionString();  // Fetch Redis connection string



                services.AddDbContext<CraneFileManagerContext>(options => options.UseSqlServer(appSettings.GetCustomDbConnectionString()));







                services.AddScoped<IAuthService, AuthServiceManager>();
                services.AddScoped<IFileService, FileServiceManager>();
                services.AddScoped<INotificationService, NotificationServiceManager>();

                services.AddScoped<IUserReadRepository, UserReadRepository>();
                services.AddScoped<IUserWriteRepository, UserWriteRepository>();

                services.AddScoped<IRoleReadRepository, RoleReadRepository>();
                services.AddScoped<IRoleWriteRepository, RoleWriteRepository>();

                services.AddScoped<IUserRoleReadRepository, UserRoleReadRepository>();
                services.AddScoped<IUserRoleWriteRepository, UserRoleWriteRepository>();

                services.AddScoped<IUserClaimReadRepository, UserClaimReadRepository>();
                services.AddScoped<IUserClaimWriteRepository, UserClaimWriteRepository>();

                services.AddScoped<IRoleClaimReadRepository, RoleClaimReadRepository>();
                services.AddScoped<IRoleClaimWriteRepository, RoleClaimWriteRepository>();

                services.AddScoped<IUserPermissionReadRepository, UserPermissionReadRepository>();
                services.AddScoped<IUserPermissionWriteRepository, UserPermissionWriteRepository>();

                services.AddScoped<IRolePermissionReadRepository, RolePermissionReadRepository>();
                services.AddScoped<IRolePermissionWriteRepository, RolePermissionWriteRepository>();

                services.AddScoped<INotificationReadRepository, NotificationReadRepository>();
                services.AddScoped<INotificationWriteRepository, NotificationWriteRepository>();

                services.AddScoped<IUserNotificationReadRepository, UserNotificationReadRepository>();
                services.AddScoped<IUserNotificationWriteRepository, UserNotificationWriteRepository>();

                services.AddScoped<IFileReadRepository, FileReadRepository>();
                services.AddScoped<IFileWriteRepository, FileWriteRepository>();

                services.AddScoped<IFileShareReadRepository, FileShareReadRepository>();
                services.AddScoped<IFileShareWriteRepository, FileShareWriteRepository>();

                services.AddScoped<IFileTypeReadRepository, FileTypeReadRepository>();
                services.AddScoped<IFileTypeWriteRepository, FileTypeWriteRepository>();

                services.AddScoped<IUserFileReadRepository, UserFileReadRepository>();
                services.AddScoped<IUserFileWriteRepository, UserFileWriteRepository>();

                services.AddScoped<IFileTrashCanReadRepository, FileTrashCanReadRepository>();
                services.AddScoped<IFileTrashCanWriteRepository, FileTrashCanWriteRepository>();

                services.AddScoped<IRabbitMQService, RabbitMQService>();


            }


        }

        public static void AddConfigureFormOptions(this IServiceCollection services)
        {
            services.Configure<FormOptions>(x =>
            {
                x.MultipartBodyLengthLimit = long.MaxValue;
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });
        }

        public static void AddConfigureKestrelServerOptions(this IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = long.MaxValue;
                options.Limits.MaxRequestBufferSize = long.MaxValue;
                options.Limits.MaxRequestLineSize = int.MaxValue;
            });
        }



        public static void ApplicationParts(this IServiceCollection services, string apiName)
        {

            services.AddControllers()
                 .ConfigureApplicationPartManager(apm =>
                 {

                     var removedParts = apm.ApplicationParts
                       .Where(part => part.Name != $"{apiName}")
                       .ToList();

                     foreach (var part in removedParts)
                     {
                         apm.ApplicationParts.Remove(part);
                     }

                 });
        }


        public static void ConfigureSwaggerUI(this IApplicationBuilder app,
                                              IServiceProvider serviceProvider,
                                              string swaggerEndpoint,
                                              string routePrefix,
                                              string apiTitle,
                                              string cssPath,
                                              string jsPath)
        {
            var apiVersioningOptions = serviceProvider.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
            var defaultApiVersion = apiVersioningOptions.DefaultApiVersion.ToString();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(swaggerEndpoint, $"{apiTitle} {defaultApiVersion}"); c.InjectStylesheet($"{cssPath}");
                c.InjectJavascript($"{jsPath}");
                c.RoutePrefix = $"{routePrefix}";
                c.DocumentTitle = apiTitle;
            });
        }


        public static void AddControllersForAssembly(this IServiceCollection services, Assembly assembly)
        {
            services.AddControllers().PartManager.ApplicationParts.Clear();
            services.AddControllers()
                    .PartManager.ApplicationParts.Add(new AssemblyPart(assembly));
        }


        public static void APIVersion(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"));
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
        }

        public static IServiceCollection AddSwaggerGenServiceExtensionForUserAPI(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {

                var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();


                var apiVersioningOptions = services.BuildServiceProvider().GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
                var defaultApiVersion = apiVersioningOptions.DefaultApiVersion;

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = $"CraneFileManager UserAPI",
                        Version = $"v{defaultApiVersion}",
                        Description = $"Environment: {appSettings.Value.APIEnvironment}"
                    });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter `Bearer` [space] and then your valid token in the text input below. \r\n\r\n Example: \"Bearer apikey \""
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }, new string[]{}
                }
            });
                });

                return services;
            }
        }
        public static IServiceCollection AddSwaggerGenServiceExtensionForFileAPI(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {

                var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();


                var apiVersioningOptions = services.BuildServiceProvider().GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
                var defaultApiVersion = apiVersioningOptions.DefaultApiVersion;

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = $"CraneFileManager FileAPI",
                        Version = $"v{defaultApiVersion}",
                        Description = $"Environment: {appSettings.Value.APIEnvironment}"
                    });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter `Bearer` [space] and then your valid token in the text input below. \r\n\r\n Example: \"Bearer apikey \""
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }, new string[]{}
                }
            });
                });

                return services;
            }
        }


        public static IServiceCollection AddSwaggerGenServiceExtensionForNotificationAPI(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {

                var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();

                var apiVersioningOptions = services.BuildServiceProvider().GetRequiredService<IOptions<ApiVersioningOptions>>().Value;
                var defaultApiVersion = apiVersioningOptions.DefaultApiVersion;

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = $"CraneFileManager NotificationAPI",
                        Version = $"v{defaultApiVersion}",
                        Description = $"Environment: {appSettings.Value.APIEnvironment}"
                    });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter `Bearer` [space] and then your valid token in the text input below. \r\n\r\n Example: \"Bearer apikey \""
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    { 
                        { new OpenApiSecurityScheme {

                            Reference = new OpenApiReference
                            {
                              Type = ReferenceType.SecurityScheme,
                              Id = "Bearer"
                            }}, new string[]{}
                        }
                    });
                });

                return services;
            }
        }


        public static Logger AddCustomSerilog(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {

                var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();



                var logConnection = appSettings.GetLogConnectionString();
                var seqConnection = appSettings.GetSeqConnectionConnectionString();

                if (string.IsNullOrEmpty(logConnection))
                {
                    throw new ArgumentNullException(nameof(logConnection), "Log connection string cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(seqConnection))
                {
                    throw new ArgumentNullException(nameof(seqConnection), "Seq connection string cannot be null or empty.");
                }

                services.AddHttpLogging(logging =>{
                    logging.LoggingFields = HttpLoggingFields.All;
                    logging.RequestHeaders.Add("sec-ch-ua");
                    logging.ResponseHeaders.Add("CraneFileManager.API");
                    logging.MediaTypeOptions.AddText("application/javascript");
                    logging.RequestBodyLogLimit = 4096;
                    logging.ResponseBodyLogLimit = 4096;
   
                });

                var fileName = "log.txt";
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var logDirectory = Path.Combine(webRootPath, "logs");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                var logFilePath = Path.Combine(logDirectory, fileName);

                Logger log = new LoggerConfiguration()
                    .WriteTo.OpenTelemetry()
                    .WriteTo.Console()
                    .WriteTo.File(logFilePath)
                    .WriteTo.PostgreSQL(logConnection, "Logs", needAutoCreateTable: true,columnOptions: new Dictionary<string, ColumnWriterBase>
                    {
                    
                        { "message", new RenderedMessageColumnWriter() },
                    
                        { "message_template", new MessageTemplateColumnWriter() },
                   
                        { "level", new LevelColumnWriter() },
                  
                        { "time_stamp", new TimestampColumnWriter() },
                  
                        { "exceptions", new ExceptionColumnWriter() },
                  
                        { "log_event", new LogEventSerializedColumnWriter() },
                 
                        { "user_name", new UsernameColumnWriter() },
                 
                        { "machine_name", new MachinenameColumnWriter() }
           
                    })
                    .WriteTo.Seq(seqConnection, restrictedToMinimumLevel: LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .MinimumLevel.Information()
                    .CreateLogger();



                return log;
            }
        }

        public static void AddHealthCheck(this IServiceCollection services)
        {


            using (var serviceProvider = services.BuildServiceProvider())
            {

                var appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>();


  

                services.AddSingleton(sp =>new BlobServiceClient(appSettings.GetConnectionStringAzure()));

           

                var factory = new ConnectionFactory()
                {
                    Uri = new Uri($"{appSettings.GetRabbitmqConnectionString()}"),
                    AutomaticRecoveryEnabled = true
                };

                var connection = factory.CreateConnection();

                services.AddSingleton(connection);



                services.AddHealthChecks()
                      
                    .AddSqlServer(connectionString: appSettings.GetCustomDbConnectionString(), failureStatus: HealthStatus.Degraded)
                    .AddRedis(appSettings.GetRedisConnectionString(), "RedisHealth", failureStatus: HealthStatus.Degraded)
                    .AddNpgSql(appSettings.GetLogConnectionString())
                    .AddSignalRHub("https://localhost:7171/notificationHub", failureStatus: HealthStatus.Degraded)
                    .AddDbContextCheck<CraneFileManagerContext>(appSettings.GetCustomDbConnectionString(), failureStatus: HealthStatus.Degraded)
                    .AddRabbitMQ()


                    .AddAzureBlobStorage(optionsFactory: sp => new AzureBlobStorageHealthCheckOptions()
                    {
                        ContainerName = "profile-images"
                    }, name: "AddAzureBlobStorage-profileimages", failureStatus: HealthStatus.Degraded)
                    .AddAzureBlobStorage(optionsFactory: sp => new AzureBlobStorageHealthCheckOptions()
                    {
                        ContainerName = "user-files"
                    }, name: "AddAzureBlobStorage-userfiles", failureStatus: HealthStatus.Degraded)

                    .AddUrlGroup(new Uri(appSettings.GetUserAPIConnectionString()), "User.API", HealthStatus.Degraded)
                    .AddUrlGroup(new Uri(appSettings.GetFileAPIConnectionString()), "File.API", HealthStatus.Degraded)
                    .AddUrlGroup(new Uri(appSettings.GetNotificationAPIConnectionString()), "Notification.API", HealthStatus.Degraded);


                services.AddHealthChecksUI(setup =>
                {
                    setup.SetEvaluationTimeInSeconds(10);
                    setup.MaximumHistoryEntriesPerEndpoint(250);
                }).AddInMemoryStorage();



            }
        }

        public static void AddRateLimiterServiceExtension(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {

                    if (httpContext.Request.Path.StartsWithSegments("/api/v1/Auth/registerAdmin") ||
                    httpContext.Request.Path.StartsWithSegments("/api/v1/Auth/registerUser") ||
                    httpContext.Request.Path.StartsWithSegments("/api/v1/Auth/profile"))
                    {
                        return RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Request.Headers.Host.ToString(), partition =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 1,
                                AutoReplenishment = true,
                                Window = TimeSpan.FromSeconds(1)
                            });
                    }



                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey: "default", partition =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = int.MaxValue,
                            AutoReplenishment = true,
                            Window = TimeSpan.FromSeconds(1)
                        });
                });

                options.OnRejected = async (context, token) =>
                {

                    context.HttpContext.Response.StatusCode = 429;
                    context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize("Too many requests. Please try again later... "), cancellationToken: token);
                };
            });
        }
    }
}
