using System;

namespace ApexVisual.F1_2020.ActivityLogging
{
    public class PackageVersion
    {
        public int Major {get; set;}
        public int Minor {get; set;}
        public int Build {get; set;}
        public int Revision {get; set;}

        public override string ToString()
        {
            return Major.ToString() + "." + Minor.ToString() + "." + Build.ToString() + "." + Revision.ToString();
        }
    }
}