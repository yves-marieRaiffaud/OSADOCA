using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Mathd_Lib;

[CustomEditor(typeof(CelestialBody),true), CanEditMultipleObjects]
public class CelestialBodyEditor : Editor
{
    CelestialBody celestBody;

    private void OnEnable()
    {
        celestBody = (CelestialBody)target;
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
        DrawCelestialBodySettingsEditor(celestBody.settings, ref celestBody.settings.bodySettingsEditorFoldout);
    }

    private void DrawCelestialBodySettingsEditor(Object settings, ref bool foldout)
    {
        if(settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using(var check = new EditorGUI.ChangeCheckScope())
            {   
                if(foldout)
                {
                    CreateCelestialBodySettingsEditor(settings);
                }
            }
        } 
    }

    private void CreateCelestialBodySettingsEditor(Object settings)
    {
        CelestialBodySettings param = (CelestialBodySettings) settings;
        //==========================================================================
        param.bodyMaterial = (Material)EditorGUILayout.ObjectField("Body material", param.bodyMaterial, typeof(Material), false);
        if(celestBody.spawnAsSimpleSphere)
        {
            // Only add the predifined planet selection
            if(CelestialBody.CelestialBodyHasTagName(celestBody, "Planet"))
            {
                if(celestBody.orbitalParams == null)
                {
                    celestBody.orbitalParams = (OrbitalParams)OrbitalParams.CreateInstance("OrbitalParams");
                }
                param.usePredifinedPlanets = true;
                EditorGUI.BeginDisabledGroup(true);
                param.usePredifinedPlanets = EditorGUILayout.Toggle("Use a predifined planet for its orbit", param.usePredifinedPlanets);
                EditorGUI.EndDisabledGroup();
                param.chosenPredifinedPlanet = (UniCsts.planets)EditorGUILayout.EnumPopup("Choose planet", param.chosenPredifinedPlanet);
                switch(param.chosenPredifinedPlanet)
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
            return;
        }

        param.heightMap = (Texture2D)EditorGUILayout.ObjectField("Height map", param.heightMap, typeof(Texture2D), false);
        celestBody.showCelestialBodyInfoPanel = EditorGUILayout.Foldout(celestBody.showCelestialBodyInfoPanel, "CelestialBody Info");
        if(celestBody.showCelestialBodyInfoPanel)
        {
            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.LabelField("Position vectors");
            EditorGUILayout.Vector3Field(new GUIContent("Relative Acc", "m.s-2\nStar relative acceleration"), (Vector3)celestBody.orbitedBodyRelativeAcc);
            EditorGUILayout.Vector3Field(new GUIContent("Relative Velocity Incr", "m.s\nStar relative velocity increment"), (Vector3)celestBody.orbitedBodyRelativeVelIncr);
            EditorGUILayout.Vector3Field(new GUIContent("Relative Velocity", "m.s\nStar relative velocity"), (Vector3)celestBody.orbitedBodyRelativeVel);
            EditorGUILayout.Vector3Field(new GUIContent("Universe Position", "Position in the universe, global position"), (Vector3)celestBody.realPosition);
            EditorGUILayout.Separator();

            EditorGUILayout.DoubleField("RadiusU", param.radiusU);
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.Separator();

        //==========================================================================
        if(CelestialBody.CelestialBodyHasTagName(celestBody, "Star"))
        {
            celestBody.orbitedBody = null;
            celestBody.orbitalParams = null;
            celestBody.settings.usePredifinedPlanets = true;
            celestBody.settings.chosenPredifinedPlanet = UniCsts.planets.Sun;
            EditorGUI.BeginDisabledGroup(true);
            celestBody.settings.chosenPredifinedPlanet = (UniCsts.planets)EditorGUILayout.EnumPopup("Choose planet", param.chosenPredifinedPlanet);
            EditorGUI.EndDisabledGroup();
        }
        else if(CelestialBody.CelestialBodyHasTagName(celestBody, "Planet"))
        {
            if(celestBody.orbitalParams == null)
            {
                celestBody.orbitalParams = (OrbitalParams)OrbitalParams.CreateInstance("OrbitalParams");
            }
            celestBody.orbitedBody = (CelestialBody)EditorGUILayout.ObjectField("Orbited Body", celestBody.orbitedBody, typeof(CelestialBody), true);
            param.usePredifinedPlanets = EditorGUILayout.Toggle("Use a predifined planet for its orbit", param.usePredifinedPlanets);
            if(param.usePredifinedPlanets)
            {
                param.chosenPredifinedPlanet = (UniCsts.planets)EditorGUILayout.EnumPopup("Choose planet", param.chosenPredifinedPlanet);
                switch(param.chosenPredifinedPlanet)
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
                celestBody.orbitalParams.orbitDefType = OrbitalParams.orbitDefinitionType.rarp;
                celestBody.orbitalParams.orbitRealPredType = OrbitalParams.typeOfOrbit.realOrbit;
                celestBody.orbitalParams.orbParamsUnits = OrbitalParams.orbitalParamsUnits.AU_degree;
                celestBody.orbitalParams.drawOrbit = EditorGUILayout.Toggle("Draw orbit", celestBody.orbitalParams.drawOrbit);
                celestBody.orbitalParams.orbitDrawingResolution = EditorGUILayout.IntSlider("Orbit Resolution", celestBody.orbitalParams.orbitDrawingResolution, 5, 500);
                celestBody.orbitalParams.drawDirections = EditorGUILayout.Toggle("Draw directions", celestBody.orbitalParams.drawDirections);

                EditorGUI.BeginDisabledGroup(!celestBody.orbitalParams.drawDirections);
                celestBody.orbitalParams.selectedVectorsDir = (OrbitalParams.typeOfVectorDir)EditorGUILayout.EnumFlagsField("Direction vectors to draw", celestBody.orbitalParams.selectedVectorsDir);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Separator();
                ShowOrbitInfoPanel(celestBody.orbitalParams);
            }
            else{
                if(celestBody.settings.planetBaseParamsDict == null)
                {
                    celestBody.settings.planetBaseParamsDict = InitNewPlanetBaseParamsDict();
                }
                CreateOrbitalParametersEditor(celestBody.settings.planetBaseParamsDict);
            }
        }
    }

    private void PredifinedPlanetEditorGUI()
    {
        
    }

    public Dictionary<string, double> InitNewPlanetBaseParamsDict()
    {
        return new Dictionary<string, double> {
            { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 0d },
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

        };
    }

    public void CreateOrbitalParametersEditor(Dictionary<string, double> dict)
    {
        // Dictionary Filling
        EditorGUILayout.LabelField("Planetary Params", EditorStyles.boldLabel);
        dict[CelestialBodyParamsBase.planetaryParams.radius.ToString()] = EditorGUILayout.DoubleField("Radius", dict[CelestialBodyParamsBase.planetaryParams.radius.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()] = EditorGUILayout.DoubleField("Inverse flattening", dict[CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()] = EditorGUILayout.DoubleField("SOI Radius", dict[CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()] = EditorGUILayout.DoubleField("Axial tilt", dict[CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()] = EditorGUILayout.DoubleField("Sidereal Rot period", dict[CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()]);
        dict[CelestialBodyParamsBase.planetaryParams.mu.ToString()] = EditorGUILayout.DoubleField("µ", dict[CelestialBodyParamsBase.planetaryParams.mu.ToString()]);

        EditorGUILayout.Separator();
        // ORBITAL PARAMETERS
        EditorGUILayout.LabelField("Rendering parameters", EditorStyles.boldLabel);
        celestBody.orbitalParams.drawOrbit = EditorGUILayout.Toggle("Draw orbit", celestBody.orbitalParams.drawOrbit);
        celestBody.orbitalParams.orbitDrawingResolution = EditorGUILayout.IntSlider("Orbit Resolution", celestBody.orbitalParams.orbitDrawingResolution, 5, 500);
        celestBody.orbitalParams.drawDirections = EditorGUILayout.Toggle("Draw directions", celestBody.orbitalParams.drawDirections);
        EditorGUI.BeginDisabledGroup(!celestBody.orbitalParams.drawDirections);
        celestBody.orbitalParams.selectedVectorsDir = (OrbitalParams.typeOfVectorDir)EditorGUILayout.EnumFlagsField("Direction vectors to draw", celestBody.orbitalParams.selectedVectorsDir);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Separator();
        celestBody.orbitalParams.orbitRealPredType = (OrbitalParams.typeOfOrbit)EditorGUILayout.EnumPopup("Type of orbit", celestBody.orbitalParams.orbitRealPredType);
        celestBody.orbitalParams.orbParamsUnits = (OrbitalParams.orbitalParamsUnits)EditorGUILayout.EnumPopup("Parameters' units", celestBody.orbitalParams.orbParamsUnits);
        EditorGUI.BeginDisabledGroup(true);
        celestBody.orbitalParams.orbitDefType = (OrbitalParams.orbitDefinitionType)EditorGUILayout.EnumPopup("Orbit Definition Type", celestBody.orbitalParams.orbitDefType);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField("Orbital Parameters", EditorStyles.boldLabel);
        dict[CelestialBodyParamsBase.orbitalParams.aphelion.ToString()] = EditorGUILayout.DoubleField("Aphelion", dict[CelestialBodyParamsBase.orbitalParams.aphelion.ToString()]);
        dict[CelestialBodyParamsBase.orbitalParams.perihelion.ToString()] = EditorGUILayout.DoubleField("Perihelion", dict[CelestialBodyParamsBase.orbitalParams.perihelion.ToString()]);
        dict[CelestialBodyParamsBase.orbitalParams.i.ToString()] = EditorGUILayout.DoubleField("i", dict[CelestialBodyParamsBase.orbitalParams.i.ToString()]);
        dict[CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()] = EditorGUILayout.DoubleField("Longitude of the ascending node", dict[CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()]);
        dict[CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()] = EditorGUILayout.DoubleField("Argument of the perihelion", dict[CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()]);
        dict[CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()] = EditorGUILayout.DoubleField("True anomaly", dict[CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()]);

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

        ShowOrbitInfoPanel(celestBody.orbitalParams);
    }

    public void ShowOrbitInfoPanel(OrbitalParams param)
    {
        param.showInfoPanel = EditorGUILayout.Foldout(param.showInfoPanel, "Orbit Info");
        OrbitalParams predictedParam = null;
        if(Application.isPlaying && celestBody.predictor != null) {
            predictedParam = celestBody.predictor.predictedOrbit.param;
        }
        if(param.showInfoPanel)
        {
            celestBody.settings.orbitInfoShowPredictedOrbitInfo = EditorGUILayout.Toggle("Show predicted orbit info", celestBody.settings.orbitInfoShowPredictedOrbitInfo);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.DoubleField(new GUIContent("Period", ".s\nOrbital period"), param.period);
            if(celestBody.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
                EditorGUILayout.DoubleField(new GUIContent("Predicted period", ".s\nOrbital period of the predicted orbit"), predictedParam.period);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Shape of the orbit", EditorStyles.boldLabel);
            EditorGUILayout.DoubleField(new GUIContent("ra", "km\nRadius of the aphelion"), param.ra, GUILayout.MaxWidth(Screen.width/2));
            EditorGUILayout.DoubleField(new GUIContent("rp", "km\nRadius of the perihelion"), param.rp, GUILayout.MaxWidth(Screen.width/2));

            EditorGUILayout.DoubleField(new GUIContent("p", "km\nParameter of the conic"), param.p);
            EditorGUILayout.DoubleField(new GUIContent("e", "-\nExcentricity"), param.e);

            EditorGUILayout.DoubleField(new GUIContent("a", "km\nSemi-Major axis"), param.a);
            EditorGUILayout.DoubleField(new GUIContent("b", "km\nSemi-Minor axis"), param.b);
            EditorGUILayout.DoubleField(new GUIContent("c", "km\nDistance focus-origin"), param.c); 

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
            EditorGUILayout.DoubleField(new GUIContent("i", "°\nInclination"), param.i);
            EditorGUILayout.DoubleField(new GUIContent("lAscN", "°\nLongitude of the asceding node"), param.lAscN);

            if(celestBody.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
                EditorGUILayout.LabelField("Rotation of the plane of the predicted orbit", EditorStyles.boldLabel);
                EditorGUILayout.DoubleField(new GUIContent("i", "°\nPredicted inclination"), predictedParam.i);
                EditorGUILayout.DoubleField(new GUIContent("lAscN", "°\nPredicted longitude of the asceding node"), predictedParam.lAscN);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Rotation of the orbit in its plane", EditorStyles.boldLabel);
            EditorGUILayout.DoubleField(new GUIContent("omega", "°\nArgument of the perihelion"), param.omega);

            if(celestBody.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
                EditorGUILayout.LabelField("Rotation of the predicted orbit in its plane", EditorStyles.boldLabel);
                EditorGUILayout.DoubleField(new GUIContent("omega", "°\nPredicted argument of the perihelion"), predictedParam.omega);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Position of the body", EditorStyles.boldLabel);
            EditorGUILayout.DoubleField(new GUIContent("nu", "°\nTrue anomaly"), param.nu);
            EditorGUILayout.DoubleField(new GUIContent("m0", "°\nMean anomaly"), param.m0);
            EditorGUILayout.DoubleField(new GUIContent("l0", "°\nMean longitude"), param.l0);
            EditorGUILayout.DoubleField(new GUIContent("t0", ".s\nTime of passage at perihelion"), param.t0);
            
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Vernal Axes", EditorStyles.boldLabel);
            EditorGUILayout.Vector3Field("Vernal point", (Vector3)param.vp);
            EditorGUILayout.Vector3Field("Vernal right axis", (Vector3)param.vpAxisRight);
            EditorGUILayout.Vector3Field("Vernal up axis", (Vector3)param.vpAxisUp);
            EditorGUI.EndDisabledGroup();
        }
    }
}