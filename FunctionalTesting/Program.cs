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
            
            UdpClient uc = new UdpClient(20777);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);

            LiveCoach lc = new LiveCoach(Track.Melbourne);

            Console.WriteLine("Receiving...");
            while (true)
            {
                byte[] rec = uc.Receive(ref ep);
                PacketType pt = CodemastersToolkit.GetPacketType(rec);
                if (pt == PacketType.Motion)
                {
                    MotionPacket mp = new MotionPacket();
                    mp.LoadBytes(rec);
                    lc.InjestPacket(mp);
                }
                else if (pt == PacketType.Lap)
                {
                    LapPacket lp = new LapPacket();
                    lp.LoadBytes(rec);
                    lc.InjestPacket(lp);
                }

                Console.WriteLine("\r" + lc.AtCorner.ToString() + " - " + lc.AtCornerStage.ToString());
            }
            
            
        }

        
    
    }
}
