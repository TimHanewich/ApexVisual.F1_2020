using System;

namespace ApexVisual.F1_2020.ActivityLogging
{
    public class ActivityLog
    {
        public DateTimeOffset TimeStamp {get; set;}
        public string IpAddress {get; set;}
        public ActivityType ActivityType {get; set;}
        public PackageVersion PackageVersion {get; set;}
        public string ActivityId {get; set;}
        public string Notes {get; set;}

        public ActivityLog()
        {
            TimeStamp = DateTimeOffset.Now;
        }

    }
}