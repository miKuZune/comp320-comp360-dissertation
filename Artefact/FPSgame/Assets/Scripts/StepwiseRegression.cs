using System.Collections;
using System.Collections.Generic;
using Accord.Statistics.Models.Regression.Linear;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

public class StepwiseRegression {

    public void GetData()
    {
        string connectionString = "URI=file:" + Application.dataPath + "/DB";
        IDbConnection conn = new SqliteConnection(connectionString);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM Events";
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        // Get the number of rows and columns necessary to setup the input and output arrays.
        int columnCount = reader.FieldCount - 2;            // Column count not including the Primary Key and the Session ID.
        int sessionCount = 0;

        int lastSessionID = 0;
        while (reader.Read())
        {
            if(lastSessionID != reader.GetInt32(1))
            {
                sessionCount++;
                lastSessionID = reader.GetInt32(1);
            }
            
        }   // Count the number of sessions by going through all lines in the DB.
        reader.Close();



        double[][] eventsData = GetMeanEventsData(reader, comm, columnCount, sessionCount);
        double[][] sessionData = GetSessionData(comm, reader, sessionCount);

        double[][] outputs = GetWeaponPreferences(sessionData);

             

        conn.Close();
        reader.Close();


        for(int i = 0; i < eventsData.Length; i++)
        {
            string output = "";
            for(int j = 0; j < eventsData[i].Length; j++)
            {
                output += eventsData[i][j] + " ";
            }
            Debug.Log(i + ": " + output);
        }
    }

    double[][] GetWeaponPreferences(double[][] sessionData)
    {
        double[][] outputs = new double[sessionData.Length][];
        for(int i = 0; i < sessionData.Length; i++)
        {
            double[] row = new double[3];   // 0 = AR pref; 1 = S pref; 2 = SR pref

            if(sessionData[i] != null)
            {
                double totalTime = sessionData[i][9] + sessionData[i][13] + sessionData[i][17];
                Debug.Log("time " + totalTime);

                row[0] = (sessionData[i][6] / sessionData[i][0]) + (sessionData[i][7] / sessionData[i][1]) + (sessionData[i][8] / sessionData[i][2]) * (sessionData[i][9] / totalTime);
                row[1] = (sessionData[i][10] / sessionData[i][0]) + (sessionData[i][11] / sessionData[i][1]) + (sessionData[i][12] / sessionData[i][2]) * (sessionData[i][13] / totalTime);
                row[2] = (sessionData[i][14] / sessionData[i][0]) + (sessionData[i][15] / sessionData[i][1]) + (sessionData[i][16] / sessionData[i][2]) * (sessionData[i][17] / totalTime);

                Debug.Log(row[0] + " " + row[1] + " " + row[2]);
            }
            outputs[i] = row;
        }
        return outputs;
    }

    double[][] GetSessionData(IDbCommand comm, IDataReader reader, int sessionCount)
    {
        double[][] sessionData = new double[sessionCount][];

        comm.CommandText = "SELECT * FROM SessionData";
        reader = comm.ExecuteReader();

        int columnCount = reader.FieldCount - 1;
        int currRowID = 0;
        while(reader.Read())
        {
            double[] row = new double[columnCount];
            for(int i = 0; i < columnCount; i++ )
            {
                double num = reader.GetDouble(i + 1);
                if (num == 0) { num = 1; }
                row[i] = num;
            }
            sessionData[currRowID] = row;
            currRowID++;
        }
        reader.Close();

        return sessionData;
    }

    double[][] GetMeanEventsData(IDataReader reader, IDbCommand comm, int columnCount, int sessionCount)
    {

        double[][] eventsData = new double[sessionCount][];

        UnityTime uTime = new UnityTime();

        reader = comm.ExecuteReader();
        int currRowID = 0;
        double[] row = new double[columnCount];
        while (reader.Read())
        {
            if (reader.GetInt32(1) != currRowID + 1)
            {
                for (int i = 0; i < row.Length; i++) { row[i] /= row.Length; }      // Get the average of the data for the session.

                eventsData[currRowID] = row;
                row = new double[columnCount];

                currRowID++;
            }

            for (int i = 0; i < columnCount; i++)
            {
                switch (reader.GetName(i + 2))
                {
                    case "KillWeapon":
                        row[i] += ConvertToGunID(reader.GetString(i + 2));
                        break;
                    case "TimeStamp":
                        row[i] += uTime.ConvertToSeconds(reader.GetString(i + 2));
                        break;
                    default:
                        row[i] += reader.GetDouble(i + 2);
                        break;
                }
            }
        }
        // Add the final row in after the loop has finished.
        for (int i = 0; i < row.Length; i++) { row[i] /= row.Length; }
        eventsData[eventsData.Length - 1] = row;

        reader.Close();

        return eventsData;
    }

    int ConvertToGunID(string gunName)
    {
        int ID = 0;
        switch (gunName)
        {
            case "Shotgun":
                ID = 2;
                break;
            case "Sniper Rifle":
                ID = 3;
                break;
            case "Assult Rifle":
                ID = 1;
                break;
        }
        return ID;
    }

	public void GetModel()
    {

    }
}
