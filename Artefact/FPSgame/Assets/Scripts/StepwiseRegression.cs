﻿using System.Collections;
using System.Collections.Generic;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Statistics.Analysis;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

using Accord.Statistics;
using Accord.Statistics.Distributions.Univariate;

using System;

public class StepwiseRegression {

    //double[][] inputs;          // Holds the collection of the event data and the session data.
    //double[][] outputs;         // Holds the collection of weapon preferences for each session.

    /*public void GetData()
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

        outputs = GetWeaponPreferences(sessionData);

        // Bring the data together in one data structure.
        inputs = new double[eventsData.Length][];
        int outputColumnCount = eventsData[0].Length + sessionData[0].Length;
        for(int i = 0; i < inputs.Length; i++)
        {
            double[] row = new double[outputColumnCount];
            if (eventsData[i] != null)
            {
                for (int j = 0; j < eventsData[i].Length; j++)
                {
                    row[j] = eventsData[i][j];
                }
            }
            else { for (int j = 0; j < eventsData[i].Length; j++) { row[j] = 0; } }

            if (sessionData[i] != null)
            {
                for (int j = 0; j < sessionData[i].Length; j++)
                {
                    row[j + eventsData[i].Length] = sessionData[i][j];
                }
            }
            else{for (int j = eventsData[i].Length; j < row.Length; j++){row[j] = 0;}}

            inputs[i] = row;
        }

        conn.Close();
        reader.Close();
    }*/

    double[][] GetWeaponPreferences(double[][] sessionData)
    {
        double[][] output = new double[3][];

        for (int i = 0; i < output.Length; i++) { output[i] = new double[sessionData.Length]; }

        for(int i = 0; i < sessionData.Length; i++)
        {
            if(sessionData[i] != null)
            {
                double totalTime = sessionData[i][9] + sessionData[i][13] + sessionData[i][17];

                output[0][i] = (sessionData[i][6] / sessionData[i][0]) + (sessionData[i][7] / sessionData[i][1]) + (sessionData[i][8] / sessionData[i][2]) * (sessionData[i][9] / totalTime);
                output[1][i] = (sessionData[i][10] / sessionData[i][0]) + (sessionData[i][11] / sessionData[i][1]) + (sessionData[i][12] / sessionData[i][2]) * (sessionData[i][13] / totalTime);
                output[2][i] = (sessionData[i][14] / sessionData[i][0]) + (sessionData[i][15] / sessionData[i][1]) + (sessionData[i][16] / sessionData[i][2]) * (sessionData[i][17] / totalTime);
            }
            else
            {
                Debug.Log("Zeroed");
                output[0][i] = 0;
                output[1][i] = 0;
                output[2][i] = 0;
            }
        }
        return output;
    }

    /*double[][] GetSessionData(IDbCommand comm, IDataReader reader, int sessionCount)
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
    }*/

    /*double[][] GetMeanEventsData(IDataReader reader, IDbCommand comm, int columnCount, int sessionCount)
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
    }*/

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

    double[][] GetEventData(int sessionCount, int columnCount, int eventCount)
    {
        double[][] inputs = null;

        string connectionString = "URI=file:" + Application.dataPath + "/DB";
        IDbConnection conn = new SqliteConnection(connectionString);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM Events";
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        
        inputs = new double[sessionCount][];

        int iter = 0;
        UnityTime UT = new UnityTime();


        List<int> sessionIDs = new List<int>();
        double[] newRow = null;
        int rowCounter = 0;
        while (reader.Read())
        {
            if (!sessionIDs.Contains(reader.GetInt32(1)))
            {
                sessionIDs.Add(reader.GetInt32(1));

                if(newRow != null)
                {
                    for(int i = 0; i < newRow.Length;i++)
                    {
                        newRow[i] = newRow[i] / rowCounter;
                    }

                    

                    inputs[iter] = newRow;
                    iter++;
                }
                rowCounter = 0;

                newRow = new double[columnCount];
            }

            for (int i = 0; i < columnCount; i++)
            {
                
                switch(reader.GetName(i + 2))
                {
                    case "KillWeapon":
                        newRow[i] += ConvertToGunID(reader.GetString(i + 2));
                        break;
                    case "TimeStamp":
                        newRow[i] += UT.ConvertToSeconds(reader.GetString(i + 2));
                        break;
                    default:
                        newRow[i] += reader.GetDouble(i + 2);
                        break;
                }
            }
            rowCounter++;
        }
        inputs[iter] = newRow;
        reader.Close();

        Debug.Log("Column count: " + columnCount);
        Debug.Log("Session Count: " + sessionCount);
        Debug.Log("Event Count: " + eventCount);


        for(int i = 0; i < inputs.Length; i++)
        {
            string output = "Row " + i + " :";
            for(int j = 0; j < inputs[i].Length; j++)
            {
                output += inputs[i][j] + " ";
            }
            Debug.Log(output);
        }

        return inputs;
    }

    double[][] GetSessionData(int sessionCount)
    {
        double[][] inputs = new double[sessionCount][];

        string connectionString = "URI=file:" + Application.dataPath + "/DB";
        IDbConnection conn = new SqliteConnection(connectionString);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM SessionData";
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        int columnCount = reader.FieldCount - 1;
        int iter = 0;
        while(reader.Read())
        {
            double[] newRow = new double[columnCount];
            for(int i = 0; i < columnCount; i++)
            {
                newRow[i] = reader.GetDouble(i + 1);
            }
            inputs[iter] = newRow;
            iter++;
        }


        for(int i = 0; i < inputs.Length; i++)
        {
            string str = "Session " + i + ": ";
            for(int j = 0; j < inputs[i].Length; j++)
            {
                str += inputs[i][j] + " ";
            }
            Debug.Log(str);
        }


        return inputs;
    }

	public void GetModel()
    {
        // Get some base data
        string connectionString = "URI=file:" + Application.dataPath + "/DB";
        IDbConnection conn = new SqliteConnection(connectionString);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM Events";
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        int columnCount = reader.FieldCount - 2;
        int sessionCount = 0;
        int eventCount = 0;

        List<int> sessionIDs = new List<int>();
        while (reader.Read())
        {
            eventCount++;
            if (!sessionIDs.Contains(reader.GetInt16(1)))
            {
                sessionIDs.Add(reader.GetInt16(1));
                sessionCount++;
            }
        }

        reader.Close();

        //GetData();
        double[][] inputs_Events = GetEventData(sessionCount, columnCount, eventCount);
        double[][] inputs_Session = GetSessionData(sessionCount);
        double[] outputs_AR;

        /*var testRegression = new StepwiseLogisticRegressionAnalysis(inputs, outputs[0]);

        testRegression.Learn(inputs, outputs[0]);
        
        StepwiseLogisticRegressionModel best = testRegression.Current;

        Debug.Log(best.ChiSquare + " chi");*/

    }
}
