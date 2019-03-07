using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityTime
{
    // These are some useful methods that can be used in relation to Unity's Time class.

    // Seperates the seconds from the milliseconds.
    public int GetSeconds(float timeSinceStart)
    {
        int seconds = 0;

        string timeSinceStart_text = "" + timeSinceStart;
        string seconds_Text = "";
        for (int i = 0; timeSinceStart_text[i] != '.'; i++) { seconds_Text += timeSinceStart_text[i]; }
        seconds = int.Parse(seconds_Text);

        return seconds;
    }

    // Seperates the milliseconds from the seconds.
    public int GetMilliseconds(float timeSinceStart)
    {
        int milliseconds = 0;

        string timeSinceStart_text = "" + timeSinceStart;
        string milliseconds_Text = "";
        for (int i = timeSinceStart_text.Length - 1; timeSinceStart_text[i] != '.'; i--) { milliseconds_Text += timeSinceStart_text[i]; }
        milliseconds = int.Parse(milliseconds_Text);

        return milliseconds;
    }

    // Gets the total mins that have elapsed based on the seconds that have elapsed.
    public int GetMinutes(int seconds)
    {
        int mins = 0;

        mins = seconds / 60;

        return mins;
    }

    public double ConvertToSeconds(string input)
    {
        double output = 0;

        // IDs - 0 = mins, 1 = secs, 2 = millisecs
        int[] times = new int[3];

        string num = "";
        int currID = 0; 
        for(int i = 0; i < input.Length; i++)
        {
            if(input[i] != ':'){num += input[i];}
            else
            {
                times[currID] = int.Parse(num);
                currID++;
                num = "";
            }
        }

        output = times[0] * 60;
        output += times[1];
        output += times[2] / 100;

        return output;
    }

}
