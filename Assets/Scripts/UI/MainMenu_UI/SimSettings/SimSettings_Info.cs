using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimSettings_Info
{
    public string title;
    // bool in the dictionary indicates if the string is a formula
    public Dictionary<string, bool> text_strings;

    public SimSettings_Info(string _title, Dictionary<string, bool> _text_strings)
    {
        title=_title;
        text_strings=_text_strings;
    }
}

public static class SimSettings_InfoList
{
    public static SimSettings_Info simulateGravity = new SimSettings_Info("Simulate Gravity", new Dictionary<string, bool>() {
        { "some text with some <color=green><b>green</b></color>", false},
        { "\\frac{1}{3}", true }
    });
}