using System;
using ApexVisual.F1_2020;
using Codemasters.F1_2020.Analysis;
using Codemasters.F1_2020;

namespace ApexVisual.F1_2020.LiveCoaching
{
    public class LiveCoach
    {
        //Vars
        private TrackDataContainer LoadedTrackData;
        private byte AtCorner;
        private CornerStage AtCornerStage;

        //For change-context
        private PacketFrame LastReceivedPackets; //We will never receive a packet frame as a whole, but will use this to store each of them

        public LiveCoach(Track track)
        {
            LoadedTrackData = TrackDataContainer.LoadTrack(track);
            LastReceivedPackets = new PacketFrame();
        }

        public void InjestPacket(Packet p)
        {

            //Coaching
            if (p.PacketType == PacketType.Motion && LastReceivedPackets.Motion != null)
            {
                
            }



            #region "Plug it in"

            if (p.PacketType == PacketType.CarStatus)
            {
                LastReceivedPackets.CarStatus = (CarStatusPacket)p;
            }
            else if (p.PacketType == PacketType.Lap)
            {
                LastReceivedPackets.Lap = (LapPacket)p;
            }
            else if (p.PacketType == PacketType.Motion)
            {
                LastReceivedPackets.Motion = (MotionPacket)p;
            }
            else if (p.PacketType == PacketType.CarTelemetry)
            {
                LastReceivedPackets.Telemetry = (TelemetryPacket)p;
            }


            #endregion
        }

    }
}