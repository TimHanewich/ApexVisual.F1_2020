using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApexVisual.F1_2020.Analysis;
using Codemasters.F1_2020;
using ApexVisual.F1_2020.ActivityLogging;

namespace ApexVisual.F1_2020.CloudStorage
{
    public static class SqlExtensions
    {

        #region "Initialization"

        public static void InitializeSqlTables(this ApexVisualManager avm)
        {
            SqlConnection connection = GetSqlConnection(avm);
            connection.Open();

            //Get a list of all the tables that already exist in the DB - we wouldn't want to try and make one that already exists
            SqlCommand get_all_tables_cmd = new SqlCommand("select TABLE_NAME from INFORMATION_SCHEMA.TABLES", connection);
            SqlDataReader dr = get_all_tables_cmd.ExecuteReader();
            List<string> TablesThatAlreadyExist = new List<string>();
            while (dr.Read())
            {
                TablesThatAlreadyExist.Add(dr.GetString(0));
            }
            dr.Close();


            List<string> TableCreationCommands = new List<string>();

            #region "Tracks"

            #region "Create track tables"

            if (TablesThatAlreadyExist.Contains("Track") == false)
            {
                //TableCreationCommands.Add("create table Track (Id tinyint not null primary key, CountryCode varchar(2), Latitude real, Longitude real)");
            }

            #endregion

            #endregion

            #region "Sessions"
   
            //Session
            if (TablesThatAlreadyExist.Contains("Session") == false)
            {
                TableCreationCommands.Add("create table Session (SessionId varchar(30) not null primary key, Game smallint, Owner varchar(255), Circuit tinyint, SessionMode tinyint, SelectedTeam tinyint, SelectedDriver tinyint, DriverName varchar(255), SessionCreatedAt datetime)");
            }

            //Lap
            if (TablesThatAlreadyExist.Contains("Lap") == false)
            {
                TableCreationCommands.Add("create table Lap (Id uniqueidentifier not null primary key, SessionId varchar(30), LapNumber tinyint, Sector1Time real, Sector2Time real, Sector3Time real, FuelConsumed real, PercentOnThrottle real, PercentOnBrake real, PercentCoasting real, PercentThrottleBrakeOverlap real, PercentOnMaxThrottle real, PercentOnMaxBrake real, ErsDeployed real, ErsHarvested real, GearChanges int, TopSpeedKph smallint, EquippedTyreCompound tinyint, IncrementalTyreWear uniqueidentifier, BeginningTyreWear uniqueidentifier)");
            }

            //TelemetrySnapshot
            if (TablesThatAlreadyExist.Contains("TelemetrySnapshot") == false)
            {
                TableCreationCommands.Add("create table TelemetrySnapshot (Id uniqueidentifier not null primary key, LapId uniqueidentifier, LocationType tinyint, LocationNumber tinyint, PositionX real, PositionY real, PositionZ real, VelocityX real, VelocityY real, VelocityZ real, gForceLateral real, gForceLongitudinal real, gForceVertical real, Yaw real, Pitch real, Roll real, CurrentLapTime real, CarPosition tinyint, LapInvalid bit, Penalties tinyint, SpeedKph int, Throttle real, Steer real, Brake real, Clutch real, Gear smallint, EngineRpm int, DrsActive bit, BrakeTemperature uniqueidentifier, TyreSurfaceTemperature uniqueidentifier, TyreInnerTemperature uniqueidentifier, EngineTemperature int, SelectedFuelMix tinyint, FuelLevel real, TyreWearPercent uniqueidentifier, TyreDamagePercent uniqueidentifier, FrontLeftWingDamage real, FrontRightWingDamage real, RearWingDamage real, ErsStored real)");
            }
               
            //WheelDataArray
            if (TablesThatAlreadyExist.Contains("WheelDataArray") == false)
            {
                TableCreationCommands.Add("create table WheelDataArray (Id uniqueidentifier not null primary key, RearLeft real, RearRight real, FrontLeft real, FrontRight real)");
            }

            //LocationPerformanceAnalysis
            if (TablesThatAlreadyExist.Contains("LocationPerformanceAnalysis") == false)
            {
                TableCreationCommands.Add("create table LocationPerformanceAnalysis (Id uniqueidentifier not null primary key, SessionId varchar(30), LocationType tinyint, LocationNumber tinyint, AverageSpeedKph real, AverageGear real, AverageDistanceToApex real, InconsistencyRating real)");
            }

            #endregion
            
            #region "Users"

            if (TablesThatAlreadyExist.Contains("UserAccount") == false)
            {
                TableCreationCommands.Add("create table UserAccount (Username varchar(15) not null primary key, Password varchar(30), Email varchar(64), AccountCreatedAt datetime, PhotoBlobId varchar(50))");
            }

            #endregion

            #region "Activity logging"

            if (TablesThatAlreadyExist.Contains("ActivityLog") == false)
            {
                TableCreationCommands.Add("create table ActivityLog (Id uniqueidentifier not null primary key, SessionId uniqueidentifier, Username varchar(15), TimeStamp datetime, ApplicationId tinyint, ActivityId int, PackageVersionMajor smallint, PackageVersionMinor smallint, PackageVersionBuild smallint, PackageVersionRevision smallint, Note varchar(255))");
            }

            #endregion

            #region "Message Submission"

            if (TablesThatAlreadyExist.Contains("MessageSubmission") == false)
            {
                TableCreationCommands.Add("create table MessageSubmission (Id uniqueidentifier not null primary key, Username varchar(15), Email varchar(64), MessageType tinyint, CreatedAt datetime)");
            }

            #endregion

            //Create the tables
            foreach (string cmd in TableCreationCommands)
            {
                SqlCommand sqlcmd_thiscmd = new SqlCommand(cmd, connection);
                sqlcmd_thiscmd.ExecuteNonQuery();
            }
            
            connection.Close();
        }

        #endregion

        #region "Existance Check operations"

        public static async Task<bool> SessionExistsAsync(this ApexVisualManager avm, string session_id)
        {
            SqlConnection sql = GetSqlConnection(avm);
            sql.Open();

            string cmd = "select SessionId from Session where SessionId='" + session_id + "'";
            SqlCommand sqlcmd = new SqlCommand(cmd, sql);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            
            bool exists = dr.Read();

            sql.Close();

            return exists;
        }


        #endregion

        #region "User operations"

        public static async Task<bool> UserAccountExists(this ApexVisualManager avm, string username)
        {
            string cmd = "select Username from UserAccount where Username='" + username + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            bool ToReturn = dr.HasRows;

            sqlcon.Close();

            return ToReturn;
        }

        public static async Task<ApexVisualUserAccount> DownloadUserAccountAsync(this ApexVisualManager avm, string username)
        {
            string cmd = "select * from UserAccount where Username='" + username + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            if (dr.HasRows == false)
            {
                throw new Exception("Unable to find user account record with username '" + username + "'");
            }

            await dr.ReadAsync();

            //Prepare the return assets
            ApexVisualUserAccount ToReturn = new ApexVisualUserAccount();

            //Username
            if (dr.IsDBNull(0) == false)
            {
                ToReturn.Username = dr.GetString(0);
            }

            //Password
            if (dr.IsDBNull(1) == false)
            {
                ToReturn.Password = dr.GetString(1);
            }

            //Email
            if (dr.IsDBNull(2) == false)
            {
                ToReturn.Email = dr.GetString(2); 
            }
            
            //Account created at
            if (dr.IsDBNull(3) == false)
            {
                ToReturn.AccountCreatedAt = dr.GetDateTime(3);
            }

            //Photo blob id
            if (dr.IsDBNull(4) == false)
            {
                ToReturn.PhotoBlobId = dr.GetString(4);
            }
            
            sqlcon.Close();

            return ToReturn;
        }

        public static async Task UploadUserAccountAsync(this ApexVisualManager avm, ApexVisualUserAccount useraccount)
        {
            //Error check
            if (useraccount.Username == null || useraccount.Username == "")
            {
                throw new Exception("Unable to upload user account: Username was null or blank.");
            }
            if (useraccount.Password == null || useraccount.Password == "")
            {
                throw new Exception("Unable to upload user account: password was null or blank.");
            }
            if (useraccount.Email == null || useraccount.Email == "")
            {
                throw new Exception("Unable to upload user account: email was null or blank.");
            }
            
            //Prepare the KVP's for this record insert/update
            List<KeyValuePair<string, string>> ColumnValuePairs = new List<KeyValuePair<string, string>>();
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Username", "'" + useraccount.Username + "'"));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Password", "'" + useraccount.Password + "'"));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Email", "'" + useraccount.Email + "'"));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("AccountCreatedAt", "'" + useraccount.AccountCreatedAt.Year.ToString("0000") + "-" + useraccount.AccountCreatedAt.Month.ToString("00") + "-" + useraccount.AccountCreatedAt.Day.ToString("00") + " " + useraccount.AccountCreatedAt.Hour.ToString("00") + ":" + useraccount.AccountCreatedAt.Minute.ToString("00") + "." + useraccount.AccountCreatedAt.Second.ToString() + "'"));
            if (useraccount.PhotoBlobId != null && useraccount.PhotoBlobId != "")
            {
                ColumnValuePairs.Add(new KeyValuePair<string, string>("PhotoBlobId", "'" + useraccount.PhotoBlobId + "'"));
            }

            //Get the appropriate cmd to send
            string cmd = "";
            bool AlreadyExists = await avm.UserAccountExists(useraccount.Username);
            if (AlreadyExists == false) //It is a new account
            {
                //Prepare the command string
                string Component_Columns = "";
                string Component_Values = "";
                foreach (KeyValuePair<string, string> kvp in ColumnValuePairs)
                {
                    Component_Columns = Component_Columns + kvp.Key + ",";
                    Component_Values = Component_Values + kvp.Value + ",";
                }
                Component_Columns = Component_Columns.Substring(0, Component_Columns.Length-1); //Remove the last comma
                Component_Values = Component_Values.Substring(0, Component_Values.Length - 1);//Remove the last comma
                cmd = "insert into UserAccount (" + Component_Columns + ") values (" + Component_Values + ")"; 
            }
            else
            {
                string setter_portion = "";
                foreach (KeyValuePair<string, string> kvp in ColumnValuePairs)
                {
                    setter_portion = setter_portion + kvp.Key + " = " + kvp.Value + ",";
                }
                setter_portion = setter_portion.Substring(0, setter_portion.Length - 1);
                cmd = "update UserAccount set " + setter_portion + " where " + "Username='" + useraccount.Username + "'";
            }

            //Send the command
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();
        }

        public static async Task<string[]> ListOwnedSessionsAsync(this ApexVisualManager avm, string username)
        {
            string cmd = "select SessionId from Session where Owner='" + username + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            
            List<string> ToReturn = new List<string>();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    ToReturn.Add(dr.GetString(0));
                }
            }

            sqlcon.Close();

            return ToReturn.ToArray();
        }

        public static async Task<ApexVisualUserAccount[]> DownloadNewlyRegisteredUsersAsync(this ApexVisualManager avm, DateTime date)
        {
            string cmd = "select * from UserAccount where " + GetTimeStampDayFilter("AccountCreatedAt", date);
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            List<ApexVisualUserAccount> ToReturn = new List<ApexVisualUserAccount>();

            while (dr.Read())
            {
                ApexVisualUserAccount avua = new ApexVisualUserAccount();

                //Username
                if (dr.IsDBNull(0) == false)
                {
                    avua.Username = dr.GetString(0);
                }

                //Password
                if (dr.IsDBNull(1) == false)
                {
                    avua.Password = dr.GetString(1);
                }

                //Email
                if (dr.IsDBNull(2) == false)
                {
                    avua.Email = dr.GetString(2); 
                }
                
                //Account created at
                if (dr.IsDBNull(3) == false)
                {
                    avua.AccountCreatedAt = dr.GetDateTime(3);
                }

                //Photo blob id
                if (dr.IsDBNull(4) == false)
                {
                    avua.PhotoBlobId = dr.GetString(4);
                }
                
                ToReturn.Add(avua);
            }

            sqlcon.Close();
            return ToReturn.ToArray();
        }

        public static async Task<int> CountNewlyRegisteredUsersAsync(this ApexVisualManager avm, DateTime date)
        {
            string cmd = "select count(Username) from UserAccount where " + GetTimeStampDayFilter("AccountCreatedAt", date);
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            dr.Read();
            int ToReturn = dr.GetInt32(0);
            sqlcon.Close();
            return ToReturn;
        }

        public static async Task<string[]> ListNewlyRegisteredUsersAsync(this ApexVisualManager avm, DateTime date)
        {
            string cmd = "select username from UserAccount where " + GetTimeStampDayFilter("AccountCreatedAt", date);
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            List<string> ToReturn = new List<string>();
            while (dr.Read())
            {
                if (dr.IsDBNull(0) == false)
                {
                    ToReturn.Add(dr.GetString(0));
                }
            }
            return ToReturn.ToArray();
        }

        #endregion

        #region "Activity log operations"

        public async static Task<Guid> UploadActivityLogAsync(this ApexVisualManager avm, ActivityLog log)
        {
            Guid ToReturn = Guid.NewGuid();

            List<KeyValuePair<string, string>> ColumnValuePairs = new List<KeyValuePair<string, string>>();

            //This unique id (primary key)
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Id", "'" + ToReturn.ToString() + "'"));

            //Session id
            if (log.SessionId == null)
            {
                log.SessionId = new Guid(); //Blank (000000, etc.)
            }
            ColumnValuePairs.Add(new KeyValuePair<string, string>("SessionId", "'" + log.SessionId.ToString() + "'"));
            
            //Username
            if (log.Username != null & log.Username != "")
            {
                ColumnValuePairs.Add(new KeyValuePair<string, string>("Username", "'" + log.Username + "'"));
            }

            //TimeStamp
            if (log.TimeStamp == null)
            {
                log.TimeStamp = DateTimeOffset.Now;
            }
            ColumnValuePairs.Add(new KeyValuePair<string, string>("TimeStamp", "'" + log.TimeStamp.Year.ToString("0000") + "-" + log.TimeStamp.Month.ToString("00") + "-" + log.TimeStamp.Day.ToString("00") + " " + log.TimeStamp.Hour.ToString("00") + ":" + log.TimeStamp.Minute.ToString("00") + ":" + log.TimeStamp.Second.ToString("00") + "." + log.TimeStamp.Millisecond.ToString() + "'"));

            //ApplicationId
            ColumnValuePairs.Add(new KeyValuePair<string, string>("ApplicationId", Convert.ToInt32(log.ApplicationId).ToString()));

            //ActivityId
            ColumnValuePairs.Add(new KeyValuePair<string, string>("ActivityId", Convert.ToInt32(log.ActivityId).ToString()));

            //Package versions
            if (log.PackageVersion != null)
            {
                ColumnValuePairs.Add(new KeyValuePair<string, string>("PackageVersionMajor", log.PackageVersion.Major.ToString()));
                ColumnValuePairs.Add(new KeyValuePair<string, string>("PackageVersionMinor", log.PackageVersion.Minor.ToString()));
                ColumnValuePairs.Add(new KeyValuePair<string, string>("PackageVersionBuild", log.PackageVersion.Build.ToString()));
                ColumnValuePairs.Add(new KeyValuePair<string, string>("PackageVersionRevision", log.PackageVersion.Revision.ToString()));
            }

            //Note
            if (log.Note != null && log.Note != "")
            {
                string NoteToWrite = "";
                if (log.Note.Length > 255)
                {
                    NoteToWrite = log.Note.Substring(0, 255);
                }
                else
                {
                    NoteToWrite = log.Note;
                }
                ColumnValuePairs.Add(new KeyValuePair<string, string>("Note", "'" + log.Note + "'"));
            }

            //Prepare the command string
            string Component_Columns = "";
            string Component_Values = "";
            foreach (KeyValuePair<string, string> kvp in ColumnValuePairs)
            {
                Component_Columns = Component_Columns + kvp.Key + ",";
                Component_Values = Component_Values + kvp.Value + ",";
            }
            Component_Columns = Component_Columns.Substring(0, Component_Columns.Length-1); //Remove the last comma
            Component_Values = Component_Values.Substring(0, Component_Values.Length - 1);//Remove the last comma

            string cmd = "insert into ActivityLog (" + Component_Columns + ") values (" + Component_Values + ")";
            SqlConnection sqlcon = GetSqlConnection(avm);
            await sqlcon.OpenAsync();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();

            return ToReturn;
        }

        public async static Task<ActivityLog> DownloadActivityLogAsync(this ApexVisualManager avm, Guid id)
        {
            string cmd = "select SessionId,Username,TimeStamp,ApplicationId,ActivityId,PackageVersionMajor,PackageVersionMinor,PackageVersionBuild,PackageVersionRevision,Note from ActivityLog where Id='" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            if (dr.HasRows == false)
            {
                throw new Exception("Activity log with ID '" + id.ToString() + "' does not exist.");
            }

            dr.Read();

            ActivityLog ToReturn = new ActivityLog();
            
            //SessionId
            if (dr.IsDBNull(0) == false)
            {
                ToReturn.SessionId = dr.GetGuid(0);
            }

            //Username
            if (dr.IsDBNull(1) == false)
            {
                ToReturn.Username = dr.GetString(1);
            }

            //Timestamp
            if (dr.IsDBNull(2) == false)
            {
                ToReturn.TimeStamp = dr.GetDateTime(2);
            }
            
            //ApplicationId
            if (dr.IsDBNull(3) == false)
            {
                ToReturn.ApplicationId = (ApplicationType)dr.GetByte(3);
            }

            //ActivityId
            if (dr.IsDBNull(4) == false)
            {
                ToReturn.ActivityId = (ActivityType)dr.GetInt32(4);
            }

            //Major
            if (dr.IsDBNull(5) == false)
            {
                if (ToReturn.PackageVersion == null)
                {
                    ToReturn.PackageVersion = new PackageVersion();
                }
                ToReturn.PackageVersion.Major = (int)dr.GetInt16(5);
            }

            //Minor
            if (dr.IsDBNull(6) == false)
            {
                if (ToReturn.PackageVersion == null)
                {
                    ToReturn.PackageVersion = new PackageVersion();
                }
                ToReturn.PackageVersion.Minor = (int)dr.GetInt16(6);
            }

            //Build
            if (dr.IsDBNull(7) == false)
            {
                if (ToReturn.PackageVersion == null)
                {
                    ToReturn.PackageVersion = new PackageVersion();
                }
                ToReturn.PackageVersion.Build = (int)dr.GetInt16(7);
            }

            //Revision
            if (dr.IsDBNull(8) == false)
            {
                if (ToReturn.PackageVersion == null)
                {
                    ToReturn.PackageVersion = new PackageVersion();
                }
                ToReturn.PackageVersion.Revision = (int)dr.GetInt16(8);
            }

            //Note
            if (dr.IsDBNull(9) == false)
            {
                ToReturn.Note = dr.GetString(9);
            }

            sqlcon.Close();
            return ToReturn;
        }

        public static async Task<ActivityLog[]> DownloadActivityLogsBySessionAsync(this ApexVisualManager avm, Guid session_id)
        {
            string cmd = "select Username, TimeStamp, ApplicationId, ActivityId, PackageVersionMajor, PackageVersionMinor, PackageVersionBuild, PackageVersionRevision, Note from ActivityLog where SessionId='" + session_id + "' order by TimeStamp asc";
            SqlConnection sqlcon = GetSqlConnection(avm);
            await sqlcon.OpenAsync();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            List<ActivityLog> ToReturn = new List<ActivityLog>();
            while (dr.Read())
            {
                ActivityLog al = new ActivityLog();

                //If there is a username
                if (dr.IsDBNull(0) == false)
                {
                    al.Username = dr.GetString(0);
                }

                //if There is a timestamp
                if (dr.IsDBNull(1) == false)
                {
                    al.TimeStamp = dr.GetDateTime(1);
                }

                //ApplicationId
                al.ApplicationId = (ApplicationType)dr.GetByte(2);

                //ActivityId
                al.ActivityId = (ActivityType)dr.GetInt32(3);

                //Package major version
                if (dr.IsDBNull(4) == false)
                {
                    if (al.PackageVersion == null)
                    {
                        al.PackageVersion = new PackageVersion();
                    }
                    al.PackageVersion.Major = (int)dr.GetInt16(4);
                }

                //Package minor version
                if (dr.IsDBNull(5) == false)
                {
                    if (al.PackageVersion == null)
                    {
                        al.PackageVersion = new PackageVersion();
                    }
                    al.PackageVersion.Minor = (int)dr.GetInt16(5);
                }

                //Package build version
                if (dr.IsDBNull(6) == false)
                {
                    if (al.PackageVersion == null)
                    {
                        al.PackageVersion = new PackageVersion();
                    }
                    al.PackageVersion.Build = (int)dr.GetInt16(6);
                }

                //Package revision version
                if (dr.IsDBNull(7) == false)
                {
                    if (al.PackageVersion == null)
                    {
                        al.PackageVersion = new PackageVersion();
                    }
                    al.PackageVersion.Revision = (int)dr.GetInt16(7);
                }

                //Note
                if (dr.IsDBNull(8) == false)
                {
                    al.Note = dr.GetString(8);
                }

                ToReturn.Add(al);
            }

            sqlcon.Close();

            return ToReturn.ToArray();
        }

        public static async Task<int> CountActivityLogsAsync(this ApexVisualManager avm, DateTime date)
        {  
            string cmd = "select count(Id) from ActivityLog where " + GetTimeStampDayFilter("TimeStamp", date);
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            int ToReturn = 0;
            while (dr.Read())
            {
                ToReturn = dr.GetInt32(0);
            }
            sqlcon.Close();
            return ToReturn;
        }

        public static async Task<int> CountActivityLogsOfTypeAsync(this ApexVisualManager avm, DateTime date, ActivityType activity_type)
        {
            string cmd = "select count(Id) from ActivityLog where " + GetTimeStampDayFilter("TimeStamp", date) + " and ActivityId = " + Convert.ToInt32(activity_type).ToString();
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            await dr.ReadAsync();
            int ToReturn = dr.GetInt32(0);
            sqlcon.Close();
            return ToReturn;
        }

        public static async Task<int> CountDistinctActivitySessionsAsync(this ApexVisualManager avm, DateTime date, ApplicationType? app_type = null)
        {
            //Date
            string datetime_filter = GetTimeStampDayFilter("TimeStamp", date);

            //Application type filter
            string app_type_filter = "";
            if (app_type.HasValue)
            {
                app_type_filter = " and ApplicationId = " + Convert.ToString((int)app_type.Value);
            }

            string cmd = "select count(distinct SessionId) from ActivityLog where " + datetime_filter + app_type_filter;
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            int ToReturn = 0;
            while (dr.Read())
            {
                ToReturn = dr.GetInt32(0);
            }
            sqlcon.Close();
            return ToReturn;
        }

        public static async Task<Guid[]> GetUniqueActivitySessionIdsAsync(this ApexVisualManager avm, DateTime date, ApplicationType? app_type = null)
        {
            //Date
            string datetime_filter = GetTimeStampDayFilter("TimeStamp", date);

            //Application type filter
            string app_type_filter = "";
            if (app_type.HasValue)
            {
                app_type_filter = " and ApplicationId = " + Convert.ToString((int)app_type.Value);
            }

            string cmd = "select distinct SessionId from ActivityLog where " + datetime_filter + app_type_filter;
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            
            List<Guid> ToReturn = new List<Guid>();
            while (dr.Read())
            {
                ToReturn.Add(dr.GetGuid(0));
            }
            sqlcon.Close();
            return ToReturn.ToArray();
        }

        
        #endregion

        #region "Message Submission operations"

        public static async Task<Guid> UploadMessageSubmissionAsync(this ApexVisualManager avm, MessageSubmission msg, Guid? as_id = null)
        {
            //If an ID is supplied, use it. If not, just use a random one.
            Guid ToReturn = Guid.NewGuid();
            if (as_id != null)
            {
                ToReturn = as_id.Value;
            }

            List<KeyValuePair<string, string>> ColumnValuePairs = new List<KeyValuePair<string, string>>();

            //The ID
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Id", "'" + ToReturn.ToString() + "'"));

            //Username
            if (msg.Username != null)
            {
                if (msg.Username != "")
                {
                    ColumnValuePairs.Add(new KeyValuePair<string, string>("Username", "'" + msg.Username + "'"));
                }
            }

            //Email
            if (msg.Email != null)
            {
                if (msg.Email != "")
                {
                    ColumnValuePairs.Add(new KeyValuePair<string, string>("Email", "'" + msg.Email + "'"));
                }
            }

            //Message Type
            ColumnValuePairs.Add(new KeyValuePair<string, string>("MessageType", Convert.ToString((int)msg.MessageType)));

            //Created At
            string cas = msg.CreatedAt.Year.ToString("0000") + "-" + msg.CreatedAt.Month.ToString("00") + "-" + msg.CreatedAt.Day.ToString("00") + " " + msg.CreatedAt.Hour.ToString("00") + ":" + msg.CreatedAt.Minute.ToString("00") + ":" + msg.CreatedAt.Second.ToString("00") + "." + msg.CreatedAt.Millisecond.ToString();
            ColumnValuePairs.Add(new KeyValuePair<string, string>("CreatedAt", "'" + cas + "'"));

            //Prepare the command string
            string Component_Columns = "";
            string Component_Values = "";
            foreach (KeyValuePair<string, string> kvp in ColumnValuePairs)
            {
                Component_Columns = Component_Columns + kvp.Key + ",";
                Component_Values = Component_Values + kvp.Value + ",";
            }
            Component_Columns = Component_Columns.Substring(0, Component_Columns.Length-1); //Remove the last comma
            Component_Values = Component_Values.Substring(0, Component_Values.Length - 1);//Remove the last comma

            //Make the command
            string cmd = "insert into MessageSubmission (" + Component_Columns + ") values (" + Component_Values + ")";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();
            
            return ToReturn;
        }

        public static async Task<MessageSubmission> DownloadMessageSubmissionAsync(this ApexVisualManager avm, Guid id)
        {
            string cmd = "select Username, Email, MessageType, CreatedAt from MessageSubmission where Id='" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            if (dr.HasRows == false)
            {
                throw new Exception("Message submission with ID '" + id.ToString() + "' does not exist in SQL storage.");
            }

            await dr.ReadAsync();

            MessageSubmission ToReturn = new MessageSubmission();

            //Username
            if (dr.IsDBNull(0) == false)
            {
                ToReturn.Username = dr.GetString(0);
            }

            //Email
            if (dr.IsDBNull(1) == false)
            {
                ToReturn.Email = dr.GetString(1);
            }

            //MessageType
            if (dr.IsDBNull(2) == false)
            {
                ToReturn.MessageType = (MessageType)dr.GetByte(2);
            }

            //CreatedAt
            if (dr.IsDBNull(3) == false)
            {
                ToReturn.CreatedAt = dr.GetDateTime(3);
            }

            sqlcon.Close();

            return ToReturn;
        }

        public static async Task<int> CountMessageSubmissionsFromDateAsync(this ApexVisualManager avm, DateTime date)
        {
            string cmd = "select count(Id) from MessageSubmission where " + GetTimeStampDayFilter("CreatedAt", date);
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            await dr.ReadAsync();
            int ToReturn = dr.GetInt32(0);
            sqlcon.Close();
            return ToReturn;
        }

        public static async Task<Guid[]> ListMessageSubmissionIdsFromDateAsync(this ApexVisualManager avm, DateTime date)
        {
            string cmd = "select Id from MessageSubmission where " + GetTimeStampDayFilter("CreatedAt", date);
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            List<Guid> ToReturn = new List<Guid>();
            while (dr.Read())
            {
                ToReturn.Add(dr.GetGuid(0));
            }

            sqlcon.Close();
            return ToReturn.ToArray();
        }

        #endregion

        #region "Full Cascade operations"

        public static async Task<ulong> CascadeUploadSessionAsync(this ApexVisualManager avm, Session s, string owner_username = null)
        {
            SqlConnection sqlcon = GetSqlConnection(avm);

            //Upload the session
            ulong session_id = await avm.UploadSessionAsync(s);

            //If owner username is provided, set the owner
            if (owner_username != null)
            {
                sqlcon.Open();
                SqlCommand sqlcmdau = new SqlCommand("update Session set Owner = '" + owner_username + "' where SessionId = '" + session_id.ToString() + "'", sqlcon);
                await sqlcmdau.ExecuteNonQueryAsync();
                sqlcon.Close();
            }

            //Upload all laps and their children
            foreach (Lap l in s.Laps)
            {
                //Upload the lap
                Guid lap_id = await avm.UploadLapAsync(l);

                //Upload the incremental and beginning tyre wear for this lap
                Guid incremental_tyre_wear_id = await avm.UploadWheelDataArrayAsync(l.IncrementalTyreWear);
                Guid beginning_tyre_wear_id = await avm.UploadWheelDataArrayAsync(l.BeginningTyreWear);

                //Set the laps parent Session, incremental tyre wear, and beginning tyre wear
                sqlcon.Open();
                SqlCommand sqlcmdal = new SqlCommand("update Lap set SessionId = '" + session_id + "', IncrementalTyreWear = cast('" + incremental_tyre_wear_id.ToString() + "' as uniqueidentifier), BeginningTyreWear = cast('" + beginning_tyre_wear_id.ToString() + "' as uniqueidentifier) where Id = cast('" + lap_id.ToString() + "' as uniqueidentifier)" , sqlcon);
                await sqlcmdal.ExecuteNonQueryAsync();
                sqlcon.Close();

                //Upload all of the corners
                foreach (TelemetrySnapshot ts in l.Corners)
                {
                    //Cascade upload the corner
                    Guid ts_id = await avm.CascadeUploadTelemetrySnapshotAsync(ts);

                    //Set the corner's parent lap id and the corner type. since this is a corner, location type will be corner. Reference is in the draw.io ERD for what values these correspond to.
                    sqlcon.Open();
                    SqlCommand sqlcmd = new SqlCommand("update TelemetrySnapshot set LapId = cast('" + lap_id.ToString() + "' as uniqueidentifier), LocationType=1 where Id = cast('" + ts_id.ToString() + "' as uniqueidentifier)", sqlcon);
                    await sqlcmd.ExecuteNonQueryAsync();
                    sqlcon.Close();
                }
            }

            //Upload all LocationPerformanceAnalysis for Corners
            foreach (LocationPerformanceAnalysis lpa in s.Corners)
            {
                //Upload it
                Guid g = await avm.UploadLocationPerformanceAnalysisAsync(lpa);

                //Update it with the location type (corner) and the parent session
                string lpa_update_cmd = "update LocationPerformanceAnalysis set SessionId='" + session_id.ToString() + "', LocationType=1 where Id='" + g.ToString() + "'";
                sqlcon.Open();
                SqlCommand sqlcmd = new SqlCommand(lpa_update_cmd, sqlcon);
                await sqlcmd.ExecuteNonQueryAsync();
                sqlcon.Close();
            }

            return session_id;
        }

        public static async Task<Session> CascadeDownloadSessionAsync(this ApexVisualManager avm, ulong session_id)
        {
            //Get the session
            Session ToReturn = await avm.DownloadSessionAsync(session_id);

            #region "Get Laps"

            //Get all of the laps that are associated (attached) to this session
            string cmd = "select Id from Lap where SessionId='" + session_id.ToString() + "' order by LapNumber";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            //Get all of the GUIDs from that
            List<Guid> AssociatedLapIds = new List<Guid>();
            while (dr.Read())
            {
                Guid lap_id = dr.GetGuid(0);
                AssociatedLapIds.Add(lap_id);
            }

            //Close the sql conneciton
            sqlcon.Close();

            //Download each of the laps and attach them to the session
            List<Lap> Laps = new List<Lap>();
            foreach (Guid g in AssociatedLapIds)
            {
                Lap this_lap = await avm.CascadeDownloadLapAsync(g);
                Laps.Add(this_lap);
            }
            ToReturn.Laps = Laps.ToArray();

            #endregion

            #region "Get Location Performance Analysis - Corners"

            //Get all location performance analysis
            sqlcon.Open();
            string cmd_lpa = "select Id from LocationPerformanceAnalysis where SessionId='" + session_id.ToString() + "' and LocationType=1";
            SqlCommand sqlcmd_lpa = new SqlCommand(cmd_lpa, sqlcon);
            SqlDataReader dr_lpa = await sqlcmd_lpa.ExecuteReaderAsync();

            //Get a list of all guids
            List<Guid> LPA_IDs = new List<Guid>();
            while (dr_lpa.Read())
            {
                Guid g = dr_lpa.GetGuid(0);
                LPA_IDs.Add(g);
            }

            //Close the connection
            sqlcon.Close();

            //Download and attach all Location Performance Analyses
            List<LocationPerformanceAnalysis> LPAs = new List<LocationPerformanceAnalysis>();
            foreach (Guid g in LPA_IDs)
            {
                LocationPerformanceAnalysis lpa = await avm.DownloadLocationPerformanceAnalysisAsync(g);
                LPAs.Add(lpa);
            }
            ToReturn.Corners = LPAs.ToArray();


            #endregion
        
            return ToReturn;
        }

        public static async Task CascadeDeleteSessionAsync(this ApexVisualManager avm, ulong session_id)
        {
            SqlConnection sqlcon = GetSqlConnection(avm);

            #region "Associated LocationPerformanceAnalysis"

            //Make the command
            string cmd_LPA = "select Id from LocationPerformanceAnalysis where SessionId='" + session_id.ToString() + "'";
            sqlcon.Open();
            SqlCommand sqlcmd_LPA = new SqlCommand(cmd_LPA, sqlcon);
            SqlDataReader dr_LPA = await sqlcmd_LPA.ExecuteReaderAsync();
            

            //Get all of the LPA GUID's
            List<Guid> Ids_LPA = new List<Guid>();
            while (dr_LPA.Read())
            {
                Ids_LPA.Add(dr_LPA.GetGuid(0));
            }

            //Delete all of them
            foreach (Guid g in Ids_LPA)
            {
                await avm.DeleteLocationPerformanceAnalysisAsync(g);
            }

            sqlcon.Close();

            #endregion
            
            #region "Cascade Delete Associated Laps"

            string cmd_Lap = "select Id from Lap where SessionId='" + session_id.ToString() + "'";
            sqlcon.Open();
            SqlCommand sqlcmd_Lap = new SqlCommand(cmd_Lap, sqlcon);
            SqlDataReader dr_Lap = await sqlcmd_Lap.ExecuteReaderAsync();
            
            //Get all of the GUID's
            List<Guid> Ids_Laps = new List<Guid>();
            while (dr_Lap.Read())
            {
                Ids_Laps.Add(dr_Lap.GetGuid(0));
            }

            //Delete each of them
            foreach (Guid g in Ids_Laps)
            {
                await avm.CascadeDeleteLapAsync(g);
            }

            sqlcon.Close();

            #endregion

            //Delete the session itself
            await avm.DeleteSessionAsync(session_id);
        }

        public static async Task<Lap> CascadeDownloadLapAsync(this ApexVisualManager avm, Guid lap_id)
        {

            //Get the Lap
            Lap ToReturn = await avm.DownloadLapAsync(lap_id, false);

            #region "WheelDataArrays - IncrementalTyreWear, BeginningTyreWear"

            string column_selector = "IncrementalTyreWear, BeginningTyreWear";
            string cmd = "select " + column_selector + " from Lap where Id='" + lap_id.ToString() + "'";
            
            //Make the call to get the key values
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            
            //Check for no records
            if (dr.HasRows == false)
            {
                throw new Exception("Unable to find Lap record with Id '" + lap_id.ToString() + "'");
            }

            //Get the values
            await dr.ReadAsync();
            
            //Incremental tyre wear
            if (dr.IsDBNull(0) == false)
            {
                Guid id = dr.GetGuid(0);
                WheelDataArray wda = await avm.DownloadWheelDataArrayAsync(id);
                ToReturn.IncrementalTyreWear = wda;
            }

            //Beginning tyre wear
            if (dr.IsDBNull(1) == false)
            {
                Guid id = dr.GetGuid(1);
                WheelDataArray wda = await avm.DownloadWheelDataArrayAsync(id);
                ToReturn.BeginningTyreWear = wda;
            }

            sqlcon.Close();

            #endregion

            #region "TelemetrySnapshot - Corners"

            //Make the command
            string cmd_corners = "select Id from TelemetrySnapshot where LapId='" + lap_id.ToString() + "' and LocationType=1";
            sqlcon.Open();
            SqlCommand sqlcmd_corners = new SqlCommand(cmd_corners, sqlcon);
            SqlDataReader dr_corners = await sqlcmd_corners.ExecuteReaderAsync();

            //Get a list of the GUIDs
            List<Guid> AssociatedCorners = new List<Guid>();
            while (dr_corners.Read())
            {
                Guid g = dr_corners.GetGuid(0);
                AssociatedCorners.Add(g);
            }

            sqlcon.Close();

            //Download and attach them to the SessionAnalysis
            List<TelemetrySnapshot> Corners = new List<TelemetrySnapshot>();
            foreach (Guid g in AssociatedCorners)
            {
                TelemetrySnapshot ts = await avm.CascadeDownloadTelemetrySnapshotAsync(g);
                Corners.Add(ts);
            }
            ToReturn.Corners = Corners.ToArray();

            #endregion

            return ToReturn;
        }

        public static async Task CascadeDeleteLapAsync(this ApexVisualManager avm, Guid lap_id)
        {
            SqlConnection sqlcon = GetSqlConnection(avm);
            

            #region "Delete the two WheelDataArrays that are associated"

            sqlcon.Open();

            //Delete the two WheelDataArrays (IncrementalTyreWear and BeginningTyreWear) that are associatd with this field.
            string cmd_WDAs = "select IncrementalTyreWear, BeginningTyreWear from Lap where Id='" + lap_id.ToString() + "'";
            SqlCommand sqlcmd = new SqlCommand(cmd_WDAs, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            await dr.ReadAsync();
            
            if (dr.IsDBNull(0) == false)
            {
                Guid id1 = dr.GetGuid(0);
                await avm.DeleteWheelDataArrayAsync(id1);
            }
            
            if (dr.IsDBNull(1) == false)
            {
                Guid id2 = dr.GetGuid(1);
                await avm.DeleteWheelDataArrayAsync(id2);
            }
            
            sqlcon.Close();

            #endregion

            #region "Delete all associated Telemetry snaposhot"

            sqlcon.Open();

            string cmd_TSs = "select Id from TelemetrySnapshot where LapId='" + lap_id.ToString() + "'";
            SqlCommand sqlcmd_GetTSs = new SqlCommand(cmd_TSs, sqlcon);
            SqlDataReader dr_TSs = await sqlcmd_GetTSs.ExecuteReaderAsync();
            
            //Get a list of all associated TS Id's
            List<Guid> TS_Ids = new List<Guid>();
            while (dr_TSs.Read())
            {
                TS_Ids.Add(dr_TSs.GetGuid(0));
            }

            //Delete all of them
            foreach (Guid g in TS_Ids)
            {
                await avm.CascadeDeleteTelemetrySnapshot(g);
            }

            sqlcon.Close();

            #endregion

            //Delete the lap itself
            await avm.DeleteLapAsync(lap_id);
            
        }

        public static async Task<TelemetrySnapshot> CascadeDownloadTelemetrySnapshotAsync(this ApexVisualManager avm, Guid ts_id)
        {
            //Download TS
            TelemetrySnapshot ts = await avm.DownloadTelemetrySnapshotAsync(ts_id);

            //Get the accompanying WheelDataArrays for this TelemetrySnapshot
            string cmd = "select BrakeTemperature, TyreSurfaceTemperature, TyreInnerTemperature, TyreWearPercent, TyreDamagePercent from TelemetrySnapshot where Id='" + ts_id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            await dr.ReadAsync();

            //Get each of them
            
            //BrakeTemperature
            if (dr.IsDBNull(0) == false)
            {
                Guid id = dr.GetGuid(0);
                WheelDataArray wda = await avm.DownloadWheelDataArrayAsync(id);
                ts.BrakeTemperature = wda;
            }

            //TyreSurfaceTemperature
            if (dr.IsDBNull(1) == false)
            {
                Guid id = dr.GetGuid(1);
                WheelDataArray wda = await avm.DownloadWheelDataArrayAsync(id);
                ts.TyreSurfaceTemperature = wda;
            }

            //TyreInnerTemperature
            if (dr.IsDBNull(2) == false)
            {
                Guid id = dr.GetGuid(2);
                WheelDataArray wda = await avm.DownloadWheelDataArrayAsync(id);
                ts.TyreInnerTemperature = wda;
            }

            //TyreWearPercent
            if (dr.IsDBNull(3) == false)
            {
                Guid id = dr.GetGuid(3);
                WheelDataArray wda = await avm.DownloadWheelDataArrayAsync(id);
                ts.TyreWearPercent = wda;
            }

            //TyreDamagePercent
            if (dr.IsDBNull(4) == false)
            {
                Guid id = dr.GetGuid(4);
                WheelDataArray wda = await avm.DownloadWheelDataArrayAsync(id);
                ts.TyreDamagePercent = wda;
            }

            sqlcon.Close();

            return ts;
        }

        public static async Task<Guid> CascadeUploadTelemetrySnapshotAsync(this ApexVisualManager avm, TelemetrySnapshot ts)
        {
            //Upload the TS
            Guid ts_guid = await avm.UploadTelemetrySnapshotAsync(ts);

            //Upload Brake Temperature
            Guid wda_BrakeTemperature = await avm.UploadWheelDataArrayAsync(ts.BrakeTemperature);
            string set_BrakeTemperature = "BrakeTemperature=cast('" + wda_BrakeTemperature.ToString() + "' as uniqueidentifier)";

            //Upload Tyre Surface Temperature
            Guid wda_TyreSurfaceTemperature = await avm.UploadWheelDataArrayAsync(ts.TyreSurfaceTemperature);
            string set_TyreSurfaceTemperature = "TyreSurfaceTemperature=cast('" + wda_TyreSurfaceTemperature.ToString() + "' as uniqueidentifier)";

            //Upload Tyre Inner Temperature
            Guid wda_TyreInnerTemperature = await avm.UploadWheelDataArrayAsync(ts.TyreInnerTemperature);
            string set_TyreInnerTemperature = "TyreInnerTemperature=cast('" + wda_TyreInnerTemperature.ToString() + "' as uniqueidentifier)";

            //Upload Tyre Wear Percent
            Guid wda_TyreWearPercentage = await avm.UploadWheelDataArrayAsync(ts.TyreWearPercent);
            string set_TyreWearPercentage = "TyreWearPercent=cast('" + wda_TyreWearPercentage.ToString() + "' as uniqueidentifier)";

            //Upload Tyre Damage Percent
            Guid wda_TyreDamagePercentage = await avm.UploadWheelDataArrayAsync(ts.TyreDamagePercent);
            string set_TyreDamagePercentage = "TyreDamagePercent=cast('" + wda_TyreDamagePercentage.ToString() + "' as uniqueidentifier)";

            //Update the TS to plug in these values
            SqlConnection sqlcon = GetSqlConnection(avm);
            string cmd = "update TelemetrySnapshot set " + set_BrakeTemperature + ", " + set_TyreSurfaceTemperature + ", " + set_TyreInnerTemperature + ", " + set_TyreWearPercentage + ", " + set_TyreDamagePercentage + " where Id=cast('" + ts_guid.ToString() + "' as uniqueidentifier)";
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();
            
            return ts_guid;
        }

        public static async Task CascadeDeleteTelemetrySnapshot(this ApexVisualManager avm, Guid id)
        {
            //Get the Id's of the WheelDataArrays this is referencing
            string cmd = "select BrakeTemperature, TyreSurfaceTemperature, TyreInnerTemperature, TyreWearPercent, TyreDamagePercent from TelemetrySnapshot where Id='" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            dr.Read();

            if (dr.IsDBNull(0) == false)
            {
                Guid thisid = dr.GetGuid(0);
                await avm.DeleteWheelDataArrayAsync(thisid);
            }

            if (dr.IsDBNull(1) == false)
            {
                Guid thisid = dr.GetGuid(1);
                await avm.DeleteWheelDataArrayAsync(thisid);
            }

            if (dr.IsDBNull(2) == false)
            {
                Guid thisid = dr.GetGuid(2);
                await avm.DeleteWheelDataArrayAsync(thisid);
            }

            if (dr.IsDBNull(3) == false)
            {
                Guid thisid = dr.GetGuid(3);
                await avm.DeleteWheelDataArrayAsync(thisid);
            }

            if (dr.IsDBNull(4) == false)
            {
                Guid thisid = dr.GetGuid(4);
                await avm.DeleteWheelDataArrayAsync(thisid);
            }

            sqlcon.Close();

            //Now delete the telemetry snapshot itself
            await avm.DeleteTelemetrySnapshotAsync(id);

            
        }

        #endregion

        #region "Helper functions"

        private static SqlConnection GetSqlConnection(this ApexVisualManager avm)
        {
            SqlConnection con = new SqlConnection(avm.AzureSqlDbConnectionString);
            return con;
        }

        private static string GetTimeStampDayFilter(string column_name, DateTime date)
        {
            string date_START = date.Year.ToString("0000") + "-" + date.Month.ToString("00") + "-" + date.Day.ToString("00");
            string date_END = (date.AddDays(1)).Year.ToString("0000") + "-" + (date.AddDays(1)).Month.ToString("00") + "-" + (date.AddDays(1)).Day.ToString("00");
            string ToReturn = column_name + " >= '" + date_START + "' and " + column_name + " < '" + date_END + "'";
            return ToReturn;
        }

        #endregion
    
        #region "Shallow Transactions (affecting a single table only, not meant to be used outside this)"

        public static async Task<ulong> UploadSessionAsync(this ApexVisualManager avm, Session to_upload)
        {
        
            List<KeyValuePair<string, string>> ColumnValuePairs = new List<KeyValuePair<string, string>>();

            ColumnValuePairs.Add(new KeyValuePair<string, string>("SessionId", "'" + to_upload.SessionId.ToString() + "'"));
            //Skip the "Owner" field right now - that is a lookup to the user, so that can be done later.
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Game", "2020"));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Circuit", Convert.ToInt32(to_upload.Circuit).ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("SessionMode", Convert.ToInt32(to_upload.SessionMode).ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("SelectedTeam", Convert.ToInt32(to_upload.SelectedTeam).ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("SelectedDriver", Convert.ToInt32(to_upload.SelectedDriver).ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("DriverName", "'" + to_upload.DriverName + "'"));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("SessionCreatedAt", "'" + to_upload.SessionCreatedAt.Year.ToString("0000") + "-" + to_upload.SessionCreatedAt.Month.ToString("00") + "-" + to_upload.SessionCreatedAt.Day.ToString("00") + " " + to_upload.SessionCreatedAt.Hour.ToString() + ":" + to_upload.SessionCreatedAt.Minute.ToString() + ":" + to_upload.SessionCreatedAt.Second.ToString() + "'"));          

            //Prepare the command string
            string Component_Columns = "";
            string Component_Values = "";
            foreach (KeyValuePair<string, string> kvp in ColumnValuePairs)
            {
                Component_Columns = Component_Columns + kvp.Key + ",";
                Component_Values = Component_Values + kvp.Value + ",";
            }
            Component_Columns = Component_Columns.Substring(0, Component_Columns.Length-1); //Remove the last comma
            Component_Values = Component_Values.Substring(0, Component_Values.Length - 1);//Remove the last comma
            string cmd = "insert into Session (" + Component_Columns + ") values (" + Component_Values + ")";

            //Make the call
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();

            return to_upload.SessionId;
        }

        public async static Task<Guid> UploadLapAsync(this ApexVisualManager avm, Lap l)
        {
            Guid g = Guid.NewGuid();
            string this_guid = g.ToString();

            List<KeyValuePair<string, string>> ColumnValuePairs = new List<KeyValuePair<string, string>>();

            ColumnValuePairs.Add(new KeyValuePair<string, string>("Id", "cast('" + this_guid + "' as uniqueidentifier)"));
            
            //Skip SessionId (this is a look up to the parent session. This will be done later)

            ColumnValuePairs.Add(new KeyValuePair<string, string>("LapNumber", l.LapNumber.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Sector1Time", l.Sector1Time.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Sector2Time", l.Sector2Time.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Sector3Time", l.Sector3Time.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("FuelConsumed", l.FuelConsumed.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PercentOnThrottle", l.PercentOnThrottle.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PercentOnBrake", l.PercentOnBrake.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PercentCoasting", l.PercentCoasting.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PercentThrottleBrakeOverlap", l.PercentThrottleBrakeOverlap.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PercentOnMaxThrottle", l.PercentOnMaxThrottle.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PercentOnMaxBrake", l.PercentOnMaxBrake.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("ErsDeployed", l.ErsDeployed.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("ErsHarvested", l.ErsHarvested.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("GearChanges", l.GearChanges.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("TopSpeedKph", l.TopSpeedKph.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("EquippedTyreCompound", Convert.ToInt32(l.EquippedTyreCompound).ToString()));

            //Skip IncrementalTyreWear (this is a lookup to a WheelDataArray, will plug it in later)
            //Skip BeginningTyreWear (this is a lookup to a WheelDataArray, will plug it in later)

            //Prepare the cmd
            
            //Prepare the command string
            string Component_Columns = "";
            string Component_Values = "";
            foreach (KeyValuePair<string, string> kvp in ColumnValuePairs)
            {
                Component_Columns = Component_Columns + kvp.Key + ",";
                Component_Values = Component_Values + kvp.Value + ",";
            }
            Component_Columns = Component_Columns.Substring(0, Component_Columns.Length-1); //Remove the last comma
            Component_Values = Component_Values.Substring(0, Component_Values.Length - 1);//Remove the last comma

            string cmd = "insert into Lap (" + Component_Columns + ") values (" + Component_Values + ")";

            //Make the call
            SqlConnection con = GetSqlConnection(avm);
            con.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, con);
            await sqlcmd.ExecuteNonQueryAsync();
            con.Close();

            //Return
            return g;
        }

        public async static Task<Guid> UploadTelemetrySnapshotAsync(this ApexVisualManager avm, TelemetrySnapshot snapshot)
        {
            

            List<KeyValuePair<string, string>> ColumnValuePairs = new List<KeyValuePair<string, string>>();

            //Id (uniqueidentifier)
            Guid g = Guid.NewGuid();
            string uniqueidentifier = g.ToString();
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Id", "cast('" + uniqueidentifier + "' as uniqueidentifier)"));

            //Skip parent lap analysis (this is a lookup to Lap, will have to be done after upload)            

            //Skip location type (this will have to be done after the fact)

            //Location number
            ColumnValuePairs.Add(new KeyValuePair<string, string>("LocationNumber", snapshot.LocationNumber.ToString()));

            //Positions
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PositionX", snapshot.PositionX.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PositionY", snapshot.PositionY.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PositionZ", snapshot.PositionZ.ToString()));

            //Velocities
            ColumnValuePairs.Add(new KeyValuePair<string, string>("VelocityX", snapshot.VelocityX.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("VelocityY", snapshot.VelocityY.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("VelocityZ", snapshot.VelocityZ.ToString()));

            //gForce
            ColumnValuePairs.Add(new KeyValuePair<string, string>("gForceLateral", snapshot.gForceLateral.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("gForceLongitudinal", snapshot.gForceLongitudinal.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("gForceVertical", snapshot.gForceVertical.ToString()));

            //Yaw, pitch, and roll
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Yaw", snapshot.Yaw.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Pitch", snapshot.Pitch.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Roll", snapshot.Roll.ToString()));

            //Current lap time
            ColumnValuePairs.Add(new KeyValuePair<string, string>("CurrentLapTime", snapshot.CurrentLapTime.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("CarPosition", snapshot.CarPosition.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("LapInvalid", Convert.ToInt32(snapshot.LapInvalid).ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Penalties", snapshot.Penalties.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("SpeedKph", snapshot.SpeedKph.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Throttle", snapshot.Throttle.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Steer", snapshot.Steer.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Brake", snapshot.Brake.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Clutch", snapshot.Clutch.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("Gear", snapshot.Gear.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("EngineRpm", snapshot.EngineRpm.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("DrsActive", Convert.ToInt32(snapshot.DrsActive).ToString()));
            
            //Skip BrakeTemperature (it is a lookup to a WheelDataArray)
            //Skip TyreSurfaceTemperature (it is a lookup to a WheelDataArray)
            //Skip TyreInnerTemperature (it is a lookup to a WheelDataArray)

            ColumnValuePairs.Add(new KeyValuePair<string, string>("EngineTemperature", snapshot.EngineTemperature.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("SelectedFuelMix", Convert.ToInt32(snapshot.SelectedFuelMix).ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("FuelLevel", snapshot.FuelLevel.ToString()));

            //Skip TyreWearPercentage (it is a lookup to a WheelDataArray)
            //Skup TyreDamagePercentage (it is a lookup to a WheelDataArray)

            ColumnValuePairs.Add(new KeyValuePair<string, string>("FrontLeftWingDamage", snapshot.FrontLeftWingDamage.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("FrontRightWingDamage", snapshot.FrontRightWingDamage.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("RearWingDamage", snapshot.RearWingDamage.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("ErsStored", snapshot.ErsStored.ToString()));

            //Prepare the command string
            string Component_Columns = "";
            string Component_Values = "";
            foreach (KeyValuePair<string, string> kvp in ColumnValuePairs)
            {
                Component_Columns = Component_Columns + kvp.Key + ",";
                Component_Values = Component_Values + kvp.Value + ",";
            }
            Component_Columns = Component_Columns.Substring(0, Component_Columns.Length-1); //Remove the last comma
            Component_Values = Component_Values.Substring(0, Component_Values.Length - 1);//Remove the last comma

            string cmd = "insert into TelemetrySnapshot (" + Component_Columns + ") values (" + Component_Values + ")";

            //Make the call
            SqlConnection sql = GetSqlConnection(avm);
            sql.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sql);
            await sqlcmd.ExecuteNonQueryAsync();

            //Close the connecton
            sql.Close();

            return g;
        }

        public async static Task<Guid> UploadWheelDataArrayAsync(this ApexVisualManager avm, WheelDataArray wda)
        {
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();

            Guid g = Guid.NewGuid();
            string this_guid = g.ToString();

            string cmd = "insert into WheelDataArray (Id, RearLeft, RearRight, FrontLeft, FrontRight) values (" + "cast('" + this_guid + "' as uniqueidentifier), " + wda.RearLeft.ToString() + ", " + wda.RearRight.ToString() + ", " + wda.FrontLeft.ToString() + ", " + wda.FrontRight.ToString() + ")";
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();

            sqlcon.Close();

            return g;
        }

        public async static Task<Guid> UploadLocationPerformanceAnalysisAsync(this ApexVisualManager avm, LocationPerformanceAnalysis lpa)
        {
            Guid g = Guid.NewGuid();

            List<KeyValuePair<string, string>> ColumnValuePairs = new List<KeyValuePair<string, string>>();

            ColumnValuePairs.Add(new KeyValuePair<string, string>("Id", "'" + g.ToString() + "'"));
            //Skip location type (this can be done after the fact by a parent cascading method)
            ColumnValuePairs.Add(new KeyValuePair<string, string>("LocationNumber", lpa.LocationNumber.ToString()));

            //Average speed kph
            if (lpa.AverageSpeedKph.ToString() != "NaN")
            {
                ColumnValuePairs.Add(new KeyValuePair<string, string>("AverageSpeedKph", lpa.AverageSpeedKph.ToString()));
            }
            
            //Average gear
            if (lpa.AverageGear.ToString() != "NaN")
            {
                ColumnValuePairs.Add(new KeyValuePair<string, string>("AverageGear", lpa.AverageGear.ToString()));
            }
            
            //Average distance to apex
            if (lpa.AverageDistanceToApex.ToString() != "NaN")
            {
                ColumnValuePairs.Add(new KeyValuePair<string, string>("AverageDistanceToApex", lpa.AverageDistanceToApex.ToString()));
            }

            //Inconsistency rating
            if (lpa.InconsistencyRating.ToString() != "NaN")
            {
                ColumnValuePairs.Add(new KeyValuePair<string, string>("InconsistencyRating", lpa.InconsistencyRating.ToString()));
            }
            
            //Prepare the command string
            string Component_Columns = "";
            string Component_Values = "";
            foreach (KeyValuePair<string, string> kvp in ColumnValuePairs)
            {
                Component_Columns = Component_Columns + kvp.Key + ",";
                Component_Values = Component_Values + kvp.Value + ",";
            }
            Component_Columns = Component_Columns.Substring(0, Component_Columns.Length-1); //Remove the last comma
            Component_Values = Component_Values.Substring(0, Component_Values.Length - 1);//Remove the last comma
            string cmd = "insert into LocationPerformanceAnalysis (" + Component_Columns + ") values (" + Component_Values + ")";

            //Make the call
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();

            return g;
        }

        public async static Task<WheelDataArray> DownloadWheelDataArrayAsync(this ApexVisualManager avm, Guid id)
        {
            string cmd = "select * from WheelDataArray where Id = '" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            if (dr.HasRows == false)
            {
                throw new Exception("Unable to find WheelDataArray record with Id '" + id.ToString() + "'");
            }

            //Get the data into the object to return
            dr.Read();
            WheelDataArray ToReturn = new WheelDataArray();
            ToReturn.RearLeft = dr.GetFloat(1);
            ToReturn.RearRight = dr.GetFloat(2);
            ToReturn.FrontLeft = dr.GetFloat(3);
            ToReturn.FrontRight = dr.GetFloat(4);
            sqlcon.Close();

            return ToReturn;
        }

        public async static Task<Lap> DownloadLapAsync(this ApexVisualManager avm, Guid id, bool timings_only = false)
        {
            //Set the column selector
            string column_selector = "";
            if (timings_only)
            {
                column_selector = "LapNumber, Sector1Time, Sector2Time, Sector3Time";
            }
            else
            {
                //Select every column except those that will not be used (will not be used: Id, SessionId, IncrementalTyreWear, BeginningTyreWear)
                column_selector = "LapNumber, Sector1Time, Sector2Time, Sector3Time, FuelConsumed, PercentOnThrottle, PercentOnBrake, PercentCoasting, PercentThrottleBrakeOverlap, PercentOnMaxThrottle, PercentOnMaxBrake, ErsDeployed, ErsHarvested, GearChanges, TopSpeedKph, EquippedTyreCompound";
            }

            string cmd = "select " + column_selector + " from Lap where Id = '" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            if (dr.HasRows == false)
            {
                throw new Exception("Unable to find Lap record with Id '" + id.ToString() + "'");
            }

            //Parse it into an object
            await dr.ReadAsync();
            Lap ToReturn = new Lap();
            if (timings_only)
            {
                ToReturn.LapNumber = dr.GetByte(0);
                ToReturn.Sector1Time = dr.GetFloat(1);
                ToReturn.Sector2Time = dr.GetFloat(2);
                ToReturn.Sector3Time = dr.GetFloat(3);
            }
            else
            {
                ToReturn.LapNumber = dr.GetByte(0);
                ToReturn.Sector1Time = dr.GetFloat(1);
                ToReturn.Sector2Time = dr.GetFloat(2);
                ToReturn.Sector3Time = dr.GetFloat(3);
                ToReturn.FuelConsumed = dr.GetFloat(4);
                ToReturn.PercentOnThrottle = dr.GetFloat(5);
                ToReturn.PercentOnBrake = dr.GetFloat(6);
                ToReturn.PercentCoasting = dr.GetFloat(7);
                ToReturn.PercentThrottleBrakeOverlap = dr.GetFloat(8);
                ToReturn.PercentOnMaxThrottle = dr.GetFloat(9);
                ToReturn.PercentOnMaxBrake = dr.GetFloat(10);
                ToReturn.ErsDeployed = dr.GetFloat(11);
                ToReturn.ErsHarvested = dr.GetFloat(12);
                ToReturn.GearChanges = dr.GetInt32(13);
                ToReturn.TopSpeedKph = Convert.ToUInt16(dr.GetInt16(14));
                ToReturn.EquippedTyreCompound = (TyreCompound)dr.GetByte(15);
            }

            //Close the connection
            sqlcon.Close();

            return ToReturn;
        }

        public async static Task<TelemetrySnapshot> DownloadTelemetrySnapshotAsync(this ApexVisualManager avm, Guid id)
        {
            //Set the column selector
            //Get all of the columns that we will use in this object - do not include any sql-only properties or any lookups to other entities (i.e. WheelDataArray)
            string column_selector = "LocationNumber, PositionX, PositionY, PositionZ, VelocityX, VelocityY, VelocityZ, gForceLateral, gForceLongitudinal, gForceVertical, Yaw, Pitch, Roll, CurrentLapTime, CarPosition, LapInvalid, Penalties, SpeedKph, Throttle, Steer, Brake, Clutch, Gear, EngineRpm, DrsActive, EngineTemperature, SelectedFuelMix, FuelLevel, FrontLeftWingDamage, FrontRightWingDamage, RearWingDamage, ErsStored";

            //Make the call
            string cmd = "select " + column_selector + " from TelemetrySnapshot where Id='" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            if (dr.HasRows == false)
            {
                throw new Exception("Unable to find TelemetrySnapshot record with Id '" + id.ToString() + "'");
            }

            //Get the object
            await dr.ReadAsync();
            TelemetrySnapshot ToReturn = new TelemetrySnapshot();
            ToReturn.LocationNumber = dr.GetByte(0);
            ToReturn.PositionX = dr.GetFloat(1);
            ToReturn.PositionY = dr.GetFloat(2);
            ToReturn.PositionZ = dr.GetFloat(3);
            ToReturn.VelocityX = dr.GetFloat(4);
            ToReturn.VelocityY = dr.GetFloat(5);
            ToReturn.VelocityZ = dr.GetFloat(6);
            ToReturn.gForceLateral = dr.GetFloat(7);
            ToReturn.gForceLongitudinal = dr.GetFloat(8);
            ToReturn.gForceVertical = dr.GetFloat(9);
            ToReturn.Yaw = dr.GetFloat(10);
            ToReturn.Pitch = dr.GetFloat(11);
            ToReturn.Roll = dr.GetFloat(12);
            ToReturn.CurrentLapTime = dr.GetFloat(13);
            ToReturn.CarPosition = dr.GetByte(14);
            ToReturn.LapInvalid = dr.GetBoolean(15);
            ToReturn.Penalties = dr.GetByte(16);
            ToReturn.SpeedKph = Convert.ToUInt16(dr.GetInt32(17));
            ToReturn.Throttle = dr.GetFloat(18);
            ToReturn.Steer = dr.GetFloat(19);
            ToReturn.Brake = dr.GetFloat(20);
            ToReturn.Clutch = dr.GetFloat(21);
            ToReturn.Gear = Convert.ToSByte(dr.GetInt16(22));
            ToReturn.EngineRpm = dr.GetInt32(23);
            ToReturn.DrsActive = dr.GetBoolean(24);
            ToReturn.EngineTemperature = dr.GetInt32(25);
            ToReturn.SelectedFuelMix = (FuelMix)dr.GetByte(26);
            ToReturn.FuelLevel = dr.GetFloat(27);
            ToReturn.FrontLeftWingDamage = dr.GetFloat(28);
            ToReturn.FrontRightWingDamage = dr.GetFloat(29);
            ToReturn.RearWingDamage = dr.GetFloat(30);
            ToReturn.ErsStored = dr.GetFloat(31);

            sqlcon.Close();

            return ToReturn;

        }

        public async static Task<LocationPerformanceAnalysis> DownloadLocationPerformanceAnalysisAsync(this ApexVisualManager avm, Guid id)
        {
            string column_selector = "LocationNumber, AverageSpeedKph, AverageGear, AverageDistanceToApex, InconsistencyRating";
            string cmd = "select " + column_selector + " from LocationPerformanceAnalysis where Id='" + id.ToString() + "'";

            //make the call
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            if (dr.HasRows == false)
            {
                throw new Exception("Unable to find LocationPerformanceAnalysis with Id '" + id.ToString() + "'");
            }

            //Get the object
            dr.Read();
            LocationPerformanceAnalysis ToReturn = new LocationPerformanceAnalysis();
            ToReturn.LocationNumber = dr.GetByte(0);
            ToReturn.AverageSpeedKph = dr.GetFloat(1);
            ToReturn.AverageGear = dr.GetFloat(2);
            if (dr.IsDBNull(3) == false)
            {
                ToReturn.AverageDistanceToApex = dr.GetFloat(3);
            }
            else
            {
                ToReturn.AverageDistanceToApex = float.NaN;
            }
            if (dr.IsDBNull(4) == false)
            {
                ToReturn.InconsistencyRating = dr.GetFloat(4);
            }
            else
            {
                ToReturn.InconsistencyRating = float.NaN;
            }
            
            sqlcon.Close();

            return ToReturn;
        }

        public async static Task<Session> DownloadSessionAsync(this ApexVisualManager avm, ulong session_id)
        {
            string column_selector = "Circuit, SessionMode, SelectedTeam, SelectedDriver, DriverName, SessionCreatedAt";
            string cmd = "select " + column_selector + " from Session where SessionId='" + session_id + "'";

            //Make the call
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            if (dr.HasRows == false)
            {
                throw new Exception("Unable to find Session with SessionId '" + session_id.ToString() + "'");
            }

            //Get the return package
            await dr.ReadAsync();
            Session ToReturn = new Session();
            ToReturn.SessionId = session_id;
            ToReturn.Circuit = (Track)dr.GetByte(0);
            ToReturn.SessionMode = (SessionPacket.SessionType)dr.GetByte(1);
            ToReturn.SelectedTeam = (Team)dr.GetByte(2);
            ToReturn.SelectedDriver = (Driver)dr.GetByte(3);
            ToReturn.DriverName = dr.GetString(4);
            ToReturn.SessionCreatedAt = dr.GetDateTime(5);

            sqlcon.Close();

            return ToReturn;
        }

        public async static Task DeleteWheelDataArrayAsync(this ApexVisualManager avm, Guid id)
        {
            string cmd = "delete from WheelDataArray where Id='" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();
        }

        public async static Task DeleteLapAsync(this ApexVisualManager avm, Guid id)
        {
            string cmd = "delete from Lap where Id='" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();
        }

        public async static Task DeleteTelemetrySnapshotAsync(this ApexVisualManager avm, Guid id)
        {
            string cmd = "delete from TelemetrySnapshot where Id='" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();
        }

        public async static Task DeleteLocationPerformanceAnalysisAsync(this ApexVisualManager avm, Guid id)
        {
            string cmd = "delete from LocationPerformanceAnalysis where Id='" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();
        }

        public async static Task DeleteSessionAsync(this ApexVisualManager avm, ulong session_id)
        {
            string cmd = "delete from Session where SessionId='" + session_id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();
        }

        /// <summary>
        /// Downloads core lap data: lap number, sector times, equipped tyre compound
        /// </summary>
        public async static Task<Lap> DownloadLapCoreAsync(this ApexVisualManager avm, Guid id)
        {
            string cmd = "select LapNumber, Sector1Time, Sector2Time, Sector3Time, EquippedTyreCompound from Lap where Id='" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            
            if (dr.HasRows == false)
            {
                throw new Exception("Unable to find a lap record with Id '" + id.ToString() + "'");
            }

            await dr.ReadAsync();

            Lap ToReturn = new Lap();
            
            //Lap number
            if (dr.IsDBNull(0) == false)
            {
                ToReturn.LapNumber = dr.GetByte(0);
            }

            //Sector 1 time
            if (dr.IsDBNull(1) == false)
            {
                ToReturn.Sector1Time = dr.GetFloat(1);
            }

            //Sector 2 time
            if (dr.IsDBNull(2) == false)
            {
                ToReturn.Sector2Time = dr.GetFloat(2);
            }

            //Sector 3 time
            if (dr.IsDBNull(3) == false)
            {
                ToReturn.Sector3Time = dr.GetFloat(3);
            }

            //Selected tyre compound
            if (dr.IsDBNull(4) == false)
            {
                ToReturn.EquippedTyreCompound = (TyreCompound)dr.GetByte(4);
            }

            sqlcon.Close();
            return ToReturn;
        }

        #endregion
    
        #region "Session Search"

        public static async Task<string[]> SearchSessionsAsync(this ApexVisualManager avm, string owner_username = null, SessionPacket.SessionType[] session_types = null, Track[] tracks = null, DateTime? date = null)
        {

            #region "Prepare filters"

            List<string> Filters = new List<string>();

            //Owners name (Username)
            if (owner_username != null)
            {
                Filters.Add("Owner = '" + owner_username + "'");
            }

            //Session types
            if (session_types != null)
            {
                //Get the filters
                List<string> SessionTypeFilters = new List<string>();
                foreach (SessionPacket.SessionType st in session_types)
                {
                    SessionTypeFilters.Add("SessionMode = " + Convert.ToInt32(st).ToString());
                }

                //Prepare it
                string SessionTypePortion = "(";
                foreach (string s in SessionTypeFilters)
                {
                    SessionTypePortion = SessionTypePortion + s + " OR ";
                }

                //Remove the last " OR "
                SessionTypePortion = SessionTypePortion.Substring(0, SessionTypePortion.Length - 4);

                //Close it off
                SessionTypePortion = SessionTypePortion + ")";

                //Add it
                Filters.Add(SessionTypePortion);
            }

            //Tracks
            if (tracks != null)
            {
                //Get the filters
                List<string> TrackFilters = new List<string>();
                foreach (Track t in tracks)
                {
                    TrackFilters.Add("Circuit = " + Convert.ToInt32(t).ToString());
                }

                //Prepare it
                string TrackTypePortion = "(";
                foreach (string s in TrackFilters)
                {
                    TrackTypePortion = TrackTypePortion + s + " OR ";
                }

                //Remove the last " OR "
                TrackTypePortion = TrackTypePortion.Substring(0, TrackTypePortion.Length - 4);

                //Close it off
                TrackTypePortion = TrackTypePortion + ")";

                //Add it
                Filters.Add(TrackTypePortion);
            }

            //Date
            if (date != null)
            {
                Filters.Add(GetTimeStampDayFilter("SessionCreatedAt", date.Value));
            }



            #endregion

            #region "Prepare Command"

            string full_filter = "";
            foreach (string s in Filters)
            {
                full_filter = full_filter + s + " AND ";
            }

            //Remove the last and
            if (Filters.Count > 0)
            {
                full_filter = full_filter.Substring(0, full_filter.Length - 5);
            }
            
            string cmd = "select SessionId from Session";
            
            if (Filters.Count > 0)
            {
                cmd = cmd + " where " + full_filter;
            }
             
            #endregion

            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();

            List<string> ToReturn = new List<string>();
            while (dr.Read())
            {
                ToReturn.Add(dr.GetString(0));
            }

            sqlcon.Close();

            return ToReturn.ToArray();
        }

        #endregion
    
        #region "Flex downloads - downloads that are specifically designed around a scenario (for example, download session with lap core, or with results for a race)"

        public static async Task<Session> FlexDownloadSessionAsync(this ApexVisualManager avm, ulong session_id)
        {
            //Get the session
            Session ToReturn = null;
            try
            {
                ToReturn = await avm.DownloadSessionAsync(session_id);
            }
            catch
            {
                throw new Exception("Unable to download session object for session with ID '" + session_id + "'");
            }
        
            //Get a list of this sessions lap guids
            string cmd = "select Id from Lap where SessionId='" + session_id + "'";
            SqlConnection sqlcon = GetSqlConnection(avm);
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            List<Guid> AssociatedLaps = new List<Guid>();
            while (dr.Read())
            {
                if (dr.IsDBNull(0) == false)
                {
                    AssociatedLaps.Add(dr.GetGuid(0));
                }
            }

            //Download core data for each of them
            List<Lap> CoreLaps = new List<Lap>();
            foreach (Guid g in AssociatedLaps)
            {
                try
                {
                    Lap ThisLap = await avm.DownloadLapCoreAsync(g);
                    CoreLaps.Add(ThisLap);
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to download core lap data for lap with ID '" + g.ToString() + "'. Msg: " + ex.Message);
                }
            }

            //Return it
            ToReturn.Laps = CoreLaps.ToArray();
            return ToReturn;        
        }

        #endregion
    }
}