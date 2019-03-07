using System.Collections;
using System.Collections.Generic;
using Accord.Statistics.Models.Regression.Linear;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

public class StepwiseRegression {

    double[][] inputs;
    double[] outputs;

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
        int rowCount = 0;

        while (reader.Read()) { rowCount++; }   // Count the rows by going through all lines in the DB.
        reader.Close();

        UnityTime uTime = new UnityTime();

        double[][] inputs = new double[rowCount][];
        double[] outputs = new double[columnCount];

        reader = comm.ExecuteReader();
        int currRowID = 0;
        while(reader.Read())
        {
            double[] row = new double[columnCount];
            for(int i = 0; i < columnCount; i++)
            {
                Debug.Log(reader.GetName(i + 2));
                switch (reader.GetName(i + 2))
                {
                    case "KillWeapon":
                        row[i] = ConvertToGunID(reader.GetString(i + 2));
                        break;
                    case "TimeStamp":
                        row[i] = uTime.ConvertToSeconds(reader.GetString(i + 2));
                        break;
                    default:
                        row[i] = reader.GetDouble(i + 2);
                        break;
                }                
            }
            inputs[currRowID] = row;
            currRowID++;
        }

        conn.Close();
        reader.Close();


        for(int i = 0; i < inputs.Length; i++)
        {
            string output = "";
            for(int j = 0; j < inputs[i].Length; j++)
            {
                output += inputs[i][j] + " ";
            }
            Debug.Log(i + ": " + output);
        }
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
