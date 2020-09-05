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
using ApexVisual.F1_2020.LiveSessionManagement;

namespace FunctionalTesting
{
    class Program
    {

        static void Main(string[] args)
        {
            string path = "D:\\Australia_Qualifying_AlphaTauri.json";
            Console.WriteLine("Reading content.");
            string conten = System.IO.File.ReadAllText(path);
            Console.WriteLine("Deserializing");
            List<byte[]> data = JsonConvert.DeserializeObject<List<byte[]>>(conten);
            Console.WriteLine("Getting packets...");
            Packet[] packets = Packet.BulkLoadAllSessionData(data);


            LiveSessionManager lsm = new LiveSessionManager();


            Console.WriteLine("Goimng");
            foreach (Packet p in packets)
            {
                lsm.InjestPacket(p);
            }

            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(lsm));


        }

        
    
    }
}
