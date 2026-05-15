using Gems.TechSupport.Application;

namespace Gems.TechSupport.Extensions;

internal static class ConfgurationManagerExtensions
{
    internal static void CheckOrThrow(this ConfigurationManager configurationManager)
    {
        if (!HasValidIdentifiers(configurationManager.GetSection($"{Constants.ConfigurationSections.Okdesk}:FilterByAssigneeIds")))
            throw new ApplicationException("В конфигурации отсутстует фильтр по ответственному лицу");
    }

    private static bool HasValidIdentifiers(IConfigurationSection section)
    {
        var values = section.Get<int[]>();

        return values?.Length > 0 && values.All(o => o > 0);
    }
}