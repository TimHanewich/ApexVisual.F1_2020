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
    

        #region "Setup Methods"
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
        #endregion

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
    
        public async Task<string[]> ListSessionAnalysisNamesAsync()
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

            //Serialize to a Stream
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            JsonTextWriter jtw = new JsonTextWriter(sw);
            JsonSerializer js = new JsonSerializer();
            js.Serialize(jtw, session_data);
            jtw.Flush();
            await ms.FlushAsync();
            ms.Position = 0;
            

            //Upload
            CloudBlockBlob blb = cont.GetBlockBlobReference(file_name);
            await blb.UploadFromStreamAsync(ms);
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

        #region "User Account Operations"

        public async Task<ApexVisualUserAccount> DownloadUserAccountAsync(string username)
        {
            CloudBlobContainer cont = cbc.GetContainerReference("useraccounts");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference(username);
            if (blb.Exists() == false)
            {
                throw new Exception("User Account with username '" + username +"' does not exist.");
            }
            string down = await blb.DownloadTextAsync();
            ApexVisualUserAccount acc = JsonConvert.DeserializeObject<ApexVisualUserAccount>(down);
            return acc;
        }

        public async Task UploadUserAccountAsync(ApexVisualUserAccount account)
        {
            List<string> FlagChars = new List<string>();
            FlagChars.Add(" ");
            FlagChars.Add("*");
            FlagChars.Add("-");
            FlagChars.Add("#");
            FlagChars.Add("@");
            FlagChars.Add("!");
            FlagChars.Add(".");
            FlagChars.Add("%");
            FlagChars.Add("^");
            FlagChars.Add("&");
            FlagChars.Add("(");
            FlagChars.Add(")");
            //Make sure the account username is acceptable
            foreach (string s in FlagChars)
            {
                if (account.Username.Contains(s))
                {
                    throw new Exception("Your username cannot contain the character '" + s + "'.");
                }
            }

            CloudBlobContainer cont = cbc.GetContainerReference("useraccounts");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference(account.Username);
            string json = JsonConvert.SerializeObject(account);
            await blb.UploadTextAsync(json);
        }
        
        #endregion

        #region "Basic downloading"

        public async Task<List<byte[]>> DownloadSessionAsync(string sessionID)
        {
            CloudBlobContainer cont = cbc.GetContainerReference("sessions");
            await cont.CreateIfNotExistsAsync();

            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            if (blb.Exists() == false)
            {
                throw new Exception("Unable to find session with title '" + sessionID + "'.");
            }

            //Download the Stream
            MemoryStream ms = new MemoryStream();
            await blb.DownloadToStreamAsync(ms);
            ms.Position = 0;

            //Desrialize
            StreamReader sr = new StreamReader(ms);
            JsonTextReader jtr = new JsonTextReader(sr);
            JsonSerializer js = new JsonSerializer();
            List<byte[]> data_to_return;
            try
            {
                data_to_return = js.Deserialize<List<byte[]>>(jtr);
            }
            catch (Exception e)
            {
                throw new Exception("Failure while deserializing content for session '" + sessionID.ToString() + "'. Message: " + e.Message);
            }

            return data_to_return;
        }

        public async Task<SessionSummary> DownloadSessionSummaryAsync(string sessionID)
        {
            CloudBlobContainer cont = cbc.GetContainerReference("sessionsummaries");
            await cont.CreateIfNotExistsAsync();

            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            if (blb.Exists() == false)
            {
                throw new Exception("Unable to find session summary with title '" + sessionID + "'.");
            }

            string content = await blb.DownloadTextAsync();
            SessionSummary data_to_return;
            try
            {
                data_to_return = JsonConvert.DeserializeObject<SessionSummary>(content);
            }
            catch
            {
                throw new Exception("Failure while deserializing content for session summary '" + sessionID.ToString() + "'.");
            }

            return data_to_return;
        }

        public async Task<SessionAnalysis> DownloadSessionAnalysisAsync(string sessionID)
        {
            CloudBlobContainer cont = cbc.GetContainerReference("sessionanalyses");
            await cont.CreateIfNotExistsAsync();

            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            if (blb.Exists() == false)
            {
                throw new Exception("Unable to find session analysis with title '" + sessionID + "'.");
            }

            string content = await blb.DownloadTextAsync();
            SessionAnalysis data_to_return;
            try
            {
                data_to_return = JsonConvert.DeserializeObject<SessionAnalysis>(content);
            }
            catch
            {
                throw new Exception("Failure while deserializing content for session analysis '" + sessionID.ToString() + "'.");
            }

            return data_to_return;
        }

        

        #endregion

        #region "Basic Existance checks"

        public async Task<bool> SessionExistsAsync(string sessionID)
        {
            CloudBlobContainer cont = cbc.GetContainerReference("sessions");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            return await blb.ExistsAsync();
        }

        public async Task<bool> SessionSummaryExistsAsync(string sessionID)
        {
            CloudBlobContainer cont = cbc.GetContainerReference("sessionsummaries");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            return await blb.ExistsAsync();
        }

        public async Task<bool> SessionAnalysisExistsAsync(string sessionID)
        {
            CloudBlobContainer cont = cbc.GetContainerReference("sessionanalyses");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            return await blb.ExistsAsync();
        }

        #endregion


        #region "Utility Functions"

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
    
        #endregion
    }
}