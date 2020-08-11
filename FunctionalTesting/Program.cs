using System;
using ApexVisual.F1_2020;
using System.Collections.Generic;
using Newtonsoft.Json;
using Codemasters.F1_2020;
using Codemasters.F1_2020.Analysis;
using System.Threading.Tasks;
using ApexVisual.F1_2020.LiveCoaching;

namespace FunctionalTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string path = "C:\\Users\\TaHan\\Downloads\\Silverstone Race Leclerc.json";
            string content = System.IO.File.ReadAllText(path);
            List<byte[]> bytes = JsonConvert.DeserializeObject<List<byte[]>>(content);
            Packet[] packets = CodemastersToolkit.BulkConvertByteArraysToPackets(bytes);


            LiveCoach lc = new LiveCoach(Track.Silverstone);

            string all = "";
            
            foreach (Packet p in packets)
            {
                lc.InjestPacket(p);
                all = all + lc.AtCorner.ToString() + " - " + lc.AtCornerStage.ToString() + Environment.NewLine;
            }

            System.IO.File.WriteAllText("C:\\Users\\TaHan\\Downloads\\TEST.txt", all);

            
        }

        
    
    }
}
