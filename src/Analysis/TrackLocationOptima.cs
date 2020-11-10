using System;

namespace ApexVisual.F1_2020.Analysis
{
    public class TrackLocationOptima : TrackLocation
    {
        //Optimums
        public float OptimalSpeedMph {get; set;} //Probably a major point of interest
        public sbyte OptimalGear {get; set;} //Probably a major point of interest
        public float OptimalSteer {get; set;}
        public float OptimalThrottle {get; set;}
        public float OptimalBrake {get; set;}
    }
}