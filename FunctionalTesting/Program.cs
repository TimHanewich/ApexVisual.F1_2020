using System;
using ApexVisual.F1_2020;
using System.Collections.Generic;
using Newtonsoft.Json;
using Codemasters.F1_2020;
using System.Threading.Tasks;
using ApexVisual.F1_2020.LiveCoaching;
using System.Net.Sockets;
using System.Net;
using ApexVisual.F1_2020.ActivityLogging;
using ApexVisual.F1_2020.LiveSessionManagement;
using System.IO;
using ApexVisual.F1_2020.Analysis;

namespace FunctionalTesting
{
    class Program
    {

        static void Main(string[] args)
        {   
            string content = System.IO.File.ReadAllText("C:\\Users\\tihanewi\\Downloads\\Silverstone 7 lap trash.json");
            List<byte[]> bytes = JsonConvert.DeserializeObject<List<byte[]>>(content);
            Packet[] packets = Packet.BulkLoadAllSessionData(bytes);
            SessionAnalysis sa = new SessionAnalysis();
            Console.WriteLine("Loading...");
            sa.Load(packets, packets[0].PlayerCarIndex);
            
            foreach (CornerPerformanceAnalysis cpa in sa.Corners)
            {
                Console.WriteLine(JsonConvert.SerializeObject(cpa));
                Console.WriteLine();
                Console.ReadLine();
            }
            

        }

        
    
    }
}
