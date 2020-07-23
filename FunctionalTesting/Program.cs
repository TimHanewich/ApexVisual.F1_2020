using System;
using ApexVisual.F1_2020;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FunctionalTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            ApexVisualManager avm = ApexVisualManager.Create("<Connection string here>");
            
            string path = "C:\\Users\\TaHan\\Downloads\\Telemetry 7-14-2020 cacc16ea-4bb2-47d6-a386-ead3bf20c54f.json";

            string content = System.IO.File.ReadAllText(path);

            List<byte[]> data = JsonConvert.DeserializeObject<List<byte[]>>(content);

            Console.WriteLine("Uploading now...");
            avm.UploadSessionAsync(data).Wait();

            Console.WriteLine("Done!");

        }
    }
}
