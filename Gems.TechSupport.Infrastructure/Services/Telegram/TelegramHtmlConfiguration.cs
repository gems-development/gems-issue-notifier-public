using Ganss.Xss;

namespace Gems.TechSupport.Infrastructure.Services.Telegram;

public static class TelegramHtmlConfiguration
{
    private static readonly HtmlSanitizer _sanitizer;

    private static readonly HashSet<string> _allowedTags = new()
    {
        "b", "strong", "i", "em", "u", "ins", "s", "strike", "del", "code", "pre", "a", "span"
    };

    private static readonly HashSet<string> _allowedAttributes = new()
    {
        "href", "class"
    };
    static TelegramHtmlConfiguration()
    {
        var options = new HtmlSanitizerOptions
        {
            AllowedTags = _allowedTags.ToHashSet(),
            AllowedAttributes = _allowedAttributes.ToHashSet()
        };
        _sanitizer = new HtmlSanitizer(options);
    }

    public static string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return html;

        string withoutNbsp = html.Replace("&nbsp;", " ");
        return _sanitizer.Sanitize(withoutNbsp);
    }
}