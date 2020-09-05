using System;
using System.Drawing;
using Codemasters.F1_2020;

namespace ApexVisual.F1_2020.LiveSessionManagement
{
    public class LiveDriverSessionData
    {
        //Session-type agnostic data
        public int Position {get; set;}
        public Color TeamColor {get; set;}
        public string DriverDisplayName {get; set;}
        
        //Race specific data
        public int Race_LapNumber {get; set;}
        public float Race_GapAhead {get; set;}
        public float Race_LastLapTime {get; set;}
        public int Race_PitCount {get; set;}
        public PitStatus Race_PitStatus {get; set;}

        //Qualifying specific data
        public float Qualifying_Sector1Time {get; set;}
        public float Qualifying_Sector2Time {get; set;}
        public float Qualifying_Sector3Time {get; set;}
        public float Qualifying_LapTime {get; set;}
        public LapPacket.DriverStatus Qualifying_DriverStatus {get; set;}


        #region "Misc Variables for internal tracking"

        private SessionPacket.SessionType ThisSessionType;
        private LapPacket.LapData LastSeenLapData;

        #endregion

        //For position, race lap number, qualifying driver status
        public void FeedLapData(LapPacket.LapData ld)
        {
            Position = ld.CarPosition;
            Race_LapNumber = ld.CurrentLapNumber;
            Qualifying_DriverStatus = ld.CurrentDriverStatus;

            //If it is Qualifying
            if (ThisSessionType != SessionPacket.SessionType.Race && ThisSessionType != SessionPacket.SessionType.Race2) //If it is NOT a race
            {

                //Plug in the driver status
                Qualifying_DriverStatus = ld.CurrentDriverStatus;
                                
                //If we just started a new lap and it is our best lap so far, plug in all of the times
                if (ld.CurrentLapNumber != LastSeenLapData.CurrentLapNumber) //It is a new lap we just started
                {
                    if (ld.LastLapTime == ld.BestLapNumber) //If the last lap (the one we just finished) is the fastest lap we have seen so far, update the s1, s2, s3, and lap time
                    {
                        Qualifying_Sector1Time = (float)LastSeenLapData.Sector1TimeMilliseconds / 1000f;
                        Qualifying_Sector2Time = (float)LastSeenLapData.Sector2TimeMilliseconds / 1000f;
                        Qualifying_Sector3Time = ld.LastLapTime - Qualifying_Sector1Time - Qualifying_Sector2Time;
                        Qualifying_LapTime = ld.LastLapTime;
                    }
                }
            }
            else //If it is a race
            {
                //Lap number
                Race_LapNumber = ld.CurrentLapNumber;

                //Did we just start a new lap? If we did, plug in the last lap time
                if (ld.CurrentLapNumber != LastSeenLapData.CurrentLapNumber)
                {
                    Race_LastLapTime = ld.LastLapTime;
                }

                //Plug in pit status
                Race_PitStatus = ld.CurrentPitStatus;

                //Increment the pit count? If the last one we saw they were in the pit lane but now they are on track, increase it
                if (LastSeenLapData.CurrentPitStatus == PitStatus.PitLane && ld.CurrentPitStatus == PitStatus.OnTrack)
                {
                    Race_PitCount = Race_PitCount + 1;
                }
            }

            //Do the internal log updating for next time
            LastSeenLapData = ld;
        }
    
        //For setting session type
        public void SetSessionType(SessionPacket.SessionType ses_type)
        {
            ThisSessionType = ses_type;
        }
    }
}