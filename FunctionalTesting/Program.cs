using System;
using ApexVisual.F1_2020;
using System.Collections.Generic;
using Newtonsoft.Json;
using Codemasters.F1_2020;
using Codemasters.F1_2020.Analysis;
using System.Threading.Tasks;

namespace FunctionalTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            string constr = System.IO.File.ReadAllText("C:\\Users\\tihanewi\\Downloads\\az_con_str.txt");
            ApexVisualManager avm = ApexVisualManager.Create(constr);

            

            Console.WriteLine("Done");

        }
    }
}
