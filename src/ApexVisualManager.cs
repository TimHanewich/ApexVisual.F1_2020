using System;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Threading.Tasks;
using System.Collections.Generic;
using Codemasters.F1_2020;
using ApexVisual.F1_2020.Analysis;
using Newtonsoft.Json;
using System.IO;
using ApexVisual.F1_2020.ActivityLogging;

namespace ApexVisual.F1_2020
{
    public class ApexVisualManager
    {
        public string con_str = "";
        private CloudStorageAccount csa;
        private CloudBlobClient cbc;

        public static ApexVisualManager Create(string connection_string)
        {
            ApexVisualManager ToReturn = new ApexVisualManager();

            //Save con string
            ToReturn.con_str = connection_string;

            //Make the connection
            bool succeededconnection = CloudStorageAccount.TryParse(connection_string, out ToReturn.csa);
            if (succeededconnection == false)
            {
                throw new Exception("Connection to Azure blob storage using the provided connection string failed.");
            }

            //Make CBC
            ToReturn.cbc = ToReturn.csa.CreateCloudBlobClient();
            
            return ToReturn;
        }
    }
}