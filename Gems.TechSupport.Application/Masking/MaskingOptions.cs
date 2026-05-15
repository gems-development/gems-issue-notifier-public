namespace Gems.TechSupport.Application.Masking;
public class MaskingOptions
{
    public const string ConfigurationSection = "Masking";
    public required IReadOnlyCollection<string> Keywords { get; init; }
}

