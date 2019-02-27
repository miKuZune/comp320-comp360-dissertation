using System.Collections;
using System.Collections.Generic;
using System;
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

        while(reader.Read()){rowCount++;}
        
        reader.Close();
        List<int> ignoreIDs = new List<int>();

        
        // Stepwise regression
        for(int x = 0; x < 10; x++)
        {
            // Get and evaluate currently included independent variables. (Looks to remove)
            double[] weights = GetCoefficients(rowCount, columnCount, conn, ignoreIDs);

            int leastEffectiveCoefficientID = GetLowestCoefficientID(weights);

            if (weights[leastEffectiveCoefficientID] < 0.15f)
            {
                ignoreIDs.Add(leastEffectiveCoefficientID);
            }

            // Check non included variables to see if they can be re added.
            
            
        }



        
        

        conn.Close();
        reader.Close();
    }

    int GetLowestCoefficientID(double[] weights)
    {
        int leastEffectiveCoefficientID = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            double positiveCheck = Math.Sqrt(weights[i] * weights[i]);
            double currHighest = Math.Sqrt(weights[leastEffectiveCoefficientID] * weights[leastEffectiveCoefficientID]);

            if (positiveCheck < currHighest) { leastEffectiveCoefficientID = i; }
        }
        Debug.Log("lowest coefficient: " + weights[leastEffectiveCoefficientID] + " ID: " + leastEffectiveCoefficientID);
        return leastEffectiveCoefficientID;
    }

    double[] GetCoefficients(int rowCount, int columnCount, IDbConnection conn, List<int> ignoredIDs)
    {
        double[][] inputs = new double[rowCount][];
        double[] outputs = new double[rowCount];

        rowCount = 0;

        IDbCommand comm = conn.CreateCommand();
        comm.CommandText = "SELECT * FROM dataset";

        IDataReader reader = comm.ExecuteReader();
        
        while (reader.Read())
        {
            double[] newRow = new double[columnCount - 1 - ignoredIDs.Count];
            int currColumnID = 0;
            for (int i = 0; i < columnCount; i++)
            {
                if (!ignoredIDs.Contains(i))
                {
                    if (i < columnCount - 1)
                    {
                        newRow[currColumnID] = reader.GetInt32(i + 1);
                        currColumnID++;
                    }
                    else { outputs[rowCount] = reader.GetInt32(i + 1); }
                }
            }
            inputs[rowCount] = newRow;
            rowCount++;
        }

        var ols = new OrdinaryLeastSquares()
        {
            UseIntercept = true
        };

        MultipleLinearRegression regression = ols.Learn(inputs, outputs);

        return regression.Weights;
    }
}