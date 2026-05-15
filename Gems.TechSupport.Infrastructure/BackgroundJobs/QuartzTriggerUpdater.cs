using Quartz;

namespace Gems.TechSupport.Infrastructure.BackgroundJobs;
public static class QuartzTriggerUpdater
{
    public static async Task UpdateIntervalSecondsAsync(IJobExecutionContext context, int newIntervalSeconds)
    {
        var oldTrigger = context.Trigger;

        var newTrigger = TriggerBuilder.Create()
            .WithIdentity(context.Trigger.Key)
            .ForJob(context.JobDetail)
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(newIntervalSeconds)
                .RepeatForever()) 
            .Build();

        var simpleTrigger = newTrigger as ISimpleTrigger;
        if (simpleTrigger != null)
        {
            var trigger = oldTrigger as ISimpleTrigger;
            if (trigger != null)
                simpleTrigger.TimesTriggered = trigger.TimesTriggered;
        }

        await context.Scheduler.RescheduleJob(context.Trigger.Key, newTrigger);
    }

    public static async Task UpdateIntervalMinutesAsync(IJobExecutionContext context, int newIntervalMinutes)
    {
        var oldTrigger = context.Trigger;
        var newTrigger = TriggerBuilder.Create()
            .WithIdentity(context.Trigger.Key)
            .ForJob(context.JobDetail)
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(newIntervalMinutes)
                .RepeatForever())
            .Build();

        var simpleTrigger = newTrigger as ISimpleTrigger;
        if (simpleTrigger != null)
        {
            var trigger = oldTrigger as ISimpleTrigger;
            if (trigger != null)
                simpleTrigger.TimesTriggered = trigger.TimesTriggered;
        }

        await context.Scheduler.RescheduleJob(context.Trigger.Key, newTrigger);
    }
}
