using UnityEngine;
using UnityEditor;
using Mathd_Lib;

[CustomEditor(typeof(Spaceship),true), CanEditMultipleObjects]
public class SpaceshipEditor: Editor
{
    Spaceship spaceship;

    private void OnEnable()
    {
        spaceship = (Spaceship)target;
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
        CreateFlyingObjCommonParams();
        CreateSpaceshipSettingsEditor(ref spaceship.settings.foldoutSpaceshipSettings);
    }

    private void CreateFlyingObjCommonParams()
    {
        spaceship.settings = (SpaceshipSettings)EditorGUILayout.ObjectField("Settings", spaceship.settings, typeof(SpaceshipSettings), false);
        spaceship.orbitedBody = (CelestialBody)EditorGUILayout.ObjectField("Orbited body", spaceship.orbitedBody, typeof(CelestialBody), true);
        spaceship.orbitalParams = (OrbitalParams)EditorGUILayout.ObjectField("Orbital params", spaceship.orbitalParams, typeof(OrbitalParams), false);

        spaceship.foldoutFlyingObjInfoPanel = EditorGUILayout.Foldout(spaceship.foldoutFlyingObjInfoPanel, "Info");
        EditorGUI.BeginDisabledGroup(true);
        if(spaceship.foldoutFlyingObjInfoPanel)
        {
            EditorGUILayout.Vector3Field(new GUIContent("Relative Acc", "m.s-2"), (Vector3)spaceship.orbitedBodyRelativeAcc);
            EditorGUILayout.Vector3Field(new GUIContent("Relative Velocity Incr", "m/s"), (Vector3)spaceship.orbitedBodyRelativeVelIncr);
            EditorGUILayout.Vector3Field(new GUIContent("Relative Velocity", "m/s"), (Vector3)spaceship.orbitedBodyRelativeVel);
            EditorGUILayout.Vector3Field(new GUIContent("Real position", "Unit to determine"), (Vector3)spaceship.realPosition);
        }
        EditorGUI.EndDisabledGroup();
    }

    private void CreateSpaceshipSettingsEditor(ref bool foldout)
    {
        if(spaceship.settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, spaceship.settings);
            using(var check = new EditorGUI.ChangeCheckScope())
            {   
                if(foldout)
                {
                    CreateOrbitalParametersEditor();
                }
            }
        } 
    }

    public void CreateOrbitalParametersEditor()
    {
        OrbitalParams param = spaceship.orbitalParams;
        if(param != null)
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
    }

    public void ShowOrbitInfoPanel(OrbitalParams param)
    {
        param.showInfoPanel = EditorGUILayout.Foldout(param.showInfoPanel, "Orbit Info");
        OrbitalParams predictedParam = null;
        /*if(Application.isPlaying && spaceship.predictor != null) {
            predictedParam = spaceship.predictor.predictedOrbit.param;
        }
        else {
            predictedParam = null;
        }*/
        if(param.showInfoPanel)
        {
            spaceship.settings.orbitInfoShowPredictedOrbitInfo = EditorGUILayout.Toggle("Show predicted orbit info", spaceship.settings.orbitInfoShowPredictedOrbitInfo);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.DoubleField(new GUIContent("Period", ".s\nOrbital period"), param.period);
            if(spaceship.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
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

            if(spaceship.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
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

            if(spaceship.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
                EditorGUILayout.LabelField("Rotation of the plane of the predicted orbit", EditorStyles.boldLabel);
                EditorGUILayout.DoubleField(new GUIContent("i", "°\nPredicted inclination"), predictedParam.i);
                EditorGUILayout.DoubleField(new GUIContent("lAscN", "°\nPredicted longitude of the asceding node"), predictedParam.lAscN);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Rotation of the orbit in its plane", EditorStyles.boldLabel);
            EditorGUILayout.DoubleField(new GUIContent("omega", "°\nArgument of the perihelion"), param.omega);

            if(spaceship.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
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



