using System;
using ApexVisual.F1_2020;
using ApexVisual.F1_2020.Analysis;

namespace ApexVisual.F1_2020
{
    public class ApexVisualToolkit
    {
        public static float DistanceBetweenTwoPoints(TrackLocation loc1, TrackLocation loc2)
        {
            float x_2 = (float)Math.Pow(loc2.PositionX - loc1.PositionX, 2);
            float y_2 = (float)Math.Pow(loc2.PositionY - loc1.PositionY, 2);
            float z_2 = (float)Math.Pow(loc2.PositionZ - loc1.PositionZ, 2);
            float dist = (float)Math.Sqrt(x_2 + y_2 + z_2);
            return dist;
        }

        public static string CleanseString(string original, string allowed_characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_ ")
        {
            string ToReturn = "";
            foreach (char c in original)
            {
                int num = Convert.ToInt32(c);
                if (num == 160) //160 is a non breaking space.
                {
                    ToReturn = ToReturn + " ";
                }
                else
                {
                    if (allowed_characters.Contains(c.ToString()))
                    {
                        ToReturn = ToReturn + c.ToString();
                    }
                } 
            }
            return ToReturn;
        }
    }
    
}