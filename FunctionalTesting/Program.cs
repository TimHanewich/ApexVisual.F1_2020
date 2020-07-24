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
            string constr = System.IO.File.ReadAllText("C:\\Users\\TaHan\\Downloads\\av_test_az_constr.txt");
            ApexVisualManager avm = ApexVisualManager.Create(constr);

            ApexVisualUserAccount acc = avm.DownloadUserAccountAsync("TimHanefwich").Result;
            Console.WriteLine(acc.Email);

            Console.WriteLine("Done");

        }
    }
}
