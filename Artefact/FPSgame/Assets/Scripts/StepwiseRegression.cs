using System.Collections;
using System.Collections.Generic;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Statistics.Analysis;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;

using Accord.Statistics;
using Accord.Statistics.Distributions.Univariate;

using System;

public class StepwiseRegression {

    const string DB_name = "/DB_Official.db";

    double[] AR_model_Coeffs;
    double[] Shotgun_model_Coeffs;
    double[] Sniper_model_Coeffs;

    double highestAR_score;
    double highestShotgun_score;
    double highestSniper_score;

    bool mapPreferencesToPercentage = false;

    public void GetModel()
    {
        // Counting data like how many sessions are there.
        string connectionString = "URI=file:" + Application.dataPath + DB_name;
        IDbConnection conn = new SqliteConnection(connectionString);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM Events";
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        int columnCount = reader.FieldCount - 2;
        int sessionCount = 0;
        int eventCount = 0;

        List<int> sessionIDs = new List<int>();
        while (reader.Read())
        {
            eventCount++;
            if (!sessionIDs.Contains(reader.GetInt16(1)))
            {
                sessionIDs.Add(reader.GetInt16(1));
                sessionCount++;
            }
        }
        reader.Close();

        // Data to get to be sorted to get independent and dependent variables.
        double[][] inputs_Events = GetEventData(sessionCount, columnCount, eventCount);     // independent 
        double[][] inputs_Session = GetSessionData(sessionCount);                           // independent

        /*for(int i =0; i < inputs_Session.Length; i++)
        {
            string outty = "thing " + i + " : ";
            for(int j = 0; j < inputs_Session[i].Length; j++)
            {
                outty += inputs_Session[i][j] + ", ";
            }
            Debug.Log(outty);
        }*/
        Debug.Log(sessionCount);
        double[][] weaponPrefs = GetWeaponPreferences(inputs_Session);                      // dependent

        // Stores the dependent variables for each of the different weapons.
        double[] AR_pref = new double[inputs_Session.Length];
        double[] shotgun_pref = new double[inputs_Session.Length];
        double[] sniper_pref = new double[inputs_Session.Length];

        // Store the combined independent variables.
        double[][] inputs = new double[sessionCount][];
        // Combine the events and the session data into the one array.
        for (int i = 0; i < inputs.Length; i++)
        {
            double[] newRow = new double[inputs_Events[i].Length + inputs_Session[i].Length];
            // Add the events data.
            for (int j = 0; j < inputs_Events[i].Length - 1; j++)
            {
                newRow[j] = inputs_Events[i][j];
            }
            // Add the session data.
            for (int j = 0; j < inputs_Session[i].Length; j++)
            {
                newRow[j + inputs_Events[i].Length - 1] = inputs_Session[i][j];
            }
            inputs[i] = newRow;
        }

        // Read weapon prefs into individual arrays.
        for (int i = 0; i < inputs_Session.Length; i++)
        {
            AR_pref[i] = weaponPrefs[0][i];
            shotgun_pref[i] = weaponPrefs[1][i];
            sniper_pref[i] = weaponPrefs[2][i];
        }

        // Get the prediction models for each of the weapon preferences.
        AR_model_Coeffs = GetWeaponPreferenceModel(inputs, AR_pref);
        Shotgun_model_Coeffs = GetWeaponPreferenceModel(inputs, shotgun_pref);
        Sniper_model_Coeffs = GetWeaponPreferenceModel(inputs, sniper_pref);


        // Find the highest score in the data so that a percentage of the highest can be calcualted.
        for (int i = 0; i < inputs.Length; i++)
        {
            double AR_pref_score = PredictWeaponPref("AR", inputs[i]);
            if (AR_pref_score > highestAR_score) { highestAR_score = AR_pref_score; }

            double shotgun_pref_score = PredictWeaponPref("Shotgun", inputs[i]);
            if (shotgun_pref_score > highestShotgun_score) { highestShotgun_score = shotgun_pref_score; }

            double sniper_pref_score = PredictWeaponPref("Sniper", inputs[i]);
            if (sniper_pref_score > highestSniper_score) { highestSniper_score = sniper_pref_score; }
        }
    }
    // Calculates a human designed way of getting a player's weapon preference.
    double[][] GetWeaponPreferences(double[][] sessionData)
    {
        double[][] outputs = new double[3][];
        for (int i = 0; i < outputs.Length; i++) { outputs[i] = new double[sessionData.Length]; }

        for(int i = 0; i < sessionData.Length; i++)
        {
            double totalTimer = sessionData[i][9] + sessionData[i][13] + sessionData[i][17];

            outputs[0][i] = ((sessionData[i][6] + sessionData[i][7] + sessionData[i][8])/sessionData[i][4]) * (sessionData[i][9] / 3);
            outputs[1][i] = ((sessionData[i][10] + sessionData[i][11] + sessionData[i][12]) / sessionData[i][4]) * (sessionData[i][13] / 3);
            outputs[2][i] = ((sessionData[i][14] + sessionData[i][15] + sessionData[i][16]) / sessionData[i][4]) * (sessionData[i][17] / 3);

            // Map the pref scores between 0 and 1.
            double totalPref = outputs[0][i] + outputs[1][i] + outputs[2][i];

            outputs[0][i] = outputs[0][i] / totalPref;
            outputs[1][i] = outputs[1][i] / totalPref;
            outputs[2][i] = outputs[2][i] / totalPref;
        }

        return outputs;
    }
    // Convert the name of a gun into it's appropriate gunID.
    int ConvertToGunID(string gunName)
    {
        int ID = 0;
        switch (gunName)
        {
            case "Shotgun":
                ID = 2;
                break;
            case "Sniper Rifle":
                ID = 3;
                break;
            case "Assult Rifle":
                ID = 1;
                break;
        }
        return ID;
    }
    // Returns the mean event data for each session in the database.
    double[][] GetEventData(int sessionCount, int columnCount, int eventCount)
    {
        double[][] eventData = null;

        // Read from database.
        string connectionString = "URI=file:" + Application.dataPath + DB_name;
        IDbConnection conn = new SqliteConnection(connectionString);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM Events";
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        eventData = new double[sessionCount][];

        int sessionCounter = 0;   
        int rowCounter = 0;
        string previousTimeStamp = "";
        List<int> sessionIDs = new List<int>();
        UnityTime UT = new UnityTime();

        double[] newRow = null; // Stores cumulative session data. Is averaged and then stored in the array to be output when a new session is found. 
        while (reader.Read())
        {
            // Check for a new session.
            if (!sessionIDs.Contains(reader.GetInt32(1)))
            {
                sessionIDs.Add(reader.GetInt32(1));

                if(newRow != null)
                {
                    // Aveage the data stored on the previous session and store in the array to be output.
                    eventData[sessionCounter] = MakeRowValuesMean(newRow, rowCounter);
                    sessionCounter++;
                }
                // Reset variables.
                rowCounter = 0;
                newRow = new double[columnCount];
            }
            // Store row data.
            for (int i = 0; i < columnCount; i++)
            {
                switch(reader.GetName(i + 2))
                {
                    case "KillWeapon":
                        newRow[i] += ConvertToGunID(reader.GetString(i + 2));
                        break;
                    case "TimeStamp":
                        newRow[i] += UT.ConvertToSeconds(reader.GetString(i + 2)) - UT.ConvertToSeconds(previousTimeStamp);
                        previousTimeStamp = reader.GetString(i + 2);
                        break;
                    default:
                        newRow[i] += reader.GetDouble(i + 2);
                        break;
                }
            }
            rowCounter++;
        }
        eventData[sessionCounter] = MakeRowValuesMean(newRow, rowCounter);             // Add leftover averaged session data.
        reader.Close();

        return eventData;
    }
    // Return an average of a given array
    double[] MakeRowValuesMean(double[] data, int rowCount)
    {
        for(int i = 0; i < data.Length; i++)
        {
            data[i] = data[i] / rowCount;
        }
        return data;
    }
    // Return the data from the session table.
    double[][] GetSessionData(int sessionCount)
    {
        double[][] inputs = new double[sessionCount][];

        string connectionString = "URI=file:" + Application.dataPath + DB_name;
        IDbConnection conn = new SqliteConnection(connectionString);
        conn.Open();

        IDbCommand comm = conn.CreateCommand();
        string query = "SELECT * FROM SessionData";
        comm.CommandText = query;
        IDataReader reader = comm.ExecuteReader();

        int columnCount = reader.FieldCount - 1;
        int sessionCounter = 0;
        while(reader.Read())
        {
            double[] newRow = new double[columnCount];
            for(int i = 0; i < columnCount; i++)
            {
                newRow[i] = reader.GetDouble(i + 1);
            }
            inputs[sessionCounter] = newRow;
            sessionCounter++;
        }
        return inputs;
    }

    // Takes the inputs and outputs and prodcues an array containg the resulting coefficients.
    double[] GetWeaponPreferenceModel(double[][] inputs, double[] outputs)
    {
        double[] coefficients = new double[inputs[0].Length];

        // Use Accord.net's stepwise regression algorithm on the data.
        var regression = new StepwiseLogisticRegressionAnalysis(inputs, outputs);
        regression.Learn(inputs, outputs);
        StepwiseLogisticRegressionModel model = regression.Complete;

        string outty = "Coeffs: ";
        for(int i = 0; i < coefficients.Length; i++)
        {
            // Check if the leftover coefficient is signifigant as the accord.net library do not sort the insignifgant ones out 
            //of the model at the stage they are being taken from.
            if (model.CoefficientValues[i] > 0.1f) { coefficients[i] = model.CoefficientValues[i]; }
            else { coefficients[i] = 0; }
            outty += coefficients[i] + ", ";
        }
        //Debug.Log(outty);

        return coefficients;
    }

    // Use the machine learned models to predict the weapon preference of a given weapon.
    public double PredictWeaponPref(string weapon, double[] inputs)
    {
        // Check the inputs are valid with each of the models.
        if (!ValidatePredictionVariables(inputs, AR_model_Coeffs) || 
            !ValidatePredictionVariables(inputs, Shotgun_model_Coeffs) ||!ValidatePredictionVariables(inputs, Sniper_model_Coeffs))
        {
            Debug.Log("Input not valid");
            return 0;
        }

        double weaponPreferenceScore = 0;

        double[] coeffs = null;
        // Decide which model to use to calculate the score.
        switch(weapon)
        {
            case "AR":
                coeffs = AR_model_Coeffs;
                break;
            case "Shotgun":
                coeffs = Shotgun_model_Coeffs;
                break;
            case "Sniper":
                coeffs = Sniper_model_Coeffs;
                break;
            default:
                Debug.Log("Not a valid weapon.");
                break;
        }

        // Calculate the weapon prefernce.
        for(int i = 0; i < coeffs.Length; i++)
        {
            weaponPreferenceScore += (inputs[i] * coeffs[i]);
        }

        if(mapPreferencesToPercentage && highestAR_score != 0 && highestShotgun_score != 0 && highestSniper_score != 0)
        {
            switch(weapon)
            {
                case "AR":
                    weaponPreferenceScore = weaponPreferenceScore / highestAR_score;
                    break;
                case "Shotgun":
                    weaponPreferenceScore = weaponPreferenceScore / highestShotgun_score;
                    break;
                case "Sniper":
                    weaponPreferenceScore = weaponPreferenceScore / highestSniper_score;
                    break;
            }
        }


        return weaponPreferenceScore;    
    }
    // Validates the given inputs and model for predicting weapon preference.
    bool ValidatePredictionVariables(double[] inputs, double[] model)
    {
        // Pressence checks.
        if (inputs == null) { Debug.Log("Inputs are missing"); return false; }
        if (model == null) { Debug.Log("Model is missing"); return false; }

        // Check if the inputs and model are compatible with each other.
        if (inputs.Length != model.Length) { Debug.Log("lengths - Inputs: " + inputs.Length + "; Model: " + model.Length); return false; }


        return true;
    }

    public void MapPreferencesAsPercentage(bool state)
    {
        mapPreferencesToPercentage = state;
    }
}