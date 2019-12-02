using System.Collections.Generic;
using ODK.Core.Utils;

namespace ODK.Core.Mail
{
    public class Email
    {
        public Email(EmailType type, string subject, string body)
        {
            Body = body;
            Subject = subject;
            Type = type;
        }

        public string Body { get; }

        public string Subject { get; }

        public EmailType Type { get; }

        public Email Interpolate(IDictionary<string, string> parameters)
        {
            string body = Body.Interpolate(parameters);
            string subject = Subject.Interpolate(parameters);

            return new Email(Type, subject, body);
        }
    }
}
