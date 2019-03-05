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

        int columnCount = reader.FieldCount - 1;

        while(reader.Read())
        {
            for(int i = 0; i < columnCount; i++)
            {
                Debug.Log(reader.GetValue(i + 1) + " " + reader.GetName(i + 1) + " " + reader.GetFieldType(i + 1)); 
            }
        }

        conn.Close();
        reader.Close();

    }

	public void GetModel()
    {

    }
}
