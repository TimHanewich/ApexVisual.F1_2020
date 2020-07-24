using System;
using System.Collections.Generic;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApexVisual.F1_2020
{
    public class ApexVisualUserAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTimeOffset AccountCreatedAt { get; set; }
        public List<string> OwnedSessionIds { get; set; }
        public string PhotoBlobId { get; set; }

        public ApexVisualUserAccount()
        {
            OwnedSessionIds = new List<string>();
            AccountCreatedAt = DateTimeOffset.Now;
        }
    }
}