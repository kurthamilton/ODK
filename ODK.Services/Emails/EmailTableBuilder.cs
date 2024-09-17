using System.Text;

namespace ODK.Services.Emails;

public class EmailTableBuilder
{
    private readonly StringBuilder _html = new();

    public EmailTableBuilder AddCell(string text)
    {
        _html.Append($"<td>{text}</td>");
        return this;
    }

    public EmailTableBuilder CloseRow()
    {
        _html.AppendLine("</tr>");
        return this;
    }

    public EmailTableBuilder OpenRow()
    {
        _html.Append("<tr>");
        return this;
    }

    public override string ToString()
    {
        var html = new StringBuilder();

        html.AppendLine("<table border=\"1\" style=\"border-collapse: collapse; width: 100%;\" >");
        html.AppendLine("<tbody>");
        html.AppendLine(_html.ToString());
        html.AppendLine("</tbody>");
        html.AppendLine("</table>");

        return html.ToString();
    }
}
