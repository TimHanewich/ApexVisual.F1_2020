using System;

namespace ApexVisual.F1_2020.LiveCoaching
{
    public delegate void CornerChangedEventHandler(byte new_corner);
    public delegate void CornerStageChangedEventHandler(CornerStage new_stage);
}