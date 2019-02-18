using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class LinearRegression
{
    private string connectionString;

    List<DataResults> results = new List<DataResults>();

    public void Read()
    {
        connectionString = "URI=file:" + Application.dataPath + "/TestDBStepwise";

        IDbConnection conn = new SqliteConnection(connectionString);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM dataset";
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        for(int i = 0; i < 6; i++)
        {
            results.Add(new DataResults());
            results[i].label = "Test" + (i + 1);
            if (results[i].label == "Test6") { results[i].label = "IQ"; }
        }

        int columnCount = reader.FieldCount - 1;
        int currRow = 0;
        while(reader.Read())
        {
            for (int i = 0; i < columnCount; i++)
            {
                results[i].Count = results[i].Count + 1;
                results[i].Mean += reader.GetInt32(i + 1);
                results[i].values.Add(reader.GetInt32(i + 1));
            }
            
            currRow++;
        }


        for(int i = 0; i < results.Count; i++)
        {
            results[i].Mean = results[i].Mean / currRow;
            
            results[i].StandardDeviation = GetStandardDeviation(results[i].Mean, results[i].values);
            Debug.Log(GetCoefficient(results[i].Mean, results[i].StandardDeviation)+ " " + results[i].label + " coefficient");
        }

       /* for(int i = 0; i < results.Count; i++)
        {
            Debug.Log(results[i].Mean + " " + results[i].Count);
        }*/

        conn.Close();
        reader.Close();
    }


    float GetStandardDeviation(float mean, List<int> values)
    {
        float sigma = 0;
        for(int i = 0; i < values.Count; i++)
        {
            sigma += (values[i] - mean) * (values[i] - mean);
        }

        float variance = sigma / values.Count;
        float SD = (float)Math.Sqrt(variance);

        return SD;
    }

    float GetCoefficient(float mean, float SD)
    {
        
        return -mean / SD;
    }

}

class DataResults
{
    public string label;

    public List<int> values = new List<int>();

    public int Count;
    public float Mean;
    public float StandardDeviation;

    public float Coefficient;
    
}
