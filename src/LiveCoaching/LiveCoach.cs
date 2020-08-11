using System;
using ApexVisual.F1_2020;
using Codemasters.F1_2020.Analysis;
using Codemasters.F1_2020;

namespace ApexVisual.F1_2020.LiveCoaching
{
    public class LiveCoach
    {
        //Vars
        private TrackDataContainer LoadedTrackData;
        public byte AtCorner;
        public CornerStage AtCornerStage;
        private bool CornerLockedOnAtLeastOnceAlready = false;

        //For change-context
        private PacketFrame LastReceivedPackets; //We will never receive a packet frame as a whole, but will use this to store each of them

        public LiveCoach(Track track)
        {
            LoadedTrackData = TrackDataContainer.LoadTrack(track);
            LastReceivedPackets = new PacketFrame();
        }

        public void InjestPacket(Packet p)
        {

            //Coaching
            //In order for this to happen, a few things must happen... (i will make the asssumption that the packets are UP TO DATE!)
            //This has to be a motion packet (this contains the X,Y, and Z position)
            //We have to already have seen the previous motion packet (this is used to see direction)
            //We have to already have seen the previous lap packet (this is used to see what sector we are in)
            if (p.PacketType == PacketType.Motion && LastReceivedPackets.Motion != null && LastReceivedPackets.Lap != null)
            {
                //Set up the variables that we will use
                MotionPacket mp = (MotionPacket)p;
                TrackLocation my_loc = new TrackLocation() {PositionX = mp.FieldMotionData[mp.PlayerCarIndex].PositionX, PositionY = mp.FieldMotionData[mp.PlayerCarIndex].PositionY, PositionZ = mp.FieldMotionData[mp.PlayerCarIndex].PositionZ};
                
                
                if (CornerLockedOnAtLeastOnceAlready) //We already have identified at least one corner that we have hit the apex of, so we know that the driver is either leaving that previous corner or entering the next corner
                {
                    
                }


                
                //Find out what corner we are closest to
                byte nearest_corner = 0;
                float nearest_corner_distance = float.MaxValue;
                for (int i = 0; i < LoadedTrackData.Corners.Length; i++) //Loop through each corner, measure the distance, and if this one is closest save it
                {
                    TrackLocation tl = LoadedTrackData.Corners[i];
                    if (tl.Sector == LastReceivedPackets.Lap.FieldLapData[LastReceivedPackets.Lap.PlayerCarIndex].Sector) //We will only evaluate corners that are in the current sector we are in.
                    {
                        float this_distance = CodemastersToolkit.DistanceBetweenTwoPoints(my_loc, tl);
                        if (this_distance < nearest_corner_distance)
                        {
                            nearest_corner_distance = this_distance;
                            nearest_corner = (byte)(i + 1);
                        }
                    }
                }
                

                //If they are within ___ (we'll just say 12 for now) units of distance from the coner, we are AT the corner
                if (nearest_corner_distance < 15)
                {
                    AtCorner = nearest_corner;
                    AtCornerStage = CornerStage.Apex;
                }
                else
                {
                    
                }



                /////OLD BELOW///////////////////////

                //Set that nearest corner data
                AtCorner = nearest_corner;


                //Now that we know what corner we are closest to, see if we are moving away from it or towards it
                //The question we need to answer - are we approaching that corner, we're at the corner, or are we moving away from it
                //ALSO - take into account that other corners on the track may be closer (i.e. through a wall) than the one we are going towards
                    //To handle this, we will do this
                    //If we are within the threshold to say we are "at the corner apex", just mark it down.
                    //If we are NOT at that limit (we are moving away or closer)
                //So this is what we will do...
                //First - If they are within ___ (we'll just say 12 for now) units of distance from the coner, we are AT the corner
                //If we are NOT at the corner, find out if we are moving away from it or moving closer to it
                if (nearest_corner_distance <= 15) //We are AT the corner
                {
                    AtCornerStage = CornerStage.Apex;
                }
                else //We are either approaching it or are moving away from it. Check!
                {
                    float current_distance = nearest_corner_distance; //My current distance from the corner
                    float last_distance = 0; // My last distance to the corner
                    //The above variables will be compared to see what direction we are moving in

                    //Find the last distance
                    TrackLocation MyLastLocation = new TrackLocation() {PositionX = LastReceivedPackets.Motion.FieldMotionData[LastReceivedPackets.Motion.PlayerCarIndex].PositionX, PositionY = LastReceivedPackets.Motion.FieldMotionData[LastReceivedPackets.Motion.PlayerCarIndex].PositionY, PositionZ = LastReceivedPackets.Motion.FieldMotionData[LastReceivedPackets.Motion.PlayerCarIndex].PositionZ, Sector = LastReceivedPackets.Lap.FieldLapData[LastReceivedPackets.Lap.PlayerCarIndex].Sector};
                    TrackLocation SubjectCorner = LoadedTrackData.Corners[nearest_corner-1]; //Minus one because this is an index
                    last_distance = CodemastersToolkit.DistanceBetweenTwoPoints(MyLastLocation, SubjectCorner);

                    //So did we get closer or further away?
                    if (current_distance < last_distance) //We got closer - this is entry
                    {
                        AtCornerStage = CornerStage.Entry;
                    }
                    else if (current_distance > last_distance) //We got further away - this is exit
                    {
                        AtCornerStage = CornerStage.Exit;
                    }
                }
            }



            #region "Save this packet"

            if (p.PacketType == PacketType.CarStatus)
            {
                LastReceivedPackets.CarStatus = (CarStatusPacket)p;
            }
            else if (p.PacketType == PacketType.Lap)
            {
                LastReceivedPackets.Lap = (LapPacket)p;
            }
            else if (p.PacketType == PacketType.Motion)
            {
                LastReceivedPackets.Motion = (MotionPacket)p;
            }
            else if (p.PacketType == PacketType.CarTelemetry)
            {
                LastReceivedPackets.Telemetry = (TelemetryPacket)p;
            }


            #endregion
        }

    }
}