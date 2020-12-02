using System;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApexVisual.F1_2020.CloudStorage
{
    public class ApexVisualUserAccountTbl : TableEntity
    {
        //Partition key: always "user"
        //Row key: Username

        public string Password { get; set; }
        public string Email { get; set; }
        public DateTimeOffset AccountCreatedAt { get; set; }
        public string OwnedSessionIds { get; set; }
        public string PhotoBlobId { get; set; }

        public ApexVisualUserAccountTbl()
        {

        }

        public ApexVisualUserAccountTbl(string username)
        {
            PartitionKey = "user";
            RowKey = username;
        }

        public static ApexVisualUserAccountTbl FromApexVisualUserAccount(ApexVisualUserAccount account)
        {
            ApexVisualUserAccountTbl to_return = new ApexVisualUserAccountTbl(account.Username);
            to_return.Password = account.Password;
            to_return.Email = account.Email;
            to_return.AccountCreatedAt = account.AccountCreatedAt;
            to_return.PhotoBlobId = account.PhotoBlobId;
            return to_return;
        } 
    
        public ApexVisualUserAccount ToApexVisualUserAccount()
        {
            ApexVisualUserAccount ToReturn = new ApexVisualUserAccount();

            ToReturn.Username = RowKey;
            ToReturn.Password = Password;
            ToReturn.Email = Email;
            ToReturn.AccountCreatedAt = AccountCreatedAt;
            ToReturn.PhotoBlobId = PhotoBlobId;

            return ToReturn;
        }
    }
}