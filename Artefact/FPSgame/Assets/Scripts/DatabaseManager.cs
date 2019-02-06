using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class DatabaseManager : MonoBehaviour
{
    [SerializeField]
    bool stopInsertion = false;                                                                         // Used to stop this script from entering data to the DB. Useful for testing game systems without messing with data.
    [SerializeField]
    bool useOfficialTable = false;

    public static DatabaseManager instance;                                                             // Store a singleton reference to this script.

    private string connectionString;                                                                    // Stores the path to the storage location of the database.

    const string EventsTable = "Events";                                                                // Stores the name of the table used to store the data.
    const string SessionTable = "Session";

    int currSessionID;                                                                                  // Stores a sessionID value to uniquley identfy the set of data created in one play session.

    List<EventTableData> PreExistingEventData = new List<EventTableData>();                                        // Stores the list of previous data from the database.

    public SessionTableData currSessionData = new SessionTableData();
    List<EventTableData> currSessionEventData = new List<EventTableData>();

    UnityTime unityTime;

    void Awake()
    {
        // Ensures there is only one instance of this script.
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
    }

    public void Start()
    {
        connectionString = "URI=file:" + Application.dataPath + "/EventsDB";                            // Sets the path for db location. Uses Unity's datapath so that it can be found on any machine.
        if (useOfficialTable) { connectionString += "_Official"; Debug.Log("WARNING! Accessing official table."); }
        connectionString += ".sqlite";
        ReadDB();                                                                                       // Store the list of data from the events database.

        currSessionID = GetHighestSessionID() + 1;                                                      // Create a new sessionID by getting the highest sessionID and adding one.
        CreateTable();

        currSessionData.sessionID = currSessionID;

        unityTime = new UnityTime();                                                                    // Custom class containing functions useful for formatting time.
    }

    // Get the highest value for SessionID from the event table data.
    int GetHighestSessionID()
    {
        int sessionID = 0;  
        foreach(EventTableData data in PreExistingEventData)                                                   // Go through all the data from the events table that has been stored in a list.
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
                PreExistingEventData.Add(newReadData);                                                     // Add this data to a list.
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

        Debug.Log("single insertion");
    }

    // Creates the table to store the data in.
    void CreateTable()
    {
        // Connect to the database.
        IDbConnection dbConn = new SqliteConnection(connectionString);
        dbConn.Open();

        // Use of try and catch to stop error messages when the table already exists.
        try
        {
            // Setup and execute the command to create the table.
            string SQL_Comm = "CREATE TABLE " + EventsTable + "(EventID int,SessionID int,KillWeapon string,Distance float,TimeToKill); ";           // SQL command to create the table.
            IDbCommand comm = dbConn.CreateCommand();                                                                                               // Create a command object to execute the SQL.
            comm.CommandText = SQL_Comm;                                                                                                            // Give the command obj the SQL command.
            comm.ExecuteNonQuery();
            // Output that the database has been created. Only seen in Unity Editor and output logs.
            Debug.Log("Table created");
        }
        catch(Exception e)
        {
            Debug.Log("Table not created. " + e.Message);
        }
                                                                                                                      // Execute the command.

        // Close connection to the database.
        dbConn.Close();
        
    }

    public void StoreNewEventData(string killWeapon, float distance, float killtime)
    {
        EventTableData newData = new EventTableData();

        newData.sessionID = currSessionID;
        newData.killWeapon = killWeapon;
        newData.distance = distance;
        newData.killTime = killtime;
        // Get the current time.
        float timeStamp = GameManager.instance.timeSinceStart;

        int milliseconds = unityTime.GetMilliseconds(timeStamp);
        int seconds = unityTime.GetSeconds(timeStamp);
        int mins = unityTime.GetMinutes(seconds);
        newData.timeStamp = mins + ":" + (seconds - (mins * 60)) + ":" + milliseconds;

        currSessionEventData.Add(newData);
    }

    public void InsertAllData()
    {
        IDbConnection dbConn = new SqliteConnection(connectionString);
        dbConn.Open();

        string SQL_Command = "";
        IDbCommand comm;
        // Insert all event data stored.
        for (int i = 0; i < currSessionEventData.Count; i++)
        {
            SQL_Command = "INSERT INTO " + EventsTable + "(SessionID, KillWeapon, Distance, TimeToKill, TimeStamp) Values( '"
                + currSessionEventData[i].sessionID + "','" + currSessionEventData[i].killWeapon + "','"
                + currSessionEventData[i].distance + "','" + currSessionEventData[i].killTime +  "','" + 
                currSessionEventData[i].timeStamp + "')";

            comm = dbConn.CreateCommand();
            comm.CommandText = SQL_Command;
            comm.ExecuteNonQuery();
        }
        Debug.Log("Inserted events");

        // Insert session data.
        
        SQL_Command = "INSERT INTO " + SessionTable + "(SessionID, headShots, bodyShots, missedShots, totalShots, endRound, enemiesKilled, AR_headShots, AR_bodyShots, AR_missedShots, S_headShots, S_bodyShots, S_missedShots, SR_headShots, SR_bodyShots, SR_missedShots)" +
            " values( '"+ currSessionData.sessionID + "','" + currSessionData.head_shots + "','" + currSessionData.body_shots
            + "','" + currSessionData.missed_shots + "','" + currSessionData.total_shots + "','" + currSessionData.endRound
            + "','" + currSessionData.enemiesKilled + "','" + currSessionData.AR_head_shots + "','" + currSessionData.AR_body_shots
            + "','" + currSessionData.AR_missed_shots + "','" + currSessionData.S_head_shots + "','" + currSessionData.S_body_shots
            + "','" + currSessionData.S_missed_shots + "','" + currSessionData.SR_head_shots + "','" + currSessionData.SR_body_shots
            + "','" + currSessionData .SR_missed_shots+ "')";

        comm = dbConn.CreateCommand();
        comm.CommandText = SQL_Command;
        comm.ExecuteNonQuery();

        dbConn.Close();
        Debug.Log("entered data");
    }
}

// Store the data that is stored in the database relating to the kill data.
class EventTableData
{
    public int sessionID;
    public string killWeapon;
    public float distance;
    public float killTime;
    public string timeStamp;
}

// Store the data to be stored in the database relating to the session data.
public class SessionTableData
{
    public int sessionID;
    public int head_shots;
    public int body_shots;
    public int missed_shots;
    public int total_shots;
    public int endRound;
    public int enemiesKilled;

    // Assult rifle data
    public int AR_head_shots;
    public int AR_body_shots;
    public int AR_missed_shots;
    // Shotgun data
    public int S_head_shots;
    public int S_body_shots;
    public int S_missed_shots;
    // Sniper rifle data
    public int SR_head_shots;
    public int SR_body_shots;
    public int SR_missed_shots;
}