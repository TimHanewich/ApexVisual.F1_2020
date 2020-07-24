using System;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Threading.Tasks;
using System.Collections.Generic;
using Codemasters.F1_2020;
using Codemasters.F1_2020.Analysis;
using Newtonsoft.Json;
using System.IO;

namespace ApexVisual.F1_2020
{
    public class ApexVisualManager
    {
        private string con_str = "";
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
    
        /// <summary>
        /// Sets up all necessary blob containers (if they do not exist)
        /// </summary>
        public async Task InitializeAsync()
        {   
            await cbc.GetContainerReference("sessions").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("sessionsummaries").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("sessionanalyses").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("useraccounts").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("userphotos").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("wallpapers").CreateIfNotExistsAsync();
        }
    

        #region "Listing data"    
        public async Task<string[]> ListSessionNamesAsync()
        {
            string[] tr = await GetBlobNamesInContainerAsync("sessions");
            return tr;
        }
    
        public async Task<string[]> ListSessionSummaryNamesAsync()
        {
            string[] tr = await GetBlobNamesInContainerAsync("sessionsummaries");
            return tr;
        }
    
        public async Task<string[]> LisSessionAnalysisNamesAsync()
        {
            string[] tr = await GetBlobNamesInContainerAsync("sessionanalyses");
            return tr;
        }
        #endregion

        #region "Basic uploading"

        public async Task UploadSessionAsync(List<byte[]> session_data)
        {
            //Get unique session
            string file_name = "";
            try
            {
                Packet p = new Packet();
                p.LoadBytes(session_data[0]);
                file_name = p.UniqueSessionId.ToString();
            }
            catch
            {
                throw new Exception("Fatal error while getting unique session ID.");
            }
            
            
            CloudBlobContainer cont = cbc.GetContainerReference("sessions");
            await cont.CreateIfNotExistsAsync();

            //Upload
            CloudBlockBlob blb = cont.GetBlockBlobReference(file_name);
            await blb.UploadTextAsync(JsonConvert.SerializeObject(session_data));
        }

        public async Task UploadSessionSummaryAsync(SessionSummary summary)
        {
            CloudBlobContainer cont = cbc.GetContainerReference("sessionsummaries");
            await cont.CreateIfNotExistsAsync();

            CloudBlockBlob blb = cont.GetBlockBlobReference(summary.SessionId.ToString());
            string json = JsonConvert.SerializeObject(summary);
            await blb.UploadTextAsync(json);
        }

        public async Task UploadSessionAnalysisAsync(SessionAnalysis analysis)
        {
            CloudBlobContainer cont = cbc.GetContainerReference("sessionanalyses");
            await cont.CreateIfNotExistsAsync();

            CloudBlockBlob blb = cont.GetBlockBlobReference(analysis.SessionId.ToString());
            string json = JsonConvert.SerializeObject(analysis);
            await blb.UploadTextAsync(json);
        }

        #endregion

        

        // UTLITY FUNCTIONS BELOW
        private async Task<string[]> GetBlobNamesInContainerAsync(string container_name)
        {
            List<string> ToReturn = new List<string>();

            CloudBlobContainer cont = cbc.GetContainerReference(container_name);
            await cont.CreateIfNotExistsAsync();

            IEnumerable<IListBlobItem> blobs = cont.ListBlobs();
            foreach (IListBlobItem bi in blobs)
            {
                CloudBlockBlob blb = (CloudBlockBlob)bi;
                ToReturn.Add(blb.Name);
            }

            return ToReturn.ToArray();
        }
    }
}