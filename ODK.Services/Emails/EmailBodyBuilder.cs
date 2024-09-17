using System.Text;

namespace ODK.Services.Emails;

public class EmailBodyBuilder
{
    private readonly StringBuilder _body = new StringBuilder();

    public EmailBodyBuilder AddLine()
    {
        _body.AppendLine("<hr/>");
        return this;
    }

    public EmailBodyBuilder AddLink(string parameterName)
    {
        _body.Append($"<a href=\"{{{parameterName}}}\">{{{parameterName}}}</a>");
        return this;
    }

    public EmailBodyBuilder AddParagraphLink(string parameterName)
    {
        OpenParagraph();
        AddLink(parameterName);
        CloseParagraph();
        return this;
    }    

    public EmailBodyBuilder AddParagraph(string text)
    {
        OpenParagraph();
        AddText(text);
        CloseParagraph();
        return this;
    }

    public EmailBodyBuilder AddText(string text)
    {
        _body.Append(text);
        return this;
    }

    public EmailBodyBuilder CloseParagraph()
    {
        _body.AppendLine("</p>");
        return this;
    }

    public EmailBodyBuilder OpenParagraph()
    {
        _body.Append("<p>");
        return this;
    }

    public override string ToString() => _body.ToString();
}
