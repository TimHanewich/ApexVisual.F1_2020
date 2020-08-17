using System;

namespace ApexVisual.F1_2020.ActivityLogging
{
    public class PackageVersion
    {
        public int Major {get; set;}
        public int Minor {get; set;}
        public int Build {get; set;}
        public int Revision {get; set;}

        public PackageVersion()
        {

        }

        public PackageVersion(int major, int minor, int build, int revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public override string ToString()
        {
            return Major.ToString() + "." + Minor.ToString() + "." + Build.ToString() + "." + Revision.ToString();
        }
    }
}