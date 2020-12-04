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
        public string PhotoBlobId { get; set; }

        public ApexVisualUserAccount()
        {
            AccountCreatedAt = DateTimeOffset.Now;
        }

        public static bool UsernameValid(string username)
        {
            //Current rules:
            //Length must be > 1
            //Length cannot be over 15 characters
            //Characters allowed: ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_

            //Does it have at least one character?
            if (username.Length < 1)
            {
                return false;
            }

            //Is it too long?
            if (username.Length > 15)
            {
                return false;
            }

            //Does it contain the right letters?
            string allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
            string stripped = "";
            foreach (char c in username)
            {
                if (allowed.Contains(c.ToString()))
                {
                    stripped = stripped + c.ToString();
                }
            }
            if (username != stripped)
            {
                return false;
            }

            return true;
        }
    
        public static bool PasswordValid(string password)
        {
            //Rules: 
            //Password length must be <= 30
            //Password length must be >= 1
            //Cannot contains equal sign

            //Length
            if (password.Length < 1)
            {
                return false;
            }

            //Length
            if (password.Length > 30)
            {
                return false;
            }

            //Equals sign
            if (password.Contains("="))
            {
                return false;
            }

            return true;
        }
    }
}