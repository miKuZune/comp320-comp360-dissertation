using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data.SqlClient;
using System;

public class TestDatabaseScript: MonoBehaviour
{
    
    string[] names = { "Barry", "Bob", "Dave", "David", "Robert", "Tamia", "Kayle", "Alexandria" };
    int dataNumber = 3000;

    public void InsertData()
    {
        string connection = "URI=file:" + Application.dataPath + "/DB_Official.DB";
        string tableName = "PersonData";

        List<CollectedData> data = CreateData(dataNumber);

        Debug.Log("TIME starting insertion: " + DateTime.Now.ToString("h:mm:ss:ms"));

        DataInsertPost(data, connection, tableName);

        Debug.Log("TIME ending insertion: " + DateTime.Now.ToString("h:mm:ss:ms"));

        
    }

    public void DataInsertPre(List<CollectedData> data, string connection, string tableName)
    {
        IDbConnection conn = new SqliteConnection(connection);
        conn.Open();

        for (int i = 0; i < data.Count; i++)
        {
            string sql_command = "INSERT INTO " + tableName + "(Name,Number) Values('" + data[i].entriesName + "','" + data[i].num + "')";

            IDbCommand comm = conn.CreateCommand();
            comm.CommandText = sql_command;
            comm.ExecuteNonQuery();
        }

        conn.Close();
    }

    public void DataInsertPost(List<CollectedData> data, string connection, string tableName)
    {
        IDbConnection conn = new SqliteConnection(connection);
        conn.Open();

        string sql_command = "INSERT INTO " + tableName + "(Name,Number) Values";

        for (int i = 0; i < data.Count; i++)
        {
            sql_command += "('" + data[i].entriesName + "','" + data[i].num + "')";

            if (i != data.Count - 1) { sql_command += ","; }
            else { sql_command += ";"; }

        }

        IDbCommand comm = conn.CreateCommand();
        comm.CommandText = sql_command;
        comm.ExecuteNonQuery();

        conn.Close();
    }

    public List<CollectedData> CreateData(int entryNumber)
    {
        List<CollectedData> collectedDatas = new List<CollectedData>();
        System.Random rand = new System.Random(10);

        for(int i = 0; i < entryNumber; i++)
        {
            CollectedData collData = new CollectedData();
            collData.entriesName = names[rand.Next(0, names.Length)];
            collData.num = rand.Next(0, 100);
            collectedDatas.Add(collData);
        }

        return collectedDatas;
    }
	
}

public class CollectedData
{
    public string entriesName;
    public  int num;
}