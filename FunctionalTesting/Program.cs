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

namespace FunctionalTesting
{
    class Program
    {

        static void Main(string[] args)
        {
            string path = "C:\\Users\\TaHan\\Downloads\\F1 2020 telemetry\\Silverstone Race Leclerc.json";
            string content = System.IO.File.ReadAllText(path);
            List<byte[]> data = JsonConvert.DeserializeObject<List<byte[]>>(content);
            Packet[] packets = Packet.BulkLoadAllSessionData(data);
            
            LiveCoach lc = new LiveCoach(Track.Silverstone);
            lc.ApexTelemetryReceived += MDR;
            lc.CornerChanged += CornerCh;
            lc.CornerStageChanged += CornerStageCh;

            Console.WriteLine("Going through...");
            foreach (Packet p in packets)
            {
                lc.InjestPacket(p);
                Task.Delay(5).Wait();
            }
        }

        static void MDR(TelemetryPacket.CarTelemetryData ctd, TrackLocation tl)
        {
            Console.WriteLine("Tel: " + ctd.SpeedMph.ToString() + "   Opt: " + tl.OptimalSpeedMph.ToString());
        }


        // static void Main(string[] args)
        // {
            
        //     UdpClient uc = new UdpClient(20777);
        //     IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);

        //     LiveCoach lc = new LiveCoach(Track.Melbourne);
        //     lc.CornerChanged += CornerCh;
        //     lc.CornerStageChanged += CornerStageCh;

        //     Console.WriteLine("Receiving...");
        //     while (true)
        //     {
        //         byte[] rec = uc.Receive(ref ep);
        //         PacketType pt = CodemastersToolkit.GetPacketType(rec);
        //         if (pt == PacketType.Motion)
        //         {
        //             MotionPacket mp = new MotionPacket();
        //             mp.LoadBytes(rec);
        //             lc.InjestPacket(mp);
        //         }
        //         else if (pt == PacketType.Lap)
        //         {
        //             LapPacket lp = new LapPacket();
        //             lp.LoadBytes(rec);
        //             lc.InjestPacket(lp);
        //         }
        //     }
            
            
        // }

        static void CornerCh(byte corner)
        {
            Console.WriteLine("New corner: " + corner.ToString());
        }
        
        static void CornerStageCh(CornerStage stage)
        {
            Console.WriteLine("New stage: " + stage.ToString());
        }
    
    }
}
