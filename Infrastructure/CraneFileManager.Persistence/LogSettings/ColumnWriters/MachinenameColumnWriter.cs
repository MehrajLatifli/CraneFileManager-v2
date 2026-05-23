using NpgsqlTypes;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraneFileManager.Persistence.LogSettings.ColumnWriters
{
    public class MachinenameColumnWriter : ColumnWriterBase
    {
        public MachinenameColumnWriter() : base(NpgsqlDbType.Text)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            var (machinename, value) = logEvent.Properties.FirstOrDefault(p => p.Key == "machine_name");
            Log.Information("Machinename property: {MachinenameProperty}", machinename);
            return value?.ToString() ?? null;
        }



    }
}
