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

        public void IngestPacket(Packet p)
        {
            if (Initialized == false) //We have not set up the array of live driver data yet
            {
                if (p.PacketType == PacketType.Participants) //We wait for the participant packets to set up
                {
                    ParticipantPacket pp = (ParticipantPacket)p;
                    List<LiveDriverSessionData> NewData = new List<LiveDriverSessionData>();
                    for (int t = 0; t < pp.NumberOfActiveCars; t++)
                    {
                        LiveDriverSessionData ldsd = new LiveDriverSessionData();
                        ldsd.SelectedDriver = pp.FieldParticipantData[t].PilotingDriver;
                        ldsd.TeamColor =  CodemastersToolkit.GetTeamColorByTeam(pp.FieldParticipantData[t].ManufacturingTeam);
                        ldsd.SelectedTeam = pp.FieldParticipantData[t].ManufacturingTeam;

                        //The driver display name
                        ldsd.DriverDisplayName = CodemastersToolkit.GetDriverDisplayNameFromDriver(pp.FieldParticipantData[t].PilotingDriver); //If the driver is not recognized (it is a real player, index 100, 101, 102, etc) this will return "Unknown"
                        if (pp.FieldParticipantData[t].IsAiControlled == false) //If it is a player (the above most likely made the displat name 'Unknown', use the player name instead)
                        {
                            ldsd.DriverDisplayName = ApexVisualToolkit.CleanseString(pp.FieldParticipantData[t].Name);
                        }

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
                    for (int t = 0; t < LiveDriverData.Length; t++)
                    {
                        LiveDriverData[t].FeedLapData(lp.FieldLapData[t], lp.SessionTime);
                    }

                    //Supply the driver ahead distance for all cars (except first place)
                    foreach (LiveDriverSessionData ldsd in LiveDriverData)
                    {
                        if (ldsd.Position != 1) //Only do this for cars that are NOT in first place
                        {
                            //Find the car that is directly ahead
                            foreach (LapPacket.LapData ld in lp.FieldLapData)
                            {
                                if (ld.CarPosition == ldsd.Position - 1) //If it is the car ahead
                                {
                                    ldsd.SetDriverAheadData(ld.TotalDistance);
                                }
                            }
                        }
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
                else if (p.PacketType == PacketType.CarStatus)
                {
                    CarStatusPacket csp = (CarStatusPacket)p;
                    for (int t = 0; t < LiveDriverData.Length; t++)
                    {
                        LiveDriverData[t].FeedCarStatusData(csp.FieldCarStatusData[t]);
                    }
                }
            }
        }
    }
}