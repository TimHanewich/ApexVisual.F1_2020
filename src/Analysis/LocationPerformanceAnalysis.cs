using System;
using ApexVisual.F1_2020.Analysis;
using Codemasters.F1_2020;

namespace ApexVisual.F1_2020.Analysis
{
    public class LocationPerformanceAnalysis
    {
        public byte LocationNumber {get; set;}
        public float AverageSpeedKph {get; set;}
        public float AverageGear {get; set;}
        public float AverageDistanceToApex {get; set;}
        /// <summary>
        /// A rating of the of the consistency in this location, taking into account speed, gear, and distance to apex. Lower = more consistent.
        /// </summary>
        public float InconsistencyRating {get; set;}
    }
}