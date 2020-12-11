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
            await cbc.GetContainerReference("userphotos").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("activitylogs").CreateIfNotExistsAsync();
            await cbc.GetContainerReference("messagesubmissions").CreateIfNotExistsAsync();
        }
        #endregion

        #region "Basic session data uploading/downloading"

        public static async Task UploadSessionDataAsync(this ApexVisualManager avm, List<byte[]> session_data)
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

        public static async Task<List<byte[]>> DownloadSessionDataAsync(this ApexVisualManager avm, string sessionID)
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


        #endregion

        #region "Downloading/Uploading profile picture image"

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

        #region "Upload/Download Message Submission bodies"

        public static async Task<Guid> UploadMessageSubmissionBodyAsync(this ApexVisualManager avm, string body, Guid? as_id = null)
        {
            //If an ID is supplied, use that. If it is not supplied, use a random one.
            Guid g = Guid.NewGuid();
            if (as_id != null)
            {
                g = as_id.Value;
            }

            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("messagesubmissions");
            await cont.CreateIfNotExistsAsync();

            //Upload it
            CloudBlockBlob blb = cont.GetBlockBlobReference(g.ToString());
            await blb.UploadTextAsync(body);

            return g;
        }

        public static async Task<string> DownloadMessageSubmissionBodyAsync(this ApexVisualManager avm, Guid id)
        {
            CloudBlobClient cbc = GetCloudBlobClient(avm.AzureStorageConnectionString);
            CloudBlobContainer cont = cbc.GetContainerReference("messagesubmissions");
            CloudBlockBlob blb = cont.GetBlockBlobReference(id.ToString());

            if (blb.Exists() == false)
            {
                throw new Exception("Message submission body with ID '" + id.ToString() + "' does not exist in blob storage.");
            }

            string content = await blb.DownloadTextAsync();
            return content;
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