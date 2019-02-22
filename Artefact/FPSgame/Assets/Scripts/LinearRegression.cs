using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

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
                results[i].values.Add(reader.GetInt32(i + 1));

                results[i].sumOfValuesSquared += reader.GetInt32(i + 1) * reader.GetInt32(i + 1);       //(ZxN^2)
                results[i].sumOfValues += reader.GetInt32(i + 1);
            }
            

            for(int i = 0; i < columnCount - 1; i++)
            {
                results[i].sumofValuesTimesY += reader.GetInt32(i + 1) * reader.GetInt32(columnCount);  //(ZxNy)
            }
            
            currRow++;
        }

        double ultiTemp = 0;
        Debug.Log("CALCULATING COEFFICIENT");
        for(int i = 0; i < results.Count; i++)
        {
            results[i].Mean = results[i].Mean / currRow;
            
            results[i].StandardDeviation = GetStandardDeviation(results[i].Mean, results[i].values);


            // Don't need to do the rest for the dependent variable
            if (i == results.Count - 1) { return; }
            // Formula for coefficient. Z = sigma/sum of
            // mn = (Z(x1^2*x2^2...xn^2))(ZxNy) - (Zx1x2...xN)(Zx1y * x2y ... xNy)
            // -------------------------------------------------------------------
            //          (Zx1^2*x2^2...xN^2) - (Zx1x2...xn)^2

            // Get Z(x1^2 * x2^2 ... xn^2)
            double sumOfSquaresExcludingCurrent = 0;
            double sumOfAllSquares = 0;                 
            for(int j = 0; j < results.Count - 1; j++)
            {
                if(j != i)
                {
                    sumOfSquaresExcludingCurrent += results[i].sumOfValuesSquared;
                }
                sumOfAllSquares += results[i].sumOfValuesSquared;
            }

            // Get (ZxNy)
            double sumOfXY = 0;
            for(int j = 0; j < results[i].values.Count; j++)
            {
                sumOfXY += (results[i].values[j] * results[results.Count - 1].values[j]);
            }

            double temporary = 0;               // (z(x1^2)(ZxNy) + ... Z(xn^2)(ZxNy) )
            for(int j = 0; j < results.Count - 1; j++)
            {
                temporary += results[j].sumOfValuesSquared * sumOfXY;
            }

            // Get (Zx1x2...xn)
            double sumOfXs = 0;
            for(int j = 0; j < results[i].values.Count; j++)
            {
                double temp = 0;
                for(int x = 0; x < results.Count - 1; x++)
                {
                    temp += results[x].values[j];
                }
                sumOfXs += temp;
            }

            double temp2 = 0;
            for(int j = 0; j < results.Count - 1; j++)
            {
                double innerTemp = 0;
                double innerTemp2 = 0;
                for(int x = 0; x < results[j].values.Count; x++)
                {
                    innerTemp += results[i].values[x] * results[j].values[x];
                    innerTemp2 += results[j].values[x] * results[results.Count - 1].values[x];
                }
                temp2 += innerTemp * innerTemp2;
            }

            // Get (Z(x1y x2y ... xNy))
            double sumOfXYs = 0;
            for(int j = 0; j < results[i].values.Count; j++)
            {
                double temp = 0;
                for(int x = 0; x < results.Count - 1; x++)
                {
                    temp += results[x].values[j] * results[results.Count - 1].values[j];
                }
                sumOfXYs += temp;
            }

            double temp3 = 0;
            for(int j = 0; j < results.Count - 1; j++)
            {
                double innerTemp = 0;
                for(int x = 0; x < results[j].values.Count; x++)
                {
                    innerTemp += results[i].values[x] * results[j].values[x];
                }
                temp3 += innerTemp * innerTemp;
            }

            double sumOfXsSquared = sumOfXs * sumOfXs;

            //results[i].coefficient = (sumOfSquaresExcludingCurrent * sumOfXY) - (sumOfXs * sumOfXYs);
            results[i].coefficient = temporary - temp2;
            results[i].coefficient /= (sumOfAllSquares - sumOfXsSquared);
            Debug.Log(sumOfAllSquares + " " + sumOfXsSquared);
            //results[i].coefficient = Math.Sqrt(results[i].coefficient);
            ultiTemp += results[i].coefficient;
           // Debug.Log("test: " + temporary);
           // Debug.Log("curr: " + (sumOfSquaresExcludingCurrent * sumOfXY));
           // Debug.Log("diff: " + (temporary - (sumOfSquaresExcludingCurrent * sumOfXY)));
            //Debug.Log(sumOfAllSquares + " " + sumOfXsSquared + " " + (sumOfAllSquares - sumOfXsSquared));
            //Debug.Log(results[i].coefficient + " before");
            //results[i].coefficient /= (sumOfAllSquares - (sumOfXsSquared));
            
            Debug.Log(results[i].coefficient + " " + results[i].label + " after");

            Debug.Log("end of iteration");
        }
        Debug.Log("DONE CALCULATING COEFFICIENT");

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

    public List<int> values = new List<int>();

    public int Count;
    public double Mean;
    public double StandardDeviation;

    public double sumOfValuesSquared;
    public double sumofValuesTimesY;
    public double sumOfValues;

    public double coefficient;
    
}
