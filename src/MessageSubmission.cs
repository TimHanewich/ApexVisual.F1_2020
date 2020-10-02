using System;

namespace ApexVisual.F1_2020
{
    public class MessageSubmission
    {
        public string Email {get; set;}
        public string Body {get; set;}
        public MessageType MessageType {get; set;}
        public DateTimeOffset CreatedAt {get; set;}

        public MessageSubmission()
        {
            CreatedAt = DateTimeOffset.Now;
        }
    }
}