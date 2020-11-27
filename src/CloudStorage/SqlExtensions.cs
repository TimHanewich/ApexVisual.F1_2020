using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApexVisual.F1_2020.Analysis;
using Codemasters.F1_2020;

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
                TableCreationCommands.Add("create table Track (Id tinyint not null primary key, CountryCode varchar(2), Latitude real, Longitude real)");
            }

            #endregion

            #endregion

            #region "Sessions"
   
            //Session
            if (TablesThatAlreadyExist.Contains("Session") == false)
            {
                TableCreationCommands.Add("create table Session (SessionId varchar(30) not null primary key, Owner varchar(255), Circuit tinyint, SessionMode tinyint, SelectedTeam tinyint, SelectedDriver tinyint, DriverName varchar(255), SessionCreatedAt datetime)");
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
            
            

            //Create the tables
            foreach (string cmd in TableCreationCommands)
            {
                SqlCommand sqlcmd_thiscmd = new SqlCommand(cmd, connection);
                sqlcmd_thiscmd.ExecuteNonQuery();
            }
            
            connection.Close();
        }

        #endregion

        #region "Full Session operations"

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

            //Upload all LocationPerformanceAnalysis
            foreach (LocationPerformanceAnalysis lpa in s.Corners)
            {
                //Upload it
                Guid g = await avm.uploadLocationp
            }

            return session_id;
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

        #endregion

        #region "Helper functions"

        private static SqlConnection GetSqlConnection(this ApexVisualManager avm)
        {
            SqlConnection con = new SqlConnection(avm.AzureSqlDbConnectionString);
            return con;
        }

        #endregion
    
        #region "Shallow Transactions (affecting a single table only, not meant to be used outside this)"

        public static async Task<ulong> UploadSessionAsync(this ApexVisualManager avm, Session to_upload)
        {
        
            List<KeyValuePair<string, string>> ColumnValuePairs = new List<KeyValuePair<string, string>>();

            ColumnValuePairs.Add(new KeyValuePair<string, string>("SessionId", "'" + to_upload.SessionId.ToString() + "'"));
            //Skip the "Owner" field right now - that is a lookup to the user, so that can be done later.
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
            ColumnValuePairs.Add(new KeyValuePair<string, string>("AverageSpeedKph", lpa.AverageSpeedKph.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("AverageGear", lpa.AverageGear.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("AverageDistanceToApex", lpa.AverageDistanceToApex.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("InconsistencyRating", lpa.InconsistencyRating.ToString()));

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

        #endregion
    }
}