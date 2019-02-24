using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;
using Accord.Statistics.Models.Regression.Linear;

public class LinearRegression
{
    private string connectionString;

    List<DataVariable> results = new List<DataVariable>();

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
            results.Add(new DataVariable());
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
                //results[i].values.Add(reader.GetInt32(i + 1));

                results[i].sumOfValuesSquared += reader.GetInt32(i + 1) * reader.GetInt32(i + 1);       //(ZxN^2)
                results[i].sumOfValues += reader.GetInt32(i + 1);
            }
            

            for(int i = 0; i < columnCount - 1; i++)
            {
                results[i].sumofValuesTimesY += reader.GetInt32(i + 1) * reader.GetInt32(columnCount);  //(ZxNy)
            }
            
            currRow++;
        }

        double[][] inputs = new double[results.Count - 1][];
        for(int i = 0; i < results.Count - 1; i++)
        {
            inputs[i] = new double[results[i].values.Length];
            for(int j = 0; j < results[i].values.Length; j++)
            {
                inputs[i][j] = results[i].values[j];
            }
        }

        double[] outputs = new double[results[results.Count - 1].values.Length];
        for(int i = 0; i < results[results.Count - 1].values.Length; i++)
        {
            outputs[i] = results[results.Count - 1].values[i];
        }

        var ols = new OrdinaryLeastSquares()
        {
            UseIntercept = true
        };

        MultipleLinearRegression regression = ols.Learn(inputs, outputs);

        /*for(int i = 0; i < results.Count - 1; i++)
        {
            results[i].coefficient = regression.Weights[i];
            Debug.Log(results[i].coefficient);
        }*/

        conn.Close();
        reader.Close();
    }


    double GetStandardDeviation(double mean, List<int> values)
    {
        double sigma = 0;
        for(int i = 0; i < values.Count; i++)
        {
            sigma += (values[i] - mean) * (values[i] - mean);
        }

        double variance = sigma / values.Count;
        double SD = Math.Sqrt(variance);

        return SD;
    }

    float GetCoefficient(float mean, float SD)
    {
        
        return -mean / SD;
    }

}

class DataVariable
{
    public string label;

    public double[] values;

    public int Count;
    public double Mean;
    public double StandardDeviation;

    public double sumOfValuesSquared;
    public double sumofValuesTimesY;
    public double sumOfValues;

    public double coefficient;
    
}
