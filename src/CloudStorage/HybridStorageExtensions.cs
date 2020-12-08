using System;
using ApexVisual.F1_2020;
using ApexVisual.F1_2020.CloudStorage;
using System.Threading.Tasks;

namespace ApexVisual.F1_2020.CloudStorage
{
    public static class HybridStorageExtensions
    {
        public static async Task<Guid> CascadeUploadMessageSubmissionAsync(this ApexVisualManager avm, MessageSubmission msg)
        {
            Guid ToUseAndReturn = Guid.NewGuid();

            //Upload the Message Submission itself to sql
            Guid id_SQL = await avm.UploadMessageSubmissionAsync(msg, ToUseAndReturn);

            //Upload the body
            Guid id_BLOB = await avm.UploadMessageSubmissionBodyAsync(msg.Body, ToUseAndReturn);

            if (id_SQL != id_BLOB)
            {
                throw new Exception("The ID of the MessageSubmission in SQL is not identical to the BLOB that contains the body.");
            }
            
            return id_SQL;
        }
    }
}