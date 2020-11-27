using System;
using Codemasters.F1_2020;

namespace ApexVisual.F1_2020.Analysis
{
    public class TelemetrySnapshot
    {
        //Identifying data
        public byte LocationNumber {get; set;} //For example, 1, 2, 3, etc. So corner 1, 2, 3, 4, etc, or a speed trap for example.

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
        public WheelDataArray TyreWearPercent {get; set;}
        public WheelDataArray TyreDamagePercent {get; set;}
        public float FrontLeftWingDamage {get; set;}
        public float FrontRightWingDamage {get; set;}
        public float RearWingDamage {get; set;}
        public float ErsStored {get; set;}
    }
}