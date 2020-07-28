using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(Spaceship),true), CanEditMultipleObjects]
public class SpaceshipEditor: Editor
{
    private Spaceship spaceship;
    private SerializedObject serializedOrbitalParams;

    private void OnEnable()
    {
        spaceship = (Spaceship)target;
        CheckCreateOrbitalParamsAsset();
        serializedOrbitalParams = new SerializedObject(serializedObject.FindProperty("_orbitalParams").objectReferenceValue);
    }

    private void CheckCreateOrbitalParamsAsset()
    {
        string orbitalParamsPath = "Assets/Resources" + Filepaths.DEBUG_shipOrbitalParams_0 + spaceship.gameObject.name + Filepaths.DEBUG_shipOrbitalParams_2;
        if(!File.Exists(orbitalParamsPath))
        {
            Debug.Log("Creating a new instance of OrbitalParams at path: '" + orbitalParamsPath + "'");
            OrbitalParams newInstance = ScriptableObject.CreateInstance<OrbitalParams>();
            AssetDatabase.CreateAsset(newInstance, orbitalParamsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        spaceship.orbitalParams = (OrbitalParams)AssetDatabase.LoadAssetAtPath(orbitalParamsPath, typeof(OrbitalParams));
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

        CreateFlyingObjCommonParams();
        CreateSpaceshipSettingsEditor();

        serializedOrbitalParams.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target); 
        }
    }

    private void CreateFlyingObjCommonParams()
    {
        spaceship.settings = (SpaceshipSettings)EditorGUILayout.ObjectField("Settings", spaceship.settings, typeof(SpaceshipSettings), false);
        EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("orbitedBodyName"));
        Show_FlyingObjInfoPanel();
    }

    private void Show_FlyingObjInfoPanel()
    {
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

    private void CreateSpaceshipSettingsEditor()
    {
        if(spaceship.settings != null)
        {
            spaceship.settings.foldoutSpaceshipSettings = EditorGUILayout.InspectorTitlebar(spaceship.settings.foldoutSpaceshipSettings, spaceship.settings);
            using(var check = new EditorGUI.ChangeCheckScope())
            {   
                if(spaceship.settings.foldoutSpaceshipSettings)
                {
                    CreateOrbitalParametersEditor();
                }
            }
        } 
    }

    public void CreateOrbitalParametersEditor()
    {
        spaceship.settings.startFromGround = EditorGUILayout.Toggle("Start from ground", spaceship.settings.startFromGround);

        if(!spaceship.settings.startFromGround)
        {
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

            EditorGUILayout.LabelField("Shape of the orbit", EditorStyles.boldLabel);
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

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Rotation of orbit's plane", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("i"));
            EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("lAscN"));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Rotation of the orbit in its plane", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("omega"));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Object's position on its orbit", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("bodyPosType"));

            switch(serializedOrbitalParams.FindProperty("bodyPosType").intValue)
            {
                case 0:
                    // 'nu'
                    EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("nu"));
                    break;
                case 1:
                    // 'm0'
                    EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("m0"));
                    break;
                case 2:
                    // 'l0'
                    EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("l0"));
                    break;
                case 3:
                    // 't0'
                    EditorGUILayout.PropertyField(serializedOrbitalParams.FindProperty("t0"));
                    break;
            }
            ShowOrbitInfoPanel();
        }
    }

    public void ShowOrbitInfoPanel()
    {
        spaceship.orbitalParams.showInfoPanel = EditorGUILayout.Foldout(spaceship.orbitalParams.showInfoPanel, "Orbit Info");
        OrbitalParams predictedParam = null;
        if(Application.isPlaying && spaceship.predictor != null) {
            predictedParam = spaceship.predictor.predictedOrbit.param;
        }
        else {
            predictedParam = null;
        }
        if(serializedOrbitalParams.FindProperty("showInfoPanel").boolValue)
        {
            spaceship.settings.orbitInfoShowPredictedOrbitInfo = EditorGUILayout.Toggle("Show predicted orbit info", spaceship.settings.orbitInfoShowPredictedOrbitInfo);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.DoubleField(new GUIContent("Period", ".s\nOrbital period"), serializedOrbitalParams.FindProperty("period").doubleValue);
            if(spaceship.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
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
            EditorGUILayout.DoubleField(new GUIContent("i", "°\nInclination"), serializedOrbitalParams.FindProperty("i").doubleValue);
            EditorGUILayout.DoubleField(new GUIContent("lAscN", "°\nLongitude of the asceding node"), serializedOrbitalParams.FindProperty("lAscN").doubleValue);

            if(spaceship.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
                EditorGUILayout.LabelField("Rotation of the plane of the predicted orbit", EditorStyles.boldLabel);
                EditorGUILayout.DoubleField(new GUIContent("i", "°\nPredicted inclination"), predictedParam.i);
                EditorGUILayout.DoubleField(new GUIContent("lAscN", "°\nPredicted longitude of the asceding node"), predictedParam.lAscN);
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Rotation of the orbit in its plane", EditorStyles.boldLabel);
            EditorGUILayout.DoubleField(new GUIContent("omega", "°\nArgument of the perihelion"), serializedOrbitalParams.FindProperty("omega").doubleValue);

            if(spaceship.settings.orbitInfoShowPredictedOrbitInfo && predictedParam!=null) {
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
            EditorGUILayout.Vector3Field("Vernal point", (Vector3)spaceship.orbitalParams.vp);
            EditorGUILayout.Vector3Field("Vernal right axis", (Vector3)spaceship.orbitalParams.vpAxisRight);
            EditorGUILayout.Vector3Field("Vernal up axis", (Vector3)spaceship.orbitalParams.vpAxisUp);
            EditorGUI.EndDisabledGroup();
        }
    }
}



