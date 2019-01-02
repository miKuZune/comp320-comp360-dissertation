using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class DatabaseManager : MonoBehaviour
{
    public bool stopInsertion = false;

    public static DatabaseManager instance;                                                             // Store a singleton reference to this script.

    private string connectionString;                                                                    // Stores the path to the storage location of the database.

    const string EventsTable = "Events";

    int currSessionID;

    List<EventTableData> EventData = new List<EventTableData>();

    void Awake()
    {
        // Ensure there is only one instance of this script.
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
    }

    public void Start()
    {
        connectionString = "URI=file:" + Application.dataPath + "/EventsDB.sqlite";                     // Sets the path for db location. Uses Unity's datapath so that it can be found on any machine.

        ReadDB();                                                                                       // Store the list of data from the events database.

        currSessionID = GetHighestSessionID() + 1;                                                      // Create a new sessionID by getting the highest sessionID and adding one.
    }

    // Get the highest value for SessionID from the event table data.
    int GetHighestSessionID()
    {
        int sessionID = 0;  
        foreach(EventTableData data in EventData)                                                   // Go through all the data from the events table that has been stored in a list.
        {
            if (data.sessionID > sessionID) { sessionID = data.sessionID; }                         // Store the value of the sessionID if it is higher than the on in the temp variable.
        }
        return sessionID;
    }

    // Read the data from the database and store it in a list.
    private void ReadDB()
    {
        // Connect to the database.
        IDbConnection dbConn = new SqliteConnection(connectionString);                          // Create a object to connect to the database stored in the connection location.
        dbConn.Open();                                                                          // Open the connection to the database.

        // Connect to the table and read data from the table.
        IDbCommand comm = dbConn.CreateCommand();                                               // Create a object that can read and execute SQL commands.
        string SQL_Query = "SELECT * FROM " + EventsTable;                                      // SQL command to read all data from a table.
        comm.CommandText = SQL_Query;                                                           // Store the SQL command in the object that can execute it.
        IDataReader reader = comm.ExecuteReader();                                              // Execute the given SQL command.

        // Get the output from the table.
        int rowCount = reader.FieldCount;
        while (reader.Read())                                                                   // Read the rows in the table. Each iteration reads a new row.
        {
            for(int i = 0; i < rowCount; i++)
            {
                EventTableData newReadData = new EventTableData();                              // Create a new object to store the data.
                newReadData.sessionID = reader.GetInt32(1);                                     // Store the sessionID.
                newReadData.killWeapon = reader.GetString(2);                                   // Store the weapon used to kill.
                newReadData.distance = reader.GetFloat(3);                                      // Store the distance from which the player killed the AI.
                newReadData.killTime = reader.GetFloat(4);                                      // Store the time it took from the first shot to the killing shot.
                EventData.Add(newReadData);                                                     // Add this data to a list.
            }
        }

        // Close connections.
        dbConn.Close();                                                                         // Close the connection to the database so that new connections can be opened.
        reader.Close();                                                                         // Close the reader object to open up the connection to the database.
    }

    public void InsertIntoDB(string killWeapon, float distance, float killTime)
    {
        if (stopInsertion) { return; }                                                          // Stop any data from being input into the database. Useful to test gameplay features without messing around with the DB.
        // Connect to the database.
        IDbConnection dbConn = new SqliteConnection(connectionString);
        dbConn.Open();

        // Connect to the table and insert the data.
        string SQL_Command = "INSERT INTO " + EventsTable + "(SessionID, KillWeapon, Distance, TimeToKill) "                
            + "Values( '" +currSessionID + "','" + killWeapon + "','" +distance + "','" + killTime + "')";                  // Stores the SQL command to insert the data in the correct fields.
        IDbCommand comm = dbConn.CreateCommand();                                               // Allows for SQL commands to be executed.
        comm.CommandText = SQL_Command;                                                         // Store the SQL command in the object that will execute it.
        comm.ExecuteNonQuery();                                                                 // Execute the SQL command that has been given.

        // Close connections
        dbConn.Close();

        Debug.Log("Data inserted");
    }
}

// Store the data that is stored in the database.
class EventTableData
{
    public int sessionID;
    public string killWeapon;
    public float distance;
    public float killTime;
}
