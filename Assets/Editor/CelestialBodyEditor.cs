using UnityEngine;
using UnityEditor;
using System.IO;
using UniCsts = UniverseConstants;
using ObjHand = CommonMethods.ObjectsHandling;
using PlDict = CelestialBodiesConstants;

[CustomEditor(typeof(CelestialBody),true)]
public class CelestialBodyEditor : Editor
{
    CelestialBody celestBody;
    string celestBodySettingsFilePath;
    bool bodySettingsFoldoutBool;
    SerializedObject settingsObj;

    void OnEnable()
    {
        celestBody = (CelestialBody)target;

        ObjHand.Check_Create_Directory(Application.persistentDataPath + Filepaths.celestBody_Folder + celestBody._gameObject.name, true);
        celestBodySettingsFilePath = Application.persistentDataPath + Filepaths.celestBody_Folder + celestBody._gameObject.name + Filepaths.celestBodySettingsFile;
        CheckCreate_CelestialBodySettings_SO();
        settingsObj = new SerializedObject(serializedObject.FindProperty("settings").objectReferenceValue);

        bodySettingsFoldoutBool = celestBody.settings.bodySettingsBoolFoldout;
    }

    void CheckCreate_CelestialBodySettings_SO()
    {
        if(celestBody.settings == null)
            celestBody.settings = ScriptableObject.CreateInstance<CelestialBodySettings>();
        if(!File.Exists(celestBodySettingsFilePath))
            CelestialBodySettings.WriteToFile_CelestialBodySettings_SaveData(celestBody.settings, celestBodySettingsFilePath);
        JsonUtility.FromJsonOverwrite(File.ReadAllText(celestBodySettingsFilePath), celestBody.settings);
    }

    void OnInspectorUpdate()
    {
        this.Repaint();
    }

    public override void OnInspectorGUI()
    {
        settingsObj.Update();

        if (!EditorGUIUtility.wideMode)
            EditorGUIUtility.wideMode = true;

        using(var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
        }

        if(!celestBody.usePredefinedPlanet && !celestBody.spawnAsUIPlanet)
            Draw_Dict_PlanetaryParams(celestBody.settings);
        else if(celestBody.usePredefinedPlanet)
            Handle_Predefined_Planet_Selection();
        else if(!celestBody.usePredefinedPlanet && celestBody.spawnAsUIPlanet)
            Debug.LogError("ERROR ! You cannot use an unpredefined planet with the 'spawnAsUIPlanet' flag that is used for UI. Cannot continue.");
        
        if(GUI.changed) {
            if(celestBody.spawnAsUIPlanet)
                Debug.LogWarningFormat("WARNING ! GUI has changed for body '{0}', but 'spawAsUIPlanet' is selected. Thus, changes are not being saved to disk.", celestBody._gameObject.name);
            else {
                string filepath = CelestialBodySettings.WriteToFile_CelestialBodySettings_SaveData(celestBody.settings, celestBodySettingsFilePath);
                Debug.Log("CelestialBodySettings successfully saved at: '" + celestBodySettingsFilePath + "'.");
            }
        }

        settingsObj.ApplyModifiedProperties();
    }

    void Handle_Predefined_Planet_Selection()
    {
        celestBody.chosenPredifinedPlanet = (CelestialBodiesConstants.planets)EditorGUILayout.EnumPopup("Predefined planet", celestBody.chosenPredifinedPlanet);
        
        if(celestBody.chosenPredifinedPlanet == CelestialBodiesConstants.planets.None) {
            Debug.LogWarningFormat("WARNING ! You have selected the 'None' predefined planet for body named {0}. Cannot continue.", celestBody._gameObject.name);
            return;
        }
        
        // FILLING THE celestBody.settings
        // Planetary Params
        celestBody.settings.planetaryParams.radius = (Distance)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.radius.ToString()];
        celestBody.settings.planetaryParams.polarRadius = (Distance)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()];
        celestBody.settings.planetaryParams.inverseFlattening = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()];
        celestBody.settings.planetaryParams.radiusSOI = (Distance)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()];
        celestBody.settings.planetaryParams.axialTilt = (Angle)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()];
        celestBody.settings.planetaryParams.siderealRotPeriod = (Time_Class)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()];
        celestBody.settings.planetaryParams.revolutionPeriod = (Time_Class)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()];
        celestBody.settings.planetaryParams.mu = (GravConstant)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.mu.ToString()];
        celestBody.settings.planetaryParams.massEarthRatio = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()];

        // Orbital Params
        celestBody.settings.lightOrbitalParams.orbitedBodyName = (String_Unit)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()];
        celestBody.settings.lightOrbitalParams.apoapsis = (Distance)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()];
        celestBody.settings.lightOrbitalParams.periapsis = (Distance)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.orbitalParams.periapsis.ToString()];
        celestBody.settings.lightOrbitalParams.i = (Angle)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.orbitalParams.i.ToString()];
        celestBody.settings.lightOrbitalParams.lAscN = (Angle)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.orbitalParams.lAscN.ToString()];
        celestBody.settings.lightOrbitalParams.periapsisArg = (Angle)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()];
        celestBody.settings.lightOrbitalParams.trueAnomaly = (Angle)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()];

        // Surface Params
        celestBody.settings.surfaceParams.surfPressure = (Pressure)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.biomeParams.surfPressure.ToString()];
        celestBody.settings.surfaceParams.surfDensity = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.biomeParams.surfDensity.ToString()];
        celestBody.settings.surfaceParams.surfTemp = (Temperature)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.biomeParams.surfTemp.ToString()];
        celestBody.settings.surfaceParams.maxAtmoHeight = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()];
        celestBody.settings.surfaceParams.highestBumpAlt = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()];

        // Jn
        celestBody.settings.jns.j2 = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.jnParams.j2.ToString()];
        celestBody.settings.jns.j3 = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.jnParams.j3.ToString()];
        celestBody.settings.jns.j4 = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.jnParams.j4.ToString()];
        celestBody.settings.jns.j5 = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.jnParams.j5.ToString()];
        celestBody.settings.jns.j6 = (DoubleNoDim)PlDict.planetsDict[celestBody.chosenPredifinedPlanet][CelestialBodyParamsBase.jnParams.j6.ToString()];
    }

    void Draw_Dict_PlanetaryParams(CelestialBodySettings settings)
    {
        if(settings != null) {
            bodySettingsFoldoutBool = EditorGUILayout.InspectorTitlebar(bodySettingsFoldoutBool, settings);
            using(var check = new EditorGUI.ChangeCheckScope()) {
                if(bodySettingsFoldoutBool)
                    Create_Dict_BodySettings_Editor(settings);
            }
        }
    }
    void Create_Dict_BodySettings_Editor(CelestialBodySettings settings)
    {
        SerializedProperty planetaryObj = settingsObj.FindProperty("planetaryParams");
        EditorGUILayout.PropertyField(planetaryObj, true);

        SerializedProperty lightOrbitalObj = settingsObj.FindProperty("lightOrbitalParams");
        EditorGUILayout.PropertyField(lightOrbitalObj, true);

        SerializedProperty surfaceObj = settingsObj.FindProperty("surfaceParams");
        EditorGUILayout.PropertyField(surfaceObj, true);

        SerializedProperty jnsObj = settingsObj.FindProperty("jns");
        EditorGUILayout.PropertyField(jnsObj, true);
    }
}