using NpgsqlTypes;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog;

namespace CraneFileManager.Persistence.LogSettings.ColumnWriters
{
    public class UsernameColumnWriter : ColumnWriterBase
    {
        public UsernameColumnWriter() : base(NpgsqlDbType.Text)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            var (username, value) = logEvent.Properties.FirstOrDefault(p => p.Key == "user_name");
            Log.Information("Username property: {UsernameProperty}", username);
            return value?.ToString() ?? null;
        }

    }
}
