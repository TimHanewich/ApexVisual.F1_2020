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

            string[] analysesssss = avm.LisSessionAnalysisNamesAsync().Result;

            foreach (string s in analysesssss)
            {
                Console.WriteLine(s);
            }
            Console.ReadLine();
            
            string path = "C:\\Users\\TaHan\\Downloads\\Alfa Romeo Hungary 25 percent race.json";

            string content = System.IO.File.ReadAllText(path);

            List<byte[]> data = JsonConvert.DeserializeObject<List<byte[]>>(content);
            Packet[] packets = CodemastersToolkit.BulkConvertByteArraysToPackets(data);

            SessionAnalysis sa = new SessionAnalysis();
            sa.Load(packets, packets[0].PlayerCarIndex);
            while (sa.LoadComplete == false)
            {
                Console.WriteLine(sa.PercentLoadComplete.ToString("#0.0%"));
                Task.Delay(1).Wait();
            }

            Console.WriteLine("Uploading...");
            avm.UploadSessionAnalysisAsync(sa).Wait();

            Console.WriteLine("Done!");

        }
    }
}
