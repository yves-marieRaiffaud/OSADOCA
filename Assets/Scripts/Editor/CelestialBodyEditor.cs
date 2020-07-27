using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Mathd_Lib;
using System.IO;

[CustomEditor(typeof(CelestialBody),true), CanEditMultipleObjects]
public class CelestialBodyEditor : Editor
{
    private CelestialBody celestBody;
    private SerializedObject serializedOrbitalParams;

    private void OnEnable()
    {
        celestBody = (CelestialBody)target;
        CheckCreateOrbitalParamsAsset();
        serializedOrbitalParams = new SerializedObject(serializedObject.FindProperty("_orbitalParams").objectReferenceValue);
    }

    private void CheckCreateOrbitalParamsAsset()
    {
        string orbitalParamsPath = "Assets/Scripts/OrbitalMechanics/OrbitalParams/" + celestBody.gameObject.name + ".asset";
        if(!File.Exists(orbitalParamsPath))
        {
            Debug.Log("Creating a new instance of OrbitalParams at path: '" + orbitalParamsPath + "'");
            OrbitalParams newInstance = ScriptableObject.CreateInstance<OrbitalParams>();
            AssetDatabase.CreateAsset(newInstance, orbitalParamsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        celestBody.orbitalParams = (OrbitalParams)AssetDatabase.LoadAssetAtPath(orbitalParamsPath, typeof(OrbitalParams));
    }


    public void OnInspectorUpdate()
    {
        this.Repaint();
    }

    public override void OnInspectorGUI()
    {
        if (!EditorGUIUtility.wideMode)
        {
            EditorGUIUtility.wideMode = true;
        }
        using(var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
        }

        serializedOrbitalParams.Update();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("orbitedBodyName"));

        if(CelestialBody.CelestialBodyHasTagName(celestBody, "Planet"))
        {
            DrawCelestialBodySettingsEditor(ref celestBody.settings.bodySettingsEditorFoldout);
        }
        else if(CelestialBody.CelestialBodyHasTagName(celestBody, "Star")) { InitSunParams(); }


        serializedOrbitalParams.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target); 
        }
    }

    private void InitSunParams()
    {
        serializedOrbitalParams.FindProperty("orbitedBodyName").stringValue = "None";
        celestBody.settings.usePredifinedPlanets = true;
        celestBody.settings.chosenPredifinedPlanet = UniCsts.planets.Sun;
        EditorGUI.BeginDisabledGroup(true);
        celestBody.settings.chosenPredifinedPlanet = (UniCsts.planets)EditorGUILayout.EnumPopup("Choose planet", celestBody.settings.chosenPredifinedPlanet);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawCelestialBodySettingsEditor(ref bool foldout)
    {
        if(celestBody.settings != null && celestBody.orbitalParams != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, celestBody.settings);
            using(var check = new EditorGUI.ChangeCheckScope())
            {   
                if(foldout)
                {
                    CreateCelestialBodySettingsEditor();
                }
            }
        } 
    }

    private void CreateCelestialBodySettingsEditor()
    {
        celestBody.settings.bodyMaterial = (Material)EditorGUILayout.ObjectField("Body material", celestBody.settings.bodyMaterial, typeof(Material), false);
        if(celestBody.spawnAsSimpleSphere)
        {
            InitSpawnAsSimpleSpheres();
            return;
        }

        celestBody.settings.heightMap = (Texture2D)EditorGUILayout.ObjectField("Height map", celestBody.settings.heightMap, typeof(Texture2D), false);
        celestBody.showCelestialBodyInfoPanel = EditorGUILayout.Foldout(celestBody.showCelestialBodyInfoPanel, "CelestialBody Info");
        ShowCelestialBodyInfoPanel();

        celestBody.settings.usePredifinedPlanets = EditorGUILayout.Toggle("Use a predifined planet for its orbit", celestBody.settings.usePredifinedPlanets);
        if(celestBody.settings.usePredifinedPlanets)
        {
            Init_SimCelestialBody_UsePredefinedPlanet();
        }
        else{
            if(celestBody.settings.planetBaseParamsDict == null)
            {
                celestBody.settings.planetBaseParamsDict = InitNewPlanetBaseParamsDict();
            }
            CreateOrbitalParametersEditor(celestBody.settings.planetBaseParamsDict);
        }
    }

    private void InitSpawnAsSimpleSpheres()
    {
        celestBody.settings.usePredifinedPlanets = true;
        EditorGUI.BeginDisabledGroup(true);
        celestBody.settings.usePredifinedPlanets = EditorGUILayout.Toggle("Use a predifined planet for its orbit", celestBody.settings.usePredifinedPlanets);
        EditorGUI.EndDisabledGroup();
        celestBody.settings.chosenPredifinedPlanet = (UniCsts.planets)EditorGUILayout.EnumPopup("Choose planet", celestBody.settings.chosenPredifinedPlanet);
        switch(celestBody.settings.chosenPredifinedPlanet)
        {
            case UniCsts.planets.Mercury:
                celestBody.settings.planetBaseParamsDict = UniCsts.mercuryBaseParams;
                break;
            
            case UniCsts.planets.Venus:
                celestBody.settings.planetBaseParamsDict = UniCsts.venusBaseParams;
                break;

            case UniCsts.planets.Earth:
                celestBody.settings.planetBaseParamsDict = UniCsts.earthBaseParams;
                break;

            case UniCsts.planets.Mars:
                celestBody.settings.planetBaseParamsDict = UniCsts.marsBaseParams;
                break;
            
            case UniCsts.planets.Jupiter:
                celestBody.settings.planetBaseParamsDict = UniCsts.jupiterBaseParams;
                break;
            
            case UniCsts.planets.Saturn:
                celestBody.settings.planetBaseParamsDict = UniCsts.saturnBaseParams;
                break;
            
            case UniCsts.planets.Uranus:
                celestBody.settings.planetBaseParamsDict = UniCsts.uranusBaseParams;
                break;
            
            case UniCsts.planets.Neptune:
                celestBody.settings.planetBaseParamsDict = UniCsts.uranusBaseParams;
                break;
        }
    }

    private void ShowCelestialBodyInfoPanel()
    {
        if(celestBody.showCelestialBodyInfoPanel)
        {
            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.LabelField("Position vectors");
            EditorGUILayout.Vector3Field(new GUIContent("Relative Acc", "m.s-2\nStar relative acceleration"), (Vector3)celestBody.orbitedBodyRelativeAcc);
            EditorGUILayout.Vector3Field(new GUIContent("Relative Velocity Incr", "m.s\nStar relative velocity increment"), (Vector3)celestBody.orbitedBodyRelativeVelIncr);
            EditorGUILayout.Vector3Field(new GUIContent("Relative Velocity", "m.s\nStar relative velocity"), (Vector3)celestBody.orbitedBodyRelativeVel);
            EditorGUILayout.Vector3Field(new GUIContent("Universe Position", "Position in the universe, global position"), (Vector3)celestBody.realPosition);
            EditorGUILayout.Separator();

            EditorGUILayout.DoubleField("RadiusU", celestBody.settings.radiusU);
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.Separator();
    }

    private void Init_SimCelestialBody_UsePredefinedPlanet()
    {
        celestBody.settings.chosenPredifinedPlanet = (UniCsts.planets)EditorGUILayout.EnumPopup("Choose planet", celestBody.settings.chosenPredifinedPlanet);
        switch(celestBody.settings.chosenPredifinedPlanet)
        {
            case UniCsts.planets.Mercury:
                celestBody.settings.planetBaseParamsDict = UniCsts.mercuryBaseParams;
                break;
            
            case UniCsts.planets.Venus:
                celestBody.settings.planetBaseParamsDict = UniCsts.venusBaseParams;
                break;

            case UniCsts.planets.Earth:
                celestBody.settings.planetBaseParamsDict = UniCsts.earthBaseParams;
                break;

            case UniCsts.planets.Mars:
                celestBody.settings.planetBaseParamsDict = UniCsts.marsBaseParams;
                break;
            
            case UniCsts.planets.Jupiter:
                celestBody.settings.planetBaseParamsDict = UniCsts.jupiterBaseParams;
                break;
            
            case UniCsts.planets.Saturn:
                celestBody.settings.planetBaseParamsDict = UniCsts.saturnBaseParams;
                break;
            
            case UniCsts.planets.Uranus:
                celestBody.settings.planetBaseParamsDict = UniCsts.uranusBaseParams;
                break;
            
            case UniCsts.planets.Neptune:
                celestBody.settings.planetBaseParamsDict = UniCsts.uranusBaseParams;
                break;
        }
        EditorGUILayout.LabelField("Rendering parameters", EditorStyles.boldLabel);

        celestBody.orbitalParams.orbitDefType = OrbitalTypes.orbitDefinitionType.rarp;
        celestBody.orbitalParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.realOrbit;
        celestBody.orbitalParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.AU_degree;
        celestBody.orbitalParams.drawOrbit = EditorGUILayout.Toggle("Draw orbit", celestBody.orbitalParams.drawOrbit);
        celestBody.orbitalParams.orbitDrawingResolution = EditorGUILayout.IntSlider("Orbit Resolution", celestBody.orbitalParams.orbitDrawingResolution, 5, 500);
        celestBody.orbitalParams.drawDirections = EditorGUILayout.Toggle("Draw directions", celestBody.orbitalParams.drawDirections);

        EditorGUI.BeginDisabledGroup(!celestBody.orbitalParams.drawDirections);
        celestBody.orbitalParams.selectedVectorsDir = (OrbitalTypes.typeOfVectorDir)EditorGUILayout.EnumFlagsField("Direction vectors to draw", celestBody.orbitalParams.selectedVectorsDir);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Separator();
        ShowOrbitInfoPanel();
    }


    public Dictionary<string, double> InitNewPlanetBaseParamsDict()
    {
        return new Dictionary<string, double> {
            { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 0d },
            { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()             , 0d },
            { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
            { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
            { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0d },
            { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 0d },
            { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 0d },

            { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0d },
            { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0d },
            { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0d },
            { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 0d },
            { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 0d },
            { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d },

            { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
            { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
            { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
            { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

            { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
            { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
            { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
            { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
            { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

            { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 0d }
        };
    }

    public void CreateOrbitalParametersEditor(Dictionary<string, double> dict)
    {
        // Dictionary Filling
        EditorGUILayout.LabelField("Planetary Params", EditorStyles.boldLabel);
        dict[CelestialBodyParamsBase.planetaryParams.radius.ToString()] = EditorGUILayout.DoubleField("Equatorial radius", dict[CelestialBodyParamsBase.planetaryParams.radius.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()] = EditorGUILayout.DoubleField("Polar radius", dict[CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()] = EditorGUILayout.DoubleField("Inverse flattening", dict[CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()] = EditorGUILayout.DoubleField("SOI Radius", dict[CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()] = EditorGUILayout.DoubleField("Axial tilt", dict[CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()] = EditorGUILayout.DoubleField("Sidereal Rot period", dict[CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.mu.ToString()] = EditorGUILayout.DoubleField("µ", dict[CelestialBodyParamsBase.planetaryParams.mu.ToString()]);
        dict[CelestialBodyParamsBase.otherParams.isRockyBody.ToString()] = EditorGUILayout.DoubleField("Rocky planet ?", dict[CelestialBodyParamsBase.otherParams.isRockyBody.ToString()]);

        EditorGUILayout.Separator();
        // ORBITAL PARAMETERS
        EditorGUILayout.LabelField("Rendering parameters", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("drawOrbit"));
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("orbitDrawingResolution"));
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("drawDirections"));
        EditorGUI.BeginDisabledGroup(!serializedOrbitalParams.FindProperty("drawDirections").boolValue);
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("selectedVectorsDir"));
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Separator();
        
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("orbitRealPredType"));
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("orbParamsUnits"));
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("orbitDefType"));

        EditorGUILayout.LabelField("Orbital Parameters", EditorStyles.boldLabel);
        switch(serializedOrbitalParams.FindProperty("orbitDefType").intValue)
        {
            case 0:
                // 'rarp'
                EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("ra"));
                EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("rp"));
                break;
            case 1:
                // 'rpe'
                EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("rp"));
                EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("e"));
                break;
            case 2:
                // 'pe'
                EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("p"));
                EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("e"));
                break;
        }
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("i"));
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("lAscN"));
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("omega"));
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("nu"));


        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Biome Parameters", EditorStyles.boldLabel);
        dict[CelestialBodyParamsBase.biomeParams.surfPressure.ToString()] = EditorGUILayout.DoubleField("Surface pressure", dict[CelestialBodyParamsBase.biomeParams.surfPressure.ToString()]);
        dict[CelestialBodyParamsBase.biomeParams.surfDensity.ToString()] = EditorGUILayout.DoubleField("Surface density", dict[CelestialBodyParamsBase.biomeParams.surfDensity.ToString()]);
        dict[CelestialBodyParamsBase.biomeParams.surfTemp.ToString()] = EditorGUILayout.DoubleField("Surface temperature", dict[CelestialBodyParamsBase.biomeParams.surfTemp.ToString()]);
        dict[CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()] = EditorGUILayout.DoubleField("Max atmo height", dict[CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()]);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Jn Parameters", EditorStyles.boldLabel);
        dict[CelestialBodyParamsBase.jnParams.j2.ToString()] = EditorGUILayout.DoubleField("J2", dict[CelestialBodyParamsBase.jnParams.j2.ToString()]);
        dict[CelestialBodyParamsBase.jnParams.j3.ToString()] = EditorGUILayout.DoubleField("J3", dict[CelestialBodyParamsBase.jnParams.j3.ToString()]);
        dict[CelestialBodyParamsBase.jnParams.j4.ToString()] = EditorGUILayout.DoubleField("J4", dict[CelestialBodyParamsBase.jnParams.j4.ToString()]);
        dict[CelestialBodyParamsBase.jnParams.j5.ToString()] = EditorGUILayout.DoubleField("J5", dict[CelestialBodyParamsBase.jnParams.j5.ToString()]);
        dict[CelestialBodyParamsBase.jnParams.j6.ToString()] = EditorGUILayout.DoubleField("J6", dict[CelestialBodyParamsBase.jnParams.j6.ToString()]);
        // End of Dictionary Filling

        ShowOrbitInfoPanel();
    }

    public void ShowOrbitInfoPanel()
    {

        serializedOrbitalParams.FindProperty("showInfoPanel").boolValue = EditorGUILayout.Foldout(serializedOrbitalParams.FindProperty("showInfoPanel").boolValue, "Orbit Info");
        OrbitalParams predictedParam = null;
        if(Application.isPlaying && celestBody.predictor != null) {
            predictedParam = celestBody.predictor.predictedOrbit.param;
        }
        if(serializedOrbitalParams.FindProperty("showInfoPanel").boolValue)
        {
            celestBody.settings.orbitInfoShowPredictedOrbitInfo = EditorGUILayout.Toggle("Show predicted orbit info", celestBody.settings.orbitInfoShowPredictedOrbitInfo);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.DoubleField(new GUIContent("Period", ".s\nOrbital period"), serializedOrbitalParams.FindProperty("period").doubleValue);
            if(celestBody.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
                EditorGUILayout.DoubleField(new GUIContent("Predicted period", ".s\nOrbital period of the predicted orbit"), predictedParam.period);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Shape of the orbit", EditorStyles.boldLabel);
            EditorGUILayout.DoubleField(new GUIContent("ra", "km\nRadius of the aphelion"), serializedOrbitalParams.FindProperty("ra").doubleValue, GUILayout.MaxWidth(Screen.width/2));
            EditorGUILayout.DoubleField(new GUIContent("rp", "km\nRadius of the perihelion"), serializedOrbitalParams.FindProperty("rp").doubleValue, GUILayout.MaxWidth(Screen.width/2));

            EditorGUILayout.DoubleField(new GUIContent("p", "km\nParameter of the conic"), serializedOrbitalParams.FindProperty("p").doubleValue);
            EditorGUILayout.DoubleField(new GUIContent("e", "-\nExcentricity"), serializedOrbitalParams.FindProperty("e").doubleValue);

            EditorGUILayout.DoubleField(new GUIContent("a", "km\nSemi-Major axis"), serializedOrbitalParams.FindProperty("a").doubleValue);
            EditorGUILayout.DoubleField(new GUIContent("b", "km\nSemi-Minor axis"), serializedOrbitalParams.FindProperty("b").doubleValue);
            EditorGUILayout.DoubleField(new GUIContent("c", "km\nDistance focus-origin"), serializedOrbitalParams.FindProperty("c").doubleValue); 

            if(celestBody.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
                EditorGUILayout.LabelField("Shape of the predicted orbit", EditorStyles.boldLabel);
                EditorGUILayout.DoubleField(new GUIContent("ra", "km\nRadius of the predicted aphelion"), predictedParam.ra);
                EditorGUILayout.DoubleField(new GUIContent("rp", "km\nRadius of the predicted perihelion"), predictedParam.rp);

                EditorGUILayout.DoubleField(new GUIContent("p", "km\nParameter of the predicted conic"), predictedParam.p);
                EditorGUILayout.DoubleField(new GUIContent("e", "-\nPredicted excentricity"), predictedParam.e);

                EditorGUILayout.DoubleField(new GUIContent("a", "km\nPredicted semi-Major axis"), predictedParam.a);
                EditorGUILayout.DoubleField(new GUIContent("b", "km\nPredicted semi-Minor axis"), predictedParam.b);
                EditorGUILayout.DoubleField(new GUIContent("c", "km\nPredicted distance focus-origin"), predictedParam.c);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Rotation of the plane of the orbit", EditorStyles.boldLabel);
            EditorGUILayout.DoubleField(new GUIContent("i", "°\nInclination"), serializedOrbitalParams.FindProperty("i").doubleValue);
            EditorGUILayout.DoubleField(new GUIContent("lAscN", "°\nLongitude of the asceding node"), serializedOrbitalParams.FindProperty("lAscN").doubleValue);

            if(celestBody.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
                EditorGUILayout.LabelField("Rotation of the plane of the predicted orbit", EditorStyles.boldLabel);
                EditorGUILayout.DoubleField(new GUIContent("i", "°\nPredicted inclination"), predictedParam.i);
                EditorGUILayout.DoubleField(new GUIContent("lAscN", "°\nPredicted longitude of the asceding node"), predictedParam.lAscN);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Rotation of the orbit in its plane", EditorStyles.boldLabel);
            EditorGUILayout.DoubleField(new GUIContent("omega", "°\nArgument of the perihelion"), serializedOrbitalParams.FindProperty("omega").doubleValue);

            if(celestBody.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
                EditorGUILayout.LabelField("Rotation of the predicted orbit in its plane", EditorStyles.boldLabel);
                EditorGUILayout.DoubleField(new GUIContent("omega", "°\nPredicted argument of the perihelion"), predictedParam.omega);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Position of the body", EditorStyles.boldLabel);
            EditorGUILayout.DoubleField(new GUIContent("nu", "°\nTrue anomaly"), serializedOrbitalParams.FindProperty("nu").doubleValue);
            EditorGUILayout.DoubleField(new GUIContent("m0", "°\nMean anomaly"), serializedOrbitalParams.FindProperty("m0").doubleValue);
            EditorGUILayout.DoubleField(new GUIContent("l0", "°\nMean longitude"), serializedOrbitalParams.FindProperty("l0").doubleValue);
            EditorGUILayout.DoubleField(new GUIContent("t0", ".s\nTime of passage at perihelion"), serializedOrbitalParams.FindProperty("t0").doubleValue);
            
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Vernal Axes", EditorStyles.boldLabel);
            EditorGUILayout.Vector3Field("Vernal point", (Vector3)celestBody.orbitalParams.vp);
            EditorGUILayout.Vector3Field("Vernal right axis", (Vector3)celestBody.orbitalParams.vpAxisRight);
            EditorGUILayout.Vector3Field("Vernal up axis", (Vector3)celestBody.orbitalParams.vpAxisUp);
            EditorGUI.EndDisabledGroup();
        }
    }
}