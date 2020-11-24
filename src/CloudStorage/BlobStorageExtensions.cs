using System;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using ApexVisual.F1_2020;
using Codemasters.F1_2020;
using ApexVisual.F1_2020.Analysis;
using ApexVisual.F1_2020.ActivityLogging;

namespace ApexVisual.F1_2020.CloudStorage
{
    public static class BlobStorageExtensions
    {
        #region "Setup Methods"
        /// <summary>
        /// Sets up all necessary blob containers (if they do not exist)
        /// </summary>
        public static async Task InitializeBlobContainersAsync(this ApexVisualManager avm)
        {  
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            await cbc.GetContainerReference("sessions").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("sessionsummaries").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("sessionanalyses").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("useraccounts").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("userphotos").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("activitylogs").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("messagesubmissions").CreateIfNotExistsAsync();
        }
        #endregion

        #region "Listing session-related data"    
        public static async Task<string[]> ListSessionNamesAsync(this ApexVisualManager avm)
        {
            string[] tr = await GetBlobNamesInContainerAsync(avm, "sessions");
            return tr;
        }
    
        public static async Task<string[]> ListSessionSummaryNamesAsync(this ApexVisualManager avm)
        {
            string[] tr = await GetBlobNamesInContainerAsync(avm, "sessionsummaries");
            return tr;
        }
    
        public static async Task<string[]> ListSessionAnalysisNamesAsync(this ApexVisualManager avm)
        {
            string[] tr = await GetBlobNamesInContainerAsync(avm, "sessionanalyses");
            return tr;
        }
        #endregion

        #region "Basic session uploading"

        public static async Task UploadSessionAsync(this ApexVisualManager avm, List<byte[]> session_data)
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
            
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
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

        public static async Task UploadSessionSummaryAsync(this ApexVisualManager avm, SessionSummary summary)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("sessionsummaries");
            await cont.CreateIfNotExistsAsync();

            CloudBlockBlob blb = cont.GetBlockBlobReference(summary.SessionId.ToString());
            string json = JsonConvert.SerializeObject(summary);
            await blb.UploadTextAsync(json);
        }

        public static async Task UploadSessionAnalysisAsync(this ApexVisualManager avm, Session analysis)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("sessionanalyses");
            await cont.CreateIfNotExistsAsync();

            CloudBlockBlob blb = cont.GetBlockBlobReference(analysis.SessionId.ToString());
            string json = JsonConvert.SerializeObject(analysis);
            await blb.UploadTextAsync(json);
        }

        #endregion

        #region "Basic session downloading"

        public static async Task<List<byte[]>> DownloadSessionAsync(this ApexVisualManager avm, string sessionID)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
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

        public static async Task<SessionSummary> DownloadSessionSummaryAsync(this ApexVisualManager avm, string sessionID)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
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

        public static async Task<Session> DownloadSessionAnalysisAsync(this ApexVisualManager avm, string sessionID)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("sessionanalyses");
            await cont.CreateIfNotExistsAsync();

            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            if (blb.Exists() == false)
            {
                throw new Exception("Unable to find session analysis with title '" + sessionID + "'.");
            }

            string content = await blb.DownloadTextAsync();
            Session data_to_return;
            try
            {
                data_to_return = JsonConvert.DeserializeObject<Session>(content);
            }
            catch
            {
                throw new Exception("Failure while deserializing content for session analysis '" + sessionID.ToString() + "'.");
            }

            return data_to_return;
        }

        

        #endregion

        #region "Basic session Existance checks"

        public static async Task<bool> SessionExistsAsync(this ApexVisualManager avm, string sessionID)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("sessions");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            return await blb.ExistsAsync();
        }

        public static async Task<bool> SessionSummaryExistsAsync(this ApexVisualManager avm, string sessionID)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("sessionsummaries");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            return await blb.ExistsAsync();
        }

        public static async Task<bool> SessionAnalysisExistsAsync(this ApexVisualManager avm, string sessionID)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("sessionanalyses");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference(sessionID);
            return await blb.ExistsAsync();
        }

        #endregion

        #region "Activity logging"

        public static async Task UploadActivityLogAsync(this ApexVisualManager avm, ActivityLog log)
        {
            //Get the container
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("activitylogs");
            await cont.CreateIfNotExistsAsync();

            //Get the append blob
            string blob_name = DateTime.UtcNow.Year.ToString() + "." + DateTime.UtcNow.Month.ToString() + "." + DateTime.UtcNow.Day.ToString();
            CloudAppendBlob blob = cont.GetAppendBlobReference(blob_name);
            
            //If the blob doesn't exist, start it off with something.
            if (blob.Exists() == false)
            {
                blob.UploadText("");
            }

            //Prepare the content to append
            string as_json = JsonConvert.SerializeObject(log);
            string to_append = as_json + "<:::SPLIT:::>";
            
            //Append it
            await blob.AppendTextAsync(to_append);
        }

        #endregion

        #region "Downloading profile picture image"

        public static async Task<Stream> DownloadProfilePictureAsync(this ApexVisualManager avm, string id)
        {
            //Get the container
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("userphotos");
            await cont.CreateIfNotExistsAsync();

            //Get the profile picture. Throw an error if it does not exist
            CloudBlockBlob blb = cont.GetBlockBlobReference(id);
            if (blb.Exists() == false)
            {
                throw new Exception("User photo with ID '" + id + "' does not exist.");
            }

            //Download the blob contents
            MemoryStream ms = new MemoryStream();
            await blb.DownloadToStreamAsync(ms);
            ms.Position = 0;
            
            return ms;
        }

        #endregion

        #region "Uploading profile picture image"

        /// <summary>
        /// This uploads the image to Azure and then provides you with the unique ID that the image is called. You can then use this ID by plugging into the Apex Visual User account as the image ID. 
        /// </summary>
        public static async Task<string> UploadProfilePictureAsync(this ApexVisualManager avm, Stream image_stream)
        {
            //Get the container
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("userphotos");
            await cont.CreateIfNotExistsAsync();

            //Get a name for it and upload it
            string ToReturnId = Guid.NewGuid().ToString();
            CloudBlockBlob blb = cont.GetBlockBlobReference(ToReturnId);
            await blb.UploadFromStreamAsync(image_stream);
            
            return ToReturnId;
        }

        #endregion

        #region "Uploading/Downloading message submissions"

        public static async Task UploadMessageSubmissionAsync(this ApexVisualManager avm, MessageSubmission message)
        {
            //Get the container
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("messagesubmissions");
            await cont.CreateIfNotExistsAsync();

            //Get the name that we are going to use and the string we are going to append to.
            string name = message.CreatedAt.Year.ToString("0000") + "." + message.CreatedAt.Month.ToString("00") + "." + message.CreatedAt.Day.ToString("00");
            string to_append = JsonConvert.SerializeObject(message) + "<:::SPLIT:::>";


            //Upload it
            CloudAppendBlob cab = cont.GetAppendBlobReference(name);
            if (cab.Exists())
            {
                await cab.AppendTextAsync(to_append);
            }
            else
            {
                await cab.UploadTextAsync(to_append);
            }
        }

        public static async Task<MessageSubmission[]> DownloadMessageSubmissionsAsync(this ApexVisualManager avm, DateTime day)
        {
            //Get the container
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("messagesubmissions");
            await cont.CreateIfNotExistsAsync();

            //Get the blob name we are searching for
            List<MessageSubmission> ToReturn = new List<MessageSubmission>();
            string name = day.Year.ToString("0000") + "." + day.Month.ToString("00") + "." + day.Day.ToString("00");
            CloudAppendBlob cab = cont.GetAppendBlobReference(name);

            //If it doesn't exist, return an empty array.
            bool exists = await cab.ExistsAsync();
            if (exists == false)
            {
                ToReturn.Clear();
                return ToReturn.ToArray(); //Return an empty array.
            }
            
            //Download the contents and split them
            string content = await cab.DownloadTextAsync();
            List<string> Splitter = new List<string>();
            Splitter.Add("<:::SPLIT:::>");
            string[] parts = content.Split(Splitter.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in parts)
            {
                try
                {
                    MessageSubmission ms = JsonConvert.DeserializeObject<MessageSubmission>(s);
                    ToReturn.Add(ms);
                }
                catch
                {

                }
            }
            
            //Return the list of them now
            return ToReturn.ToArray();
        }

        #endregion

        #region "Utility Functions"

        public static CloudBlobClient GetCloudBlobClient(string connection_string)
        {
            CloudStorageAccount csa;
            CloudStorageAccount.TryParse(connection_string, out csa);
            return csa.CreateCloudBlobClient();
        }

        // UTLITY FUNCTIONS BELOW
        private static async Task<string[]> GetBlobNamesInContainerAsync(this ApexVisualManager avm, string container_name)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);

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