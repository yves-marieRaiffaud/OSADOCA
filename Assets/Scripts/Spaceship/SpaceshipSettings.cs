using UnityEngine;
using System.IO;

[CreateAssetMenu()]
public class SpaceshipSettings : ScriptableObject, FlyingObjSettings
{
    public bool foldoutSpaceshipSettings;
    public bool orbitalParamsEditorFoldout;
    public bool orbitInfoShowPredictedOrbitInfo;

    public double mass; // kg

    public Material bodyMaterial;
    //==============
    public bool startFromGround; // Boolean if the spaceship must spawn on the OrbitedBody surface (no orbit definition then)
    public string startLaunchPadName; // Used Only if StartFromGround is enabled
    public Vector2 groundStartLatLong; // Latitude and longitude for a spacecraft starting from ground. Must have 'startFromGround' set to true
    //==============
    public bool showInfoPanel=false;
}

[System.Serializable]
public struct SpaceshipSettingsSaveData
{
    //=========================================
    public const int NB_PARAMS=4; // Won't be serialized and won't be saved. Used only to set the size of the array passed in the constructor
    //=========================================
    public string massDouble; // double
    public string startFromGroundInt; // Bool
    public string groundStartLatLongVec2; // Vector2
    public string startLaunchPadName; // string

    public SpaceshipSettingsSaveData(params string[] values)
    {
        if(values.Length != NB_PARAMS) {
            Debug.Log("The passed array to save the SpaceshipSettingsSaveData has an incorrect size. Size should be " + NB_PARAMS + ", but passed array has size " + values.Length);
        }
        this.massDouble             = values[0];
        this.startFromGroundInt     = values[1];
        this.groundStartLatLongVec2 = values[2];
        this.startLaunchPadName     = values[3];
    }

    //========================================================================
    public static SpaceshipSettings LoadObjectFromJSON(string filepath)
    {
        SpaceshipSettingsSaveData loadedData = JsonUtility.FromJson<SpaceshipSettingsSaveData>(File.ReadAllText(filepath));
        //==========
        SpaceshipSettings output = (SpaceshipSettings)ScriptableObject.CreateInstance<SpaceshipSettings>();
        UsefulFunctions.ParseStringToDouble(loadedData.massDouble, out output.mass);
        output.startFromGround = System.Convert.ToBoolean(int.Parse(loadedData.startFromGroundInt));
        output.groundStartLatLong = UsefulFunctions.StringToVector2(loadedData.groundStartLatLongVec2);
        output.startLaunchPadName = loadedData.startLaunchPadName;
        //===========
        return output;
    }
}