using System;
using Codemasters.F1_2020;

namespace ApexVisual.F1_2020.Analysis
{
    public class TelemetrySnapshot
    {
        //Info about the corner
        public byte CornerNumber {get; set;}
        public TrackLocation CornerData {get; set;}

        //The data
        public MotionPacket.CarMotionData Motion {get; set;}
        public LapPacket.LapData Lap {get; set;}
        public TelemetryPacket.CarTelemetryData Telemetry {get; set;}
        public CarStatusPacket.CarStatusData CarStatus {get; set;}

        //The data
        public float PositionX {get; set;}
        public float PositionY {get; set;}
        public float PositionZ {get; set;}
        public float VelocityX {get; set;}
        public float VelocityY {get; set;}
        public float VelocityZ {get; set;}
        public float gForceLateral {get; set;}
        public float gForceLongitudinal {get; set;}
        public float gForceVertical {get; set;}
        public float Yaw {get; set;}
        public float Pitch {get; set;}
        public float Roll {get; set;}
        public float CurrentLapTime {get; set;}
        public byte CarPosition {get; set;}
        public bool LapInvalid {get; set;}
        public byte Penalties {get; set;}
        public ushort SpeedKph {get; set;}
        public float Throttle {get; set;}
        public float Steer {get; set;}
        public float Brake {get; set;}
        public float Clutch {get; set;}
        public sbyte Gear {get; set;}
        public int EngineRpm {get; set;}
        public bool DrsActive {get; set;}
        public WheelDataArray BrakeTemperature {get; set;}
        public WheelDataArray TyreSurfaceTemperature {get; set;}
        public WheelDataArray TyreInnerTemperature {get; set;}
        public int EngineTemperature {get; set;}
        public FuelMix SelectedFuelMix {get; set;}
        public float FuelLevel {get; set;}
        public WheelDataArray TyreWearPercentage {get; set;}
        public WheelDataArray TyreDamagePercent {get; set;}
        public float FrontLeftWingDamage {get; set;}
        public float FrontRightWingDamage {get; set;}
        public float RearWingDamage {get; set;}
        public float ErsStored {get; set;}

        public float DistanceToApex()
        {
            if (CornerData == null)
            {
                throw new Exception("Unable to calculate distance to Apex: CornerData was null.");
            }

            if (Motion == null)
            {
                throw new Exception("Unable to calculate distance to Apex: Motion data was null. Perhaps this corner record does not exist");
            }

            TrackLocation car_loc = new TrackLocation();
            car_loc.PositionX = Motion.PositionX;
            car_loc.PositionY = Motion.PositionY;
            car_loc.PositionZ = Motion.PositionZ;
            car_loc.Sector = Lap.Sector;

            float distance = ApexVisualToolkit.DistanceBetweenTwoPoints(CornerData, car_loc);
            return distance;
        }

    }
}