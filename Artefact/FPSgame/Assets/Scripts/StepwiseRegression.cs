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

    const string DB_name = "/DB_Official.db";

    double[][] GetWeaponPreferences(double[][] sessionData)
    {
        double[][] outputs = new double[3][];
        for (int i = 0; i < outputs.Length; i++) { outputs[i] = new double[sessionData.Length]; }

        for(int i = 0; i < sessionData.Length; i++)
        {
            double totalTimer = sessionData[i][9] + sessionData[i][13] + sessionData[i][17];

            outputs[0][i] = ((sessionData[i][6] + sessionData[i][7] + sessionData[i][8])/sessionData[i][4]) * (sessionData[i][9] / 3);
            outputs[1][i] = ((sessionData[i][10] + sessionData[i][11] + sessionData[i][12]) / sessionData[i][4]) * (sessionData[i][13] / 3);
            outputs[2][i] = ((sessionData[i][14] + sessionData[i][15] + sessionData[i][16]) / sessionData[i][4]) * (sessionData[i][17] / 3);

            // Map the pref scores between 0 and 1.
            double totalPref = outputs[0][i] + outputs[1][i] + outputs[2][i];

            outputs[0][i] = outputs[0][i] / totalPref;
            outputs[1][i] = outputs[1][i] / totalPref;
            outputs[2][i] = outputs[2][i] / totalPref;
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

        string connectionString = "URI=file:" + Application.dataPath + DB_name;
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

        string connectionString = "URI=file:" + Application.dataPath + DB_name;
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
        string connectionString = "URI=file:" + Application.dataPath + DB_name;
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

        // Data to get to be sorted to get independent and dependent variables.
        double[][] inputs_Events = GetEventData(sessionCount, columnCount, eventCount);     // independent 
        double[][] inputs_Session = GetSessionData(sessionCount);                           // independent
        double[][] weaponPrefs = GetWeaponPreferences(inputs_Session);                      // dependent

        // Stores the dependent variables for each of the different weapons.
        double[] AR_pref = new double[inputs_Session.Length];
        double[] shotgun_pref = new double[inputs_Session.Length];
        double[] sniper_pref = new double[inputs_Session.Length];

        // Store the combined independent variables.
        double[][] inputs = new double[sessionCount][];

        for(int i = 0; i < inputs.Length; i++)
        {
            double[] newRow = new double[inputs_Events[i].Length + inputs_Session[i].Length];
            // Add the events data.
            for(int j = 0; j < inputs_Events[i].Length - 1; j++)
            {
                newRow[j] = inputs_Events[i][j];
            }
            // Add the session data.
            for(int j = 0; j < inputs_Session[i].Length; j++)
            {
                newRow[j + inputs_Events[i].Length - 1] = inputs_Session[i][j];
            }
            inputs[i] = newRow;
        }

        // Read weapon prefs into individual arrays.
        for(int i = 0; i < inputs_Session.Length; i++)
        {
            AR_pref[i] = weaponPrefs[0][i];
            shotgun_pref[i] = weaponPrefs[1][i];
            sniper_pref[i] = weaponPrefs[2][i];
        }

        // AR model
        var AR_regression = new StepwiseLogisticRegressionAnalysis(inputs, AR_pref);

        AR_regression.Learn(inputs, AR_pref);
        
        StepwiseLogisticRegressionModel AR_bestModel = AR_regression.Current;

        Debug.Log(AR_bestModel.ChiSquare + " chi for AR");

        // Shotgun model
        var S_regression = new StepwiseLogisticRegressionAnalysis(inputs, shotgun_pref);

        S_regression.Learn(inputs, shotgun_pref);

        StepwiseLogisticRegressionModel S_bestModel = S_regression.Current;

        Debug.Log(S_bestModel.ChiSquare + " chi for Shotgun");

        // Sniper model
        var SR_regression = new StepwiseLogisticRegressionAnalysis(inputs, shotgun_pref);

        SR_regression.Learn(inputs, sniper_pref);

        StepwiseLogisticRegressionModel SR_bestModel = SR_regression.Current;

        Debug.Log(SR_bestModel.ChiSquare + " chi for Sniper");

    }
}
