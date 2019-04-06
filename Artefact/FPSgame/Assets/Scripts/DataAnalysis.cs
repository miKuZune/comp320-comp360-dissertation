using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

public class DataAnalysis : MonoBehaviour
{
    double[][] noML_eventData;
    double[][] noML_sessionData;

    double[][] ML_eventData;
    double[][] ML_sessionData;

    string connPath;

    public void Analyse()
    {
        connPath = "URI=file:" + Application.dataPath + "/DB_Official.DB";
        // Get the data from when there was no machine learning present
        string[][] data = GetData(connPath, "NoMLEventData");
        noML_eventData = OrganiseEventData(data);

        data = GetData(connPath, "NoMLSessionData");
        noML_sessionData = OrganiseSessionData(data);

        // Get the data from when machine learning was present.
        data = GetData(connPath, "MLEventData");
        ML_eventData = OrganiseEventData(data);

        data = GetData(connPath, "MLSessionData");
        ML_sessionData = OrganiseSessionData(data);

        double[][] meanNoMLEventDataOfEachSession = new double[noML_sessionData.Length][];
        double[][] meanMLEventDataOfEachSession = new double[ML_sessionData.Length][];
        int iter = 0;

        List<double[]> currSessionsEventData = new List<double[]>();

        double previousSessionID = 0;
        for (int i = 0; i < noML_eventData.Length; i++)
        {
            if (noML_eventData[i][0] != previousSessionID)
            {
                previousSessionID = noML_eventData[i][0];

                if (currSessionsEventData.Count > 0)
                {
                    meanNoMLEventDataOfEachSession[iter] = GetMeanOfData(currSessionsEventData);
                    currSessionsEventData.Clear();
                    iter++;
                }
            }
            currSessionsEventData.Add(noML_eventData[i]);
        }
        meanNoMLEventDataOfEachSession[iter] = GetMeanOfData(currSessionsEventData);
        currSessionsEventData.Clear();

        previousSessionID = 0;
        iter = 0;
        for (int i = 0; i < ML_eventData.Length; i++)
        {
            if (ML_eventData[i][0] != previousSessionID)
            {
                previousSessionID = ML_eventData[i][0];
                if (currSessionsEventData.Count > 0)
                {
                    meanMLEventDataOfEachSession[iter] = GetMeanOfData(currSessionsEventData);
                    currSessionsEventData.Clear();
                    iter++;
                }
            }
            currSessionsEventData.Add(ML_eventData[i]);
        }
        meanMLEventDataOfEachSession[iter] = GetMeanOfData(currSessionsEventData);

        List<double> experienceDifferenceScores = new List<double>();

        for (int i = 0; i < meanNoMLEventDataOfEachSession.Length; i++)
        {
            Debug.Log( "session " + i + ": " + GetDifferenceScore(meanMLEventDataOfEachSession[i], meanNoMLEventDataOfEachSession[i], ML_sessionData[i], noML_sessionData[i]));
            //experienceDifferenceScores.Add(GetDifferenceScore(meanMLEventDataOfEachSession[i], meanNoMLEventDataOfEachSession[i], ML_sessionData[i], noML_sessionData[i]));
        }

        for (int i = 0; i < experienceDifferenceScores.Count; i++)
        {
            Debug.Log("Score " + i + ": " + experienceDifferenceScores[i]);
        }


    }

    double GetDifferenceScore(double[] meanMLEventData, double[] meanNoMLEventData, double[] MLSessionData, double[] NoMLSessionData)
    {
        double score = 0;

        for(int i = 0; i < meanMLEventData.Length; i++)
        {
            double value = meanMLEventData[i] - meanNoMLEventData[i];
            score += System.Math.Sqrt(value * value);
        }

        for(int i = 0; i < MLSessionData.Length; i++)
        {
            double value = MLSessionData[i] - NoMLSessionData[i];
            score += System.Math.Sqrt(value * value);
        }

        return score;
    }

    double[] GetMeanOfData(double[][] inputData)
    {
        double[] meanData = new double[inputData[0].Length];

        for(int i = 0; i < inputData.Length; i++)
        {
            for(int j = 0; j < inputData[i].Length; j++)
            {
                meanData[j] += inputData[i][j];
            }
        }

        for(int i = 0; i < meanData.Length; i++)
        {
            meanData[i] /= inputData.Length;
        }

        return meanData;
    }

    double[] GetMeanOfData(List<double[]> inputData)
    {
        double[] meanData = new double[inputData[0].Length];

        for (int i = 0; i < inputData.Count; i++)
        {
            for (int j = 0; j < inputData[i].Length; j++)
            {
                meanData[j] += inputData[i][j];
            }
        }

        for (int i = 0; i < meanData.Length; i++)
        {
            meanData[i] /= inputData.Count;
        }

        return meanData;
    }

    double[][] OrganiseSessionData(string[][] inputData)
    {
        double[][] organisedData = new double[inputData.Length][];

        for(int i = 0; i < inputData.Length; i++)
        {
            double[] newEntry = new double[inputData[i].Length];
            for(int j = 0; j < inputData[i].Length; j++)
            {
                newEntry[j] = double.Parse(inputData[i][j]);
            }
            organisedData[i] = newEntry;
        }

        return organisedData;
    }

    double[][] OrganiseEventData(string[][] inputData)
    {
        double[][] organisedData = new double[inputData.Length][];           

        UnityTime UTime = new UnityTime();
        string previousTime = "0:00:0000";

        for(int i = 0; i < inputData.Length; i++)
        {
            double[] newEntry = new double[inputData[i].Length - 2];
            for(int j = 0; j < inputData[i].Length - 2; j++)
            {
                if(j == 1)          // Position of the gun name.
                {
                    newEntry[j] = GetGunID(inputData[i][j + 1]);
                }else if(j == 4)        // Position of the timestamp.
                {
                    newEntry[j] = UTime.ConvertToSeconds(inputData[i][j + 1]) - UTime.ConvertToSeconds(previousTime);
                    previousTime = inputData[i][j + 1];
                }else
                {
                    newEntry[j] = double.Parse(inputData[i][j + 1]);
                }
            }
            organisedData[i] = newEntry;
        }

        return organisedData;
    }

    int GetGunID(string gunName)
    {
        int ID = 0;
        switch(gunName)
        {
            case "Assult Rifle":
                ID = 1;
                break;
            case "Shotgun":
                ID = 2;
                break;
            case "Sniper Rifle":
                ID = 3;
                break;
        }
        return ID;
    }

    string[][] GetData(string ConnectionPath, string tableName)
    {
        IDbConnection conn = new SqliteConnection(ConnectionPath);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM " + tableName;
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        int fields = reader.FieldCount;
        int entryCount = 0;
        while(reader.Read())
        {
            entryCount++;
        }
        reader.Close();

        reader = comm.ExecuteReader();

        string[][] data = new string[entryCount][];
        int iter = 0;
        while (reader.Read())
        {
            string[] newEntry = new string[fields];
            for(int i = 0; i < fields; i++)
            {
                switch (reader.GetDataTypeName(i))
                {
                    case "INTEGER":
                        newEntry[i] = "" + reader.GetValue(i);
                        break;
                    case "TEXT":
                        newEntry[i] = reader.GetString(i);
                        break;
                    case "REAL":
                        newEntry[i] = "" + reader.GetDouble(i);
                        break;
                }

               
            }
            data[iter] = newEntry;
            iter++;
        }
        conn.Close();

        return data;
    }
	
}
