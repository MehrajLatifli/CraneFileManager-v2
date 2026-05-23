using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;

namespace CraneFileManager.User.API.TextOutputFormatters
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            SupportedMediaTypes.Add("text/csv");
            SupportedMediaTypes.Add("application/csv");
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type type)
        {

            return typeof(IEnumerable<object>).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "text/csv";

            var records = context.Object as IEnumerable<object> ?? new[] { context.Object };
            var stringBuilder = new StringBuilder();
            var firstRecord = records.FirstOrDefault();

            if (firstRecord != null)
            {
                var properties = firstRecord.GetType().GetProperties();
                stringBuilder.AppendLine(string.Join(",", properties.Select(p => p.Name)));

                foreach (var record in records)
                {
                    var values = properties.Select(p =>
                    {
                        var value = p.GetValue(record, null);
                        return value is IEnumerable<string> ? string.Join(";", (IEnumerable<string>)value) : value?.ToString();
                    });
                    stringBuilder.AppendLine(string.Join(",", values));
                }
            }

            return response.WriteAsync(stringBuilder.ToString());
        }

    }

}
