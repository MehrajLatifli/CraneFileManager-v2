using Microsoft.AspNetCore.Mvc.Formatters;
using Serilog.Formatting.Elasticsearch;
using System.Text;
using YamlDotNet.Serialization;

namespace CraneFileManager.Notification.API.TextOutputFormatters
{


    public class YamlOutputFormatter : TextOutputFormatter
    {
        public YamlOutputFormatter()
        {
            SupportedMediaTypes.Add("application/yaml");
            SupportedMediaTypes.Add("text/yaml");
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var serializer = new Serializer();
            var yaml = serializer.Serialize(context.Object);

            return context.HttpContext.Response.WriteAsync(yaml);
        }
    }

}
