using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System;

public class DatabaseManager_Tests {

    // Test if the database manager is setup as a singleton when a new instance of the script is created.
    [Test]
    public void DatabaseManager_CreatesSingleton()
    {
        var DB_GameObject = new GameObject().AddComponent<DatabaseManager>();
        
        bool works = false;

        if(DatabaseManager.instance != null)
        {
            works = true;
        }

        Assert.AreEqual(works,true);
    }

    // Test if the Database can be read from.
    [Test]
    public void CanReadFromDB()
    {
        GameObject DB_GameObject = new GameObject();
        var DB_manager = DB_GameObject.AddComponent<DatabaseManager>();
        DB_manager.useOfficialTable = true;

        DB_manager.Start();
        DB_manager.ReadDB();

        Assert.Pass();          // If the code reaches this piont then no errors have been thrown and it is successful.
    }

    [Test]
    public void CanWriteToDB()
    {
        GameObject DB_GameObject = new GameObject();
        DB_GameObject.AddComponent<GameManager>();
        var DB_manager = DB_GameObject.AddComponent<DatabaseManager>();
        DB_manager.useOfficialTable = true;
        DB_manager.Start();

        DB_manager.StoreNewEventData("Sniper", 75.66f, 0.0f, "Sniper");
        DB_manager.currSessionData.SR_head_shots++;

        DB_manager.currSessionData.sessionID = 999;

        DB_manager.InsertAllData();


        Assert.Pass();
    }

    [Test]
    public void ValidEventDataCanBeAdded()
    {
        GameObject DB_GameObject = new GameObject();
        DB_GameObject.AddComponent<GameManager>();
        var DB_manager = DB_GameObject.AddComponent<DatabaseManager>();
        DB_manager.useOfficialTable = true;

        string weaponName = "Sniper Rifle";
        float dist = 75.6565f;
        float timeToKill = 0.0f;
        string currProfileName = "Sniper";

        DB_manager.StoreNewEventData(weaponName, dist, timeToKill, currProfileName);

        Assert.Pass();
    }

    [Test]
    public void InvalidEventDataThrowsException()
    {
        GameObject DB_GameObject = new GameObject();
        DB_GameObject.AddComponent<GameManager>();
        var DB_manager = DB_GameObject.AddComponent<DatabaseManager>();
        DB_manager.useOfficialTable = true;

        string weaponName = "notValidGun";
        float dist = 75.6565f;
        float timeToKill = 0.0f;
        string currProfileName = "notValidProfile";

        try
        {
            DB_manager.StoreNewEventData(weaponName, dist, timeToKill, currProfileName);
            Assert.Fail();
        }
        catch(ArgumentException e){Assert.Pass();}
    }

    [Test]
    public void CanPredictAndStoreWeaponPreferences()
    {
        // Setup gameobject to hold scripts
        GameObject DB_GameObject = new GameObject();

        //Setup Database manager
        var DB_manager = DB_GameObject.AddComponent<DatabaseManager>();
        // Setup gamemanager.
        var Game_Manager = DB_GameObject.AddComponent<GameManager>();
        Game_Manager.Start();
        
        DB_manager.useOfficialTable = true; 
        //Do the function to be tested.
        DB_manager.PredictAndStoreWeaponPrefs();

        Assert.Pass();
    }


    [TearDown]
    public void AfterEveryTest()
    {
        GameObject.Destroy(GameObject.FindObjectOfType<DatabaseManager>());
    }
}
