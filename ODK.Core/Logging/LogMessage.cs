using System;

namespace ODK.Core.Logging
{
    public class LogMessage
    {
        public LogMessage(int id, string level, string message, DateTime timeStamp, string exception)
        {
            Exception = exception;
            Id = id;
            Level = level;
            Message = message;
            TimeStamp = timeStamp;
        }

        public string Exception { get; }

        public int Id { get; }

        public string Level { get; }

        public string Message { get; }

        public DateTime TimeStamp { get; }
    }
}
