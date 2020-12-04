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
        private static string UsernameAllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
        private static int UsernameMaxLength = 15;
        private static int UsernameMinLength = 1;
        private static int PasswordMaxLength = 30;
        private static int PasswordMinLength = 1;


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

            //Does it have at least one character?
            if (username.Length < UsernameMinLength)
            {
                return false;
            }

            //Is it too long?
            if (username.Length > UsernameMaxLength)
            {
                return false;
            }

            //Does it contain the right letters?
            string stripped = "";
            foreach (char c in username)
            {
                if (UsernameAllowedCharacters.Contains(c.ToString()))
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
            //Unique Rules: 
            //Cannot contains equal sign

            //Length
            if (password.Length < PasswordMinLength)
            {
                return false;
            }

            //Length
            if (password.Length > PasswordMaxLength)
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
    
        public static string ModifyUsernameToValid(string username)
        {
            //Strip it down to the allowed characters
            string ToReturn = "";
            foreach (char c in username)
            {
                if (UsernameAllowedCharacters.Contains(c.ToString()))
                {
                    ToReturn = ToReturn + c.ToString();
                }
            }

            //If the stripped is too small, throw an error
            if (ToReturn.Length < UsernameMinLength)
            {
                throw new Exception("There are not enough valid characters in username '" + username + "' to convert it to a valid username.");
            }

            //If it is too long, shorten it
            if (ToReturn.Length > UsernameMaxLength)
            {
                ToReturn = ToReturn.Substring(0, UsernameMaxLength);
            }

            return ToReturn;
        }
    }
}