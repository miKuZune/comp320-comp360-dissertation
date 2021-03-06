﻿using System.Collections;
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

        double[] highestDifferencePerVariable_Event;
        double[] highestDifferencePerVariable_Session;
        int iter = 0;

        List<double[]> currSessionsEventData = new List<double[]>();
        // Get the mean of each session of the No ML event data.
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
        // Get the mean of each sessions data when ML was present.
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

        // Calculate the experience difference scores.
        List<double> experienceDifferenceScores = new List<double>();

        double[] highestDifferences = GetHighestDifferences(meanNoMLEventDataOfEachSession, meanMLEventDataOfEachSession, noML_sessionData, ML_sessionData);

        for (int i = 0; i < meanNoMLEventDataOfEachSession.Length; i++)
        {
            experienceDifferenceScores.Add(GetDifferenceScore(meanMLEventDataOfEachSession[i], meanNoMLEventDataOfEachSession[i], ML_sessionData[i], noML_sessionData[i], highestDifferences));
        }


        // Get the mean of all non ML data.
        double[] meanOfAllNoMLEventData = GetMeanOfData(noML_eventData);
        double[] meanOfAllNoMLSessionData = GetMeanOfData(noML_sessionData);
        // For storing the data without session IDs.
        double[] meanOfAllNoMLEventData_Sorted = new double[meanOfAllNoMLEventData.Length - 1];
        double[] meanOfAllNoMLSessionData_Sorted = new double[meanOfAllNoMLSessionData.Length - 1];
        // Store a sorted version of each sessions data
        double[][] meanNoMLeventDataOfEachSession_Sorted = new double[meanNoMLEventDataOfEachSession.Length][];
        double[][] NoMLSessionData_Sorted = new double[noML_sessionData.Length][];
        // Sort the session IDs out of the event data.
        for(int i = 0; i < meanNoMLEventDataOfEachSession.Length; i++)
        {
            double[] newEntry = new double[meanNoMLEventDataOfEachSession[i].Length - 1];
            for(int j = 1; j < meanNoMLEventDataOfEachSession[i].Length; j++)
            {
                newEntry[j - 1] = meanNoMLEventDataOfEachSession[i][j];
            }
            meanNoMLeventDataOfEachSession_Sorted[i] = newEntry;
        }
        // Sort the session IDs out of the session data.
        for(int i = 0; i < noML_sessionData.Length; i++)
        {
            double[] newEntry = new double[noML_sessionData.Length - 1];
            for(int j = 1; j < noML_sessionData[i].Length; j++)
            {
                newEntry[j - 1] = noML_sessionData[i][j];
            }
            NoMLSessionData_Sorted[i] = newEntry;
        }
        // Sort the session IDs out of the mean data.
        for(int i = 1; i < meanOfAllNoMLEventData.Length; i++){meanOfAllNoMLEventData_Sorted[i - 1] = meanOfAllNoMLEventData[i];}
        for(int i = 1; i < meanOfAllNoMLSessionData.Length; i++){meanOfAllNoMLSessionData_Sorted[i - 1] = meanOfAllNoMLSessionData[i];}

        // Get experience difference scores for each no ml session compared to the average of all no ml sessions.
        List<double> noML_experienceDifferenceScores = new List<double>();


        double[][] temp = new double[meanNoMLeventDataOfEachSession_Sorted.Length][];
        double[][] temp2 = new double[NoMLSessionData_Sorted.Length][];

        for (int i = 0; i < temp.Length; i++) { temp[i] = meanOfAllNoMLEventData_Sorted; }
        for (int i = 0; i < temp2.Length; i++) { temp2[i] = meanOfAllNoMLSessionData_Sorted; }

        highestDifferences = GetHighestDifferences(temp, meanNoMLeventDataOfEachSession_Sorted, temp2, NoMLSessionData_Sorted);

        for(int i = 0; i < meanNoMLeventDataOfEachSession_Sorted.Length; i++)
        {
            double score = GetDifferenceScore(meanOfAllNoMLEventData_Sorted, meanNoMLeventDataOfEachSession_Sorted[i], meanOfAllNoMLSessionData_Sorted, NoMLSessionData_Sorted[i], highestDifferences);
            noML_experienceDifferenceScores.Add(score);
        }

        for(int i = 0; i < noML_experienceDifferenceScores.Count; i++)
        {
            //Debug.Log("NoML vs ML socre: " + experienceDifferenceScores[i] + " NoML vs NoMLAverage: " + noML_experienceDifferenceScores[i]);
        }

        WriteToFile(experienceDifferenceScores, noML_experienceDifferenceScores);
    }

    void WriteToFile(List<double> scores, List<double> scores2)
    {
        string dataPath = Application.dataPath + "/analysesResults.txt";
        Debug.Log(dataPath);

        System.IO.StreamWriter writer;
        writer = new System.IO.StreamWriter(dataPath);
        string inputText = "Ml vs NoML:";
        for(int i = 0; i < scores.Count; i++)
        {
            inputText += scores[i] + "@";
        }

        inputText += "NoML vs NoMlMean:";
        for(int i = 0; i < scores2.Count; i++)
        {
            inputText += scores2[i] + "@";
        }

        inputText = inputText.Replace("@", System.Environment.NewLine);


        writer.Write(inputText);
        writer.Close();

        Debug.Log("File wrote");
    }

    double[] GetHighestDifferences(double[][] eventData1, double[][] eventData2, double[][] sessionData1, double[][] sessionData2)
    {
        Debug.Log("Some quick tests of stuff:");
        Debug.Log(eventData1.Length + " " + eventData2.Length);
        Debug.Log(sessionData1.Length + " " + sessionData2.Length);

        double[] highestVariables = new double[eventData1[0].Length + sessionData1[0].Length];

        for(int i = 0; i < eventData1.Length; i++)
        {
            for(int j = 0; j < eventData1[i].Length; j++)
            {
                double result = eventData1[i][j] - eventData2[i][j];

                if (result < 0) { result = result * -1; }
                if (result > highestVariables[j]) { highestVariables[j] = result;}
            }
        }

        for(int i = 0; i < sessionData1.Length; i++)
        {
            for(int j = 0; j < sessionData1[i].Length; j++)
            {
                double result = sessionData1[i][j] - sessionData2[i][j];
                if (result < 0) { result = result * -1; }
                if (result > highestVariables[j + eventData1[i].Length]) { highestVariables[j + eventData1[i].Length] = result; }
            }
        }

        return highestVariables;
    }

    double GetDifferenceScore(double[] eventData1, double[] eventData2, double[] sessionData1, double[] sessionData2, double[] highestDifferenceScores)
    {
        double score = 0;

        for(int i = 0; i < eventData1.Length; i++)
        {
            double value = eventData1[i] - eventData2[i];

            if (value < 0) { value = value * -1; }

            if (highestDifferenceScores[i] != 0){value = value / highestDifferenceScores[i];}

            score += value;
        }

        for(int i = 0; i < sessionData1.Length; i++)
        {
            double value = sessionData1[i] - sessionData2[i];

            if (value < 0) { value = value * -1; }

            if (highestDifferenceScores[i + eventData1.Length] != 0) { value = value / highestDifferenceScores[i + eventData1.Length]; }

            score += value;
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
