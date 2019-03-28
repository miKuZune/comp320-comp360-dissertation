using System.Collections;
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

    /*double[][] GetWeaponPreferences(double[][] sessionData)
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
    }*/

    double[][] GetWeaponPreferences(double[][] sessionData)
    {
        double[][] outputs = new double[3][];
        for (int i = 0; i < outputs.Length; i++) { outputs[i] = new double[sessionData.Length]; }

        for(int i = 0; i < sessionData.Length; i++)
        {
            double totalTimer = sessionData[i][9] + sessionData[i][13] + sessionData[i][17];

            outputs[0][i] = ((sessionData[i][6] + sessionData[i][7] + sessionData[i][8])/sessionData[i][4]) * sessionData[i][9];
            outputs[1][i] = ((sessionData[i][10] + sessionData[i][11] + sessionData[i][12]) / sessionData[i][4]) * sessionData[i][13]; ;
            outputs[2][i] = ((sessionData[i][14] + sessionData[i][15] + sessionData[i][16]) / sessionData[i][4]) * sessionData[i][17]; ;
        }

        return outputs;
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
        double[][] weaponPref = GetWeaponPreferences(inputs_Session);

        for(int i = 0; i < inputs_Session.Length; i++)
        {
            Debug.Log("Session " + i);
            string outty = "AR pref: " + weaponPref[0][i] + " Shot pref: " + weaponPref[1][i] + " Sniper pref: " + weaponPref[2][i];
            Debug.Log(outty);
            outty = "";
            for(int j = 0; j < inputs_Session.Length; j++)
            {
                outty += inputs_Session[i][j] + ", ";
            }
            Debug.Log(outty);
        }

        /*var testRegression = new StepwiseLogisticRegressionAnalysis(inputs, outputs[0]);

        testRegression.Learn(inputs, outputs[0]);
        
        StepwiseLogisticRegressionModel best = testRegression.Current;

        Debug.Log(best.ChiSquare + " chi");*/

    }
}
