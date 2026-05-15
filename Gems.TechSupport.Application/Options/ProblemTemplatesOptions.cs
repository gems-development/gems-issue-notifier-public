namespace Gems.TechSupport.Application.Options;

public class ProblemTemplatesOptions
{
    public const string ConfigurationSection = "AutoCompletedOptions";
    public required List<KeySolutionParametersPair<string, string>> Templates { get; init; }
    public string UnPublicComment { get; init; } = string.Empty;
    public string TimeEntry { get; init; } = string.Empty;
}
public class KeySolutionParametersPair<K, V>
{
    public K Key { get; set; }
    public V SolutionParameters { get; set; }
}
