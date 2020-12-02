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
        public string AzureStorageConnectionString {get;}
        public string AzureSqlDbConnectionString {get;}

        public ApexVisualManager(string azure_storage_connection_string, string azure_sql_db_connection_string)
        {
            AzureStorageConnectionString = azure_storage_connection_string;
            AzureSqlDbConnectionString = azure_sql_db_connection_string;
        }
    }
}