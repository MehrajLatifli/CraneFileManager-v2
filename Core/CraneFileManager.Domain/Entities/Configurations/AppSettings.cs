using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Domain.Entities.Configurations
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public Seq Seq { get; set; }
        public string ConnectionAzureStorage { get; set; }
        public JWT JWT { get; set; }
        public RabbitMQ RabbitMQ { get; set; }
        public string NotificationAPICheck { get; set; }
        public string UserAPI { get; set; }
        public string NotificationAPI { get; set; }
        public string FileAPI { get; set; }
        public string APIEnvironment { get; set; }
        public string AllowedHosts { get; set; }

    }

    public class ConnectionStrings
    {
        public string CustomDbConnection { get; set; }
        public string LogConnection { get; set; }
        public string RedisConnection { get; set; }
    }

    public class Seq
    {
        public string SeqConnection { get; set; }
    }

    public class JWT
    {
        public string Secret { get; set; }
        public int TokenValidityInHour { get; set; }
        public int RefreshTokenValidityInDays { get; set; }
        public string ValidateIssuer { get; set; }
        public string ValidateAudience { get; set; }
    }

    public class RabbitMQ
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool DispatchConsumersAsync { get; set; }
    }

}
