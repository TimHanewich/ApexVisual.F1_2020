using System;
using Codemasters.F1_2020;
using ApexVisual.F1_2020.Analysis;


namespace ApexVisual.F1_2020.LiveCoaching
{
    public delegate void CornerChangedEventHandler(byte new_corner);
    public delegate void CornerStageChangedEventHandler(CornerStage new_stage);
    public delegate void CornerApexTelemetryDataReceivedEventHandler(TelemetryPacket.CarTelemetryData telemetry, TrackLocation optimal_telemetry);
}