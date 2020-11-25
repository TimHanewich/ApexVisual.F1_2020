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
                TableCreationCommands.Add("create table Track (Id tinyint, CountryCode varchar(2), Latitude real, Longitude real)");
            }

            #endregion

            #endregion

            #region "Sessions"
   
            //Session
            if (TablesThatAlreadyExist.Contains("Session") == false)
            {
                TableCreationCommands.Add("create table Session (SessionId varchar(30), SessionMode tinyint, SelectedTeam tinyint, SelectedDriver tinyint, DriverName varchar(255), SessionSummaryCreatedAt datetime)");
            }

            //Lap
            if (TablesThatAlreadyExist.Contains("Lap") == false)
            {
                TableCreationCommands.Add("create table Lap (Id uniqueidentifier, SessionId varchar(30), LapNumber tinyint, Sector1Time real, Sector2Time real, Sector3Time real, LapTime real, FuelConsumed real, PercentOnThrottle real, PercentOnBrake real, PercentCoasting real, PercentThrottleBrakeOverlap real, PercentOnMaxThrottle real, PercentOnMaxBrake real, ErsDeployed real, ErsHarvested real, GearChanges int, TopSpeedKph smallint, EquippedTyreCompount tinyint, IncrementalTyreWear uniqueidentifier, BeginningTyreWear uniqueidentifier)");
            }

            //TelemetrySnapshot
            if (TablesThatAlreadyExist.Contains("TelemetrySnapshot") == false)
            {
                TableCreationCommands.Add("create table TelemetrySnapshot (Id uniqueidentifier, LapAnalysisId uniqueidentifier, LocationType tinyint, LocationNumber tinyint, PositionX real, PositionY real, PositionZ real, VelocityX real, VelocityY real, VelocityZ real, gForceLateral real, gForceLongitudinal real, gForceVertical real, Yaw real, Pitch real, Roll real, CurrentLapTime real, CarPosition tinyint, LapInvalid bit, Penalties tinyint, SpeedKph int, Throttle real, Steer real, Brake real, Clutch real, Gear smallint, EngineRpm int, DrsActive bit, BrakeTemperature uniqueidentifier, TyreSurfaceTemperature uniqueidentifier, TyreInnerTemperature uniqueidentifier, EngineTemperature int, SelectedFuelMix tinyint, FuelLevel real, TyreWearPercentage uniqueidentifier, TyreDamagePercent uniqueidentifier, FrontLeftWingDamage real, FrontRightWingDamage real, RearWingDamage real, ErsStored real)");
            }
               
            //WheelDataArray
            if (TablesThatAlreadyExist.Contains("WheelDataArray") == false)
            {
                TableCreationCommands.Add("create table WheelDataArray (Id uniqueidentifier, RearLeft real, RearRight real, FrontLeft real, FrontRight real)");
            }

            //LocationPerformanceAnalysis
            if (TablesThatAlreadyExist.Contains("LocationPerformanceAnalysis") == false)
            {
                TableCreationCommands.Add("create table LocationPerformanceAnalysis (Id uniqueidentifier, SessionId varchar(30), LocationType tinyint, LocationNumber tinyint, AverageSpeedKph real, AverageGear real, AverageDistanceToApex real, CornerConsistencyRating real)");
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

        #region "Session operations"

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

        public static async Task UploadSessionAsync(this ApexVisualManager avm, Session to_upload)
        {

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
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PositionX", snapshot.PositionY.ToString()));
            ColumnValuePairs.Add(new KeyValuePair<string, string>("PositionX", snapshot.PositionZ.ToString()));

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

            string cmd = "insert into TelemetrySnapshot (" + Component_Columns + ") + values (" + Component_Values + ")";

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

        #endregion
    }
}