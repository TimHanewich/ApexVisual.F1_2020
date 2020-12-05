using System;

namespace ApexVisual.F1_2020.ActivityLogging
{
    public class ActivityLog
    {
        public Guid SessionId {get; set;} //A unique identifier for this session. A new one is created when launched and then the remainder follow suit.
        public string Username {get; set;}
        public DateTimeOffset TimeStamp {get; set;}
        public ApplicationType ApplicationId {get; set;}
        public ActivityType ActivityId {get; set;}
        public PackageVersion PackageVersion {get; set;}
        public string Note {get; set;}

        public ActivityLog(Guid? use_id = null)
        {
            TimeStamp = DateTimeOffset.Now;
            if (use_id != null)
            {
                SessionId = use_id.Value;
            }
            else
            {
                SessionId = Guid.NewGuid();
            }
        }

    }
}