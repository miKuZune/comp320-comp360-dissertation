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

}
