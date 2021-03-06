using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public class DebugGameObject : MonoBehaviour
{
    // Bool to load data in the 'UniverseRunner' instance using the JSON files generated by the StartLocation UI
    // If set to FALSE, the ship will be initialized using user input data in its scriptable objects from the UNITY_EDITOR
    public bool loadShipDataFromUIDiskFile=false;
    [Header("Save Orbital Params Data")]
    public simUI simUIScript;
    public bool saveOrbitalParamsData;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if(simUIScript != null)
        {
            simUIScript.saveOrbitalDataToText = saveOrbitalParamsData;
        }
    }
}
