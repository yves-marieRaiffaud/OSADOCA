using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.IO;

public class simUI : MonoBehaviour
{
    public UniverseRunner universeRunner;
    public TMPro.TMP_Text velocityVal;
    public TMPro.TMP_Text altitudeVal;

    string fileName = "dataOrbit.txt";
    StreamWriter sr;

    void Start()
    {
        if(universeRunner == null) { Debug.LogError("The UniverseRunner instance has not been set in the editor."); return; }

        if (File.Exists(fileName))
        {
            Debug.Log(fileName+" already exists.");
        }
        sr = File.CreateText(fileName);
    }

    void LateUpdate()
    {
        string velocity = UsefulFunctions.DoubleToSignificantDigits(universeRunner.activeSpaceship.GetRelativeVelocityMagnitude(), UniCsts.UI_SIGNIFICANT_DIGITS);
        string altitude = UsefulFunctions.DoubleToSignificantDigits(universeRunner.activeSpaceship.GetAltitude(), UniCsts.UI_SIGNIFICANT_DIGITS);

        velocityVal.text = velocity + " m/s";
        altitudeVal.text = altitude + " km";
    
        sr.WriteLine (velocity + ";" + altitude);
    }

    void OnApplicationQuit()
    {
        sr.Close();
    }
}