using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mathd_Lib;
using System.IO;
using System.Linq;

using Universe;
using MSDropdownNamespace;
using ObjHand = CommonMethods.ObjectsHandling;

public class SimUI_Dispatcher : MonoBehaviour
{
    public MSDropdown displayOPts_dropdown;
    
    //---------
    UniverseRunner universe;

    void Awake()
    {
        universe = GameObject.Find("UniverseRunner").GetComponent<UniverseRunner>();
    }

    void Start()
    {
        universe.hasDoneStart.AddListener(SimUI_Dispatcher_Init);
    }
    void SimUI_Dispatcher_Init()
    {
        Init_DisplayOpts_Dropdown();
    }

    void Init_DisplayOpts_Dropdown()
    {
        // This method initializes the options and callbacks for the 'Display Options' MS_Dropdown
        // First option is 'Toggle orbit display'
        // Second option is 'Toggle ship axes'
        // Third option is 'Toggle ship thurst axis'
        ObjHand.Check_Create_Directory(Filepaths.simUI_Folder, true);
        string filepath = Filepaths.simUI_Folder + Filepaths.simUI_displayOptions;
        List<stringBoolStruct> loadedOpts = new List<stringBoolStruct>();
        if(File.Exists(filepath)) {
            GenericStringBoolStruct loadedClass = JsonUtility.FromJson<GenericStringBoolStruct>(filepath);
            loadedOpts = loadedClass.stringBoolStructsList;
        }
        else {
            // File not found or do not exist, thus creating default List of options
            loadedOpts.Add(new stringBoolStruct("Toggle orbit display", true));
            loadedOpts.Add(new stringBoolStruct("Toggle ship axes", false));
            loadedOpts.Add(new stringBoolStruct("Toggle ship thrust axis", false));
        }

        displayOPts_dropdown.ClearOptions();
        displayOPts_dropdown.SetOptions(loadedOpts);
        displayOPts_dropdown.buttonsList[0].OnClick.AddListener(Toggle_OrbitDisplay);
        displayOPts_dropdown.buttonsList[1].OnClick.AddListener(Toggle_Ship_Axes);
        displayOPts_dropdown.buttonsList[2].OnClick.AddListener(Toggle_Ship_Thrust_Axis);
    }
    void Toggle_OrbitDisplay()
    {
        Debug.LogWarning("clicked on Toggle Orbit display");
    }
    void Toggle_Ship_Axes()
    {
        Debug.LogWarning("clicked on Toggle Ship axes");
    }
    void Toggle_Ship_Thrust_Axis()
    {
        Debug.LogWarning("clicked on Toggle ship thrust axis");
    }
}