using System.Text.RegularExpressions;
using Gems.TechSupport.Application.Abstractions.Masking;
using Microsoft.Extensions.Options;

namespace Gems.TechSupport.Application.Masking;
internal sealed class Masker(IOptionsMonitor<MaskingOptions> maskingOptions): IMasker
{
    private static readonly Regex EmailMask = new Regex(@"(?<=@)[^@\.]+", RegexOptions.Compiled);
    public string MaskFullName(string fullName)
    {
        var options = maskingOptions.CurrentValue;
        if (String.IsNullOrWhiteSpace(fullName) || options.Keywords.Any(k =>
            fullName.Contains(k, StringComparison.OrdinalIgnoreCase)))
        {
            return fullName;
        }
        if (EmailMask.IsMatch(fullName))
        {
            return EmailMask.Replace(fullName, "*****");
        }
        var nameArr = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (nameArr.Length < 2)
        {
            return nameArr[0];
        }
        var lastName = nameArr[0];
        var firstName = nameArr[1];
        lastName = lastName.Length == 1 ? lastName : lastName[0] + ".";
        return $"{lastName} {firstName}";
    }
}

