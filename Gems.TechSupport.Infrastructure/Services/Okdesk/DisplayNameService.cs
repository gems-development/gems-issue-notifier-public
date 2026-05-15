using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Masking;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Gems.TechSupport.Infrastructure.Services.Okdesk;

internal sealed class DisplayNameService(IOptionsMonitor<MaskingOptions> maskingOptions) : IDisplayNameService
{
    private static readonly Regex EmailMask = new Regex(@"(?<=@)[^@\.]+", RegexOptions.Compiled);

    public string GetDisplayName(string name)
    {
        var options = maskingOptions.CurrentValue;
        if (String.IsNullOrEmpty(name)) {
            return "";
        }
        if( options.Keywords.Any(k =>
            name.Contains(k, StringComparison.OrdinalIgnoreCase)))
        {
            return name;
        }
        if (EmailMask.IsMatch(name))
        {
            return "";
        }
        var nameArr = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (nameArr.Length < 2)
        {
            return nameArr[0];
        }
        var firstName = nameArr[1];
        return firstName;
}
}
