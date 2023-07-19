using System.Data;
using ODK.Core.Logging;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class LogMessageMap : SqlMap<LogMessage>
    {
        public LogMessageMap()
            : base("Logs")
        {
            Property(x => x.Id).IsIdentity();
            Property(x => x.Level);
            Property(x => x.Message);
            Property(x => x.TimeStamp);
            Property(x => x.Exception);
        }

        public override LogMessage Read(IDataReader reader)
        {
            return new LogMessage
            (
                id: reader.GetInt32(0),
                level: reader.GetString(1),
                message: reader.GetString(2),
                timeStamp: reader.GetDateTime(3),
                exception: reader.GetStringOrDefault(4)
            );
        }
    }
}
