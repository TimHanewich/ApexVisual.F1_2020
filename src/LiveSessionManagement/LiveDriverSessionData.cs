using System;
using System.Drawing;

namespace ApexVisual.F1_2020.LiveSessionManagement
{
    public class LiveDriverSessionData
    {
        //Session-type agnostic data
        public int Position {get; set;}
        public Color TeamColor {get; set;}
        public string DriverDisplayName {get; set;}
        public LivePitStatus PitStatus {get; set;}

        //Race specific data
        public int Race_LapNumber {get; set;}
        public float Race_GapAhead {get; set;}
        public float Race_LastLapTime {get; set;}
        public int Race_PitCount {get; set;}

        //Qualifying specific data
        public float Qualifying_Sector1Time {get; set;}
        public float Qualifying_Sector2Time {get; set;}
        public float Qualifying_Sector3Time {get; set;}
        public float Qualifying_LapTime {get; set;}
    }
}