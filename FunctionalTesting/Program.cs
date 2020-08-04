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
            
            DownloadFromCloud();
            
        }

        static void UploadSession()
        {
            string constr = System.IO.File.ReadAllText("C:\\Users\\tihanewi\\Downloads\\az.txt");
            ApexVisualManager avm = ApexVisualManager.Create(constr);

            string path = "C:\\Users\\tihanewi\\Downloads\\Telemetry 7-14-2020 8360128d-45ca-4242-84db-36a1d43d027a.json";
            string content = System.IO.File.ReadAllText(path);

            List<byte[]> data = JsonConvert.DeserializeObject<List<byte[]>>(content);
            Console.WriteLine(data.Count.ToString());

            avm.UploadSessionAsync(data).Wait();

                        
        
        
        
        }
    
        static void OpenFromLocal()
        {
            string path = "C:\\Users\\tihanewi\\Downloads\\11381929133624196240";
            string content = System.IO.File.ReadAllText(path);
            List<byte[]> data = JsonConvert.DeserializeObject<List<byte[]>>(content);
            Console.WriteLine(data.Count.ToString());
        }
    
        static void DownloadFromCloud()
        {
            string constr = System.IO.File.ReadAllText("C:\\Users\\tihanewi\\Downloads\\az.txt");
            ApexVisualManager avm = ApexVisualManager.Create(constr);
            List<byte[]> data = avm.DownloadSessionAsync("11381929133624196240").Result;
            Console.WriteLine(data.Count.ToString());
        }
    
    }
}
