using System;
using Codemasters.F1_2020;
using System.Collections.Generic;

namespace ApexVisual.F1_2020.LiveSessionManagement
{
    public class LiveSessionManager
    {
        private bool Initialized;
        public LiveDriverSessionData[] LiveDriverData {get; set;}

        public LiveSessionManager()
        {
            Initialized = false;
        }

        public void InjestPacket(Packet p)
        {
            if (Initialized == false) //We have not set up the array of live driver data yet
            {
                if (p.PacketType == PacketType.Participants) //We wait for the participant packets to set up
                {
                    ParticipantPacket pp = (ParticipantPacket)p;
                    List<LiveDriverSessionData> NewData = new List<LiveDriverSessionData>();
                    foreach (ParticipantPacket.ParticipantData pd in pp.FieldParticipantData)
                    {
                        LiveDriverSessionData ldsd = new LiveDriverSessionData();
                        ldsd.DriverDisplayName = CodemastersToolkit.GetDriverDisplayNameFromDriver(pd.PilotingDriver);
                        ldsd.TeamColor =  CodemastersToolkit.GetTeamColorByTeam(pd.ManufacturingTeam);
                        NewData.Add(ldsd);
                    }
                    LiveDriverData = NewData.ToArray();
                    Initialized = true;
                }
            }
            else //We already have established a list of live driver session data
            {

                //If it is a lap packet, plug them in one by one
                if (p.PacketType == PacketType.Lap)
                {
                    LapPacket lp = (LapPacket)p;
                    for (int t = 0; t < lp.FieldLapData.Length; t++)
                    {
                        LiveDriverData[t].FeedLapData(lp.FieldLapData[t]);
                    }
                }
                else if (p.PacketType == PacketType.Session) //if it is a session packet, we have to plug the session type into each
                {
                    SessionPacket sp = (SessionPacket)p;
                    foreach (LiveDriverSessionData ldsd in LiveDriverData)
                    {
                        ldsd.SetSessionType(sp.SessionTypeMode);
                    }
                }


            }
        }

    }
}