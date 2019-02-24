using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Accord.Statistics.Models.Regression.Linear;
using System.Data;
using Mono.Data.Sqlite;


public class AccordTest {

    // Use this for initialization
    public void DO()
    {
        string connectionString = "URI=file:" + Application.dataPath + "/TestDBstepwise";
        IDbConnection conn = new SqliteConnection(connectionString);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM dataset";
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        int columnCount = reader.FieldCount - 1;
        int rowCount = 0;

        while(reader.Read())
        {
            rowCount++;
        }
        double[][] inputs = new double[rowCount][];
        double[] outputs = new double[rowCount];
        rowCount = 0;
        reader.Close();

        reader = comm.ExecuteReader();
        while(reader.Read())
        {
            double[] newRow = new double[columnCount - 1];
            for(int i = 0; i < columnCount; i++)
            {
                if (i < columnCount - 1) { newRow[i] = reader.GetInt32(i + 1);}
                else { outputs[rowCount] = reader.GetInt32(i + 1); Debug.Log("Output added " + reader.GetInt32(i + 1)); }   
            }
            inputs[rowCount] = newRow;
            rowCount++;
        }

        var ols = new OrdinaryLeastSquares()
        {
            UseIntercept = true
        };

        MultipleLinearRegression regression = ols.Learn(inputs, outputs );

        double[] coefficients = new double[regression.Weights.Length];
        for(int i = 0; i < coefficients.Length; i++)
        {
            coefficients[i] = regression.Weights[i];
            Debug.Log(coefficients[i]);
        }

        conn.Close();
        reader.Close();
    }
}
