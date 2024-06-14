using System;

namespace ODK.Core.Logging
{
    public class ErrorProperty
    {
        public ErrorProperty(Guid errorId, string name, string value)
        {
            ErrorId = errorId;
            Name = name;
            Value = value;
        }

        public Guid ErrorId { get; }

        public string Name { get; }

        public string Value { get; }
    }
}
