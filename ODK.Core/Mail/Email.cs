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
    }
}
