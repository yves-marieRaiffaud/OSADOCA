using UnityEngine;
using UnityEditor;
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

        param.mu = EditorGUILayout.DoubleField("µ", param.mu);
        param.radius = EditorGUILayout.DoubleField("Radius", param.radius);
        param.equatorialPlane.forwardVec = new Vector3d(EditorGUILayout.Vector3Field("Forward Vector", (Vector3)param.equatorialPlane.forwardVec));
        param.equatorialPlane.rightVec = new Vector3d(EditorGUILayout.Vector3Field("Right Vector", (Vector3)param.equatorialPlane.rightVec));
        param.equatorialPlane.point = new Vector3d(EditorGUILayout.Vector3Field("Point", (Vector3)param.equatorialPlane.point));
        param.equatorialPlane.normal = new Vector3d(EditorGUILayout.Vector3Field("Normal Vector", (Vector3)param.equatorialPlane.normal));
        param.soiRadius = EditorGUILayout.DoubleField("SOI radius", param.soiRadius);
        param.rotationSpeed = EditorGUILayout.DoubleField("Rotation speed", param.rotationSpeed);
        param.rotationAxis = new Vector3d(EditorGUILayout.Vector3Field("Rotation axis", (Vector3)param.rotationAxis));
        param.planetRefreshPeriod = EditorGUILayout.Slider("Planet refresh period", param.planetRefreshPeriod, 0.1f, 2f);
        param.cullingMinAngle = EditorGUILayout.Slider("Cullin min angle", param.cullingMinAngle, 0.1f, 2f);

        param.bodyMaterial = (Material)EditorGUILayout.ObjectField("Body material", param.bodyMaterial, typeof(Material), true);

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

        if(CelestialBody.CelestialBodyHasTagName(celestBody, "Star"))
        {
            celestBody.orbitedBody = null;
            celestBody.orbitalParams = null;
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
                        celestBody.settings.refDictOrbitalParams = UniCsts.mercuryOrbitalParams;
                        break;
                    
                    case UniCsts.planets.Venus:
                        celestBody.settings.refDictOrbitalParams = UniCsts.venusOrbitalParams;
                        break;

                    case UniCsts.planets.Earth:
                        celestBody.settings.refDictOrbitalParams = UniCsts.earthOrbitalParams;
                        break;

                    case UniCsts.planets.Mars:
                        celestBody.settings.refDictOrbitalParams = UniCsts.marsOrbitalParams;
                        break;
                    
                    case UniCsts.planets.Jupiter:
                        celestBody.settings.refDictOrbitalParams = UniCsts.jupiterOrbitalParams;
                        break;
                    
                    case UniCsts.planets.Saturn:
                        celestBody.settings.refDictOrbitalParams = UniCsts.saturnOrbitalParams;
                        break;
                    
                    case UniCsts.planets.Uranus:
                        celestBody.settings.refDictOrbitalParams = UniCsts.uranusOrbitalParams;
                        break;
                    
                    case UniCsts.planets.Neptune:
                        celestBody.settings.refDictOrbitalParams = UniCsts.neptuneOrbitalParams;
                        break;
                }
                EditorGUILayout.LabelField("Rendering parameters", EditorStyles.boldLabel);
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
                CreateOrbitalParametersEditor(celestBody.orbitalParams);
            }
        }
    }

    public void CreateOrbitalParametersEditor(OrbitalParams param)
    {
        EditorGUILayout.LabelField("Rendering parameters", EditorStyles.boldLabel);
        param.drawOrbit = EditorGUILayout.Toggle("Draw orbit", param.drawOrbit);
        param.orbitDrawingResolution = EditorGUILayout.IntSlider("Orbit Resolution", param.orbitDrawingResolution, 5, 500);
        param.drawDirections = EditorGUILayout.Toggle("Draw directions", param.drawDirections);
        
        EditorGUI.BeginDisabledGroup(!param.drawDirections);
        param.selectedVectorsDir = (OrbitalParams.typeOfVectorDir)EditorGUILayout.EnumFlagsField("Direction vectors to draw", param.selectedVectorsDir);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Separator();

        param.orbitRealPredType = (OrbitalParams.typeOfOrbit)EditorGUILayout.EnumPopup("Type of orbit", param.orbitRealPredType);
        param.orbParamsUnits = (OrbitalParams.orbitalParamsUnits)EditorGUILayout.EnumPopup("Parameters' units", param.orbParamsUnits);
        param.orbitDefType = (OrbitalParams.orbitDefinitionType)EditorGUILayout.EnumPopup("Orbit Definition Type", param.orbitDefType);
        EditorGUILayout.LabelField("Shape of the orbit", EditorStyles.boldLabel);
        switch(param.orbitDefType)
        {
            case OrbitalParams.orbitDefinitionType.rarp:
                param.ra = EditorGUILayout.DoubleField("ra", param.ra);
                param.rp = EditorGUILayout.DoubleField("rp", param.rp);
                break;
            case OrbitalParams.orbitDefinitionType.rpe:
                param.rp = EditorGUILayout.DoubleField("rp", param.rp);
                param.e = EditorGUILayout.DoubleField("e", param.e);
                break;
            case OrbitalParams.orbitDefinitionType.pe:
                param.p = EditorGUILayout.DoubleField("p", param.p);
                param.e = EditorGUILayout.DoubleField("e", param.e);
                break;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Rotation of orbit's plane", EditorStyles.boldLabel);
        param.i = EditorGUILayout.DoubleField("Inclination", param.i);
        param.lAscN = EditorGUILayout.DoubleField("Longitude of the ascending Node", param.lAscN);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Rotation of the orbit in its plane", EditorStyles.boldLabel);
        param.omega = EditorGUILayout.DoubleField("Perihelion's argument", param.omega);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Object's position on its orbit", EditorStyles.boldLabel);
        param.bodyPosType = (OrbitalParams.bodyPositionType)EditorGUILayout.EnumPopup("Body Position Type", param.bodyPosType);

        switch(param.bodyPosType)
        {
            case OrbitalParams.bodyPositionType.l0:
                param.l0 = EditorGUILayout.DoubleField("Mean longitude", param.l0);
                break;
            case OrbitalParams.bodyPositionType.m0:
                param.m0 = EditorGUILayout.DoubleField("Mean anomaly", param.m0);
                break;
            case OrbitalParams.bodyPositionType.nu:
                param.nu = EditorGUILayout.DoubleField("True anomaly", param.nu);
                break;
            case OrbitalParams.bodyPositionType.t0:
                param.t0 = EditorGUILayout.DoubleField("Perihelion time of passage", param.t0);
                break;
        }

        ShowOrbitInfoPanel(param);
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