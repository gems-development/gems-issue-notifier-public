namespace Gems.TechSupport.Application.Commands.Okdesk;

internal static class Constants
{
    internal static class SkitMessagePatterns
    {
        internal const string AuthorSupport = "Автор: Техническая поддержка Gems";
        internal const string TitleNewComment = "Новый комментарий по заявке";
        internal const string TitleParent = "Вам назначена заявка в СКИТ";
        internal const string TitleSkitResponse = "Ответ согласования в СКИТ";
        internal const string TitleSlaReminder = "Автоматические напоминания SLA";
        internal const string SkitTagPattern = @"\[SKIT\s*#(?<num>\d+)\]";
        internal const string ParentTitleTemplate = "[SKIT #{0}] Вам назначена заявка в СКИТ";
    }
    
    internal static class OkdeskFeatures
    {
        internal const string SkitIssuesProcessing = "SkitIssuesProcessingEnabled";
    }

    internal static class IssueUpdateAuthorType
    {
        internal const string Employee = "employee";
        internal const string Contact = "contact";
    }
}
