using System;
using ApexVisual.F1_2020;
using System.Collections.Generic;
using Newtonsoft.Json;
using Codemasters.F1_2020;
using Codemasters.F1_2020.Analysis;
using System.Threading.Tasks;
using ApexVisual.F1_2020.LiveCoaching;
using System.Net.Sockets;
using System.Net;
using ApexVisual.F1_2020.ActivityLogging;

namespace FunctionalTesting
{
    class Program
    {

        static void Main(string[] args)
        {
            string path = "C:\\Users\\TaHan\\Downloads\\AZ KEY.txt";
            string content = System.IO.File.ReadAllText(path);
            
            ApexVisualManager avm = ApexVisualManager.Create(content);
            
            ActivityLog al = new ActivityLog();
            al.IpAddress = "7.8.9.0";
            al.ActivityType = ActivityType.Launch;
            al.ActivityId = "launch";
            al.PackageVersion = new PackageVersion(9,8,7,6);
            al.Notes = "First test?";
            
            avm.UploadActivityLogAsync(al).Wait();
        }

        
    
    }
}
