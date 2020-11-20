using System;
using Microsoft.Azure.Cosmos.Table;
using ApexVisual.F1_2020;
using System.Threading.Tasks;

namespace ApexVisual.F1_2020.CloudStorage
{
    public static class TableStorageExtensions
    {

        #region "Initialization"

        public static async Task InitializeTablesAsync(this ApexVisualManager avm)
        {
            //Log in
            CloudTableClient ctc = GetCloudTableClient(avm.con_str);

            //useraccounts
            CloudTable useraccounts = ctc.GetTableReference("useraccounts");
            await useraccounts.CreateIfNotExistsAsync();
        }

        #endregion

        #region "User account transactions"
        /// <summary>
        /// Uses cloud table storage to fulfill request.
        /// </summary>
        public static async Task<ApexVisualUserAccount> DownloadUserAccountAsync(this ApexVisualManager manager, string username)
        {
            //Get cloud table client
            CloudTableClient ctc = GetCloudTableClient(manager.con_str);

            //Get the user table
            CloudTable ct = ctc.GetTableReference("useraccounts");
            await ct.CreateIfNotExistsAsync();

            //Retrieve the user
            TableOperation to = TableOperation.Retrieve<ApexVisualUserAccountTbl>("user", username);
            TableResult tr = await ct.ExecuteAsync(to);
            if (tr.HttpStatusCode != 200)
            {
                throw new Exception("Unable to download user account '" + username + "'.");
            }
            ApexVisualUserAccountTbl avuatbl = (ApexVisualUserAccountTbl)tr.Result;

            //Conver it
            ApexVisualUserAccount ToReturn = avuatbl.ToApexVisualUserAccount();

            //Return it
            return ToReturn;
        }

        /// <summary>
        /// Uses cloud table storage to fulfill request.
        /// </summary>
        public static async Task UploadUserAccountAsync(this ApexVisualManager manager, ApexVisualUserAccount account)
        {
            //Get cloud table client
            CloudTableClient ctc = GetCloudTableClient(manager.con_str);

            //Get the user table
            CloudTable ct = ctc.GetTableReference("useraccounts");
            await ct.CreateIfNotExistsAsync();

            //Convert it to a table entity
            ApexVisualUserAccountTbl tble = ApexVisualUserAccountTbl.FromApexVisualUserAccount(account);

            //Prepare and execute the action
            TableOperation to = TableOperation.InsertOrReplace(tble);
            TableResult tr = await ct.ExecuteAsync(to);
            if (tr.HttpStatusCode != 204)
            {
                throw new Exception("Unable to upload user account '" + account.Username + "'.");
            }
        }

        #endregion

        #region "Utility functions"
        private static CloudTableClient GetCloudTableClient(string connection_string)
        {
            CloudStorageAccount csa;
            CloudStorageAccount.TryParse(connection_string, out csa);
            return csa.CreateCloudTableClient();
        }
        #endregion
    }
}