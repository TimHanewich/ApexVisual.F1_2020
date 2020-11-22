using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApexVisual.F1_2020.SqlCloudStorage
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
            
            //SessionSummary
            if (TablesThatAlreadyExist.Contains("SessionSummary") == false)
            {
                string cmd_sessionsummary = "create table SessionSummary (SessionId varchar(30), Circuit tinyint, SelectedTeam tinyint, DriverName varchar(255), SessionSummaryCreatedAt datetime)";
                TableCreationCommands.Add(cmd_sessionsummary);
            }
            
            //SessionAnalysis
            if (TablesThatAlreadyExist.Contains("SessionAnalysis") == false)
            {
                string cmd_sessionanalysis = "create table SessionAnalysis (SessionId varchar(30), SessionAnalysisGeneratedAt datetime)";
                TableCreationCommands.Add(cmd_sessionanalysis);
            }
            
            //LapAnalysis
            if (TablesThatAlreadyExist.Contains("LapAnalysis") == false)
            {
                TableCreationCommands.Add("create table LapAnalysis (Id uniqueidentifier, SessionId varchar(30), LapNumber tinyint, Sector1Time real, Sector2Time real, LapTime real, FuelConsumed real, PercentOnThrottle real, PercentOnBrake real, PercentCoasting real, PercentThrottleBrakeOverlap real, PercentOnMaxThrottle real, PercentOnMaxBrake real, ErsDeployed real, ErsHarvested real, GearChanges int, TopSpeedKph smallint, EquippedTyreCompound tinyint, IncrementalTyreWear uniqueidentifier, BeginningTyreWear uniqueidentifier)");
            }

            //CornerPerformanceAnalysis
            if (TablesThatAlreadyExist.Contains("CornerPerformanceAnalysis") == false)
            {
                TableCreationCommands.Add("create table CornerPerformanceAnalysis (Id uniqueidentifier, SessionId varchar(30), AverageSpeed real, AverageGear real, AverageDistanceToApex real, CornerConsistencyRating real, CornerNumber tinyint)");
            }

            //CornerAnalysis
            if (TablesThatAlreadyExist.Contains("CornerAnalysis") == false)
            {
                TableCreationCommands.Add("create table CornerAnalysis (Id uniqueidentifier, LapAnalysisId uniqueidentifier, CornerData uniqueidentifier, Motion uniqueidentifier, Lap uniqueidentifier, Telemetry uniqueidentifier, CarStatus uniqueidentifier)");
            }
            
            //WheelDataArray
            if (TablesThatAlreadyExist.Contains("WheelDataArray") == false)
            {
                
            }

            //CarMotionData
            if (TablesThatAlreadyExist.Contains("CarMotionData") == false)
            {
                
            }

            //LapData
            if (TablesThatAlreadyExist.Contains("LapData") == false)
            {
                
            }

            //CarTelemetryData
            if (TablesThatAlreadyExist.Contains("CarTelemetryData") == false)
            {
                
            }

            //CarStatusData
            if (TablesThatAlreadyExist.Contains("CarStatusData") == false)
            {
                
            }
            

            //Create the tables
            foreach (string cmd in TableCreationCommands)
            {
                SqlCommand sqlcmd_thiscmd = new SqlCommand(cmd, connection);
                sqlcmd_thiscmd.ExecuteNonQuery();
            }
            
            connection.Close();
        }

        #endregion

        #region "Helper functions"

        private static SqlConnection GetSqlConnection(this ApexVisualManager avm)
        {
            SqlConnection con = new SqlConnection(avm.AzureSqlDbConnectionString);
            return con;
        }

        #endregion
    }
}