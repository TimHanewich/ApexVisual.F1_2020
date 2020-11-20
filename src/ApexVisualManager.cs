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
            await cbc.GetContainerReference("activitylogs").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("messagesubmissions").CreateIfNotExistsAsync();
        }
        #endregion

        #region "Listing session-related data"    
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

        #region "Basic session uploading"

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

        #region "Basic session downloading"

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

        #region "Basic session Existance checks"

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

        #region "Activity logging"

        public async Task UploadActivityLogAsync(ActivityLog log)
        {
            //Get the container
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

        public async Task<Stream> DownloadProfilePictureAsync(string id)
        {
            //Get the container
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
        public async Task<string> UploadProfilePictureAsync(Stream image_stream)
        {
            //Get the container
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

        public async Task UploadMessageSubmissionAsync(MessageSubmission message)
        {
            //Get the container
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

        public async Task<MessageSubmission[]> DownloadMessageSubmissionsAsync(DateTime day)
        {
            //Get the container
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