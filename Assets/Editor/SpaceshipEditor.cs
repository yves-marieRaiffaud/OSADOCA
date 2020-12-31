using UnityEngine;
using UnityEditor;
using System.IO;
using UniCsts = UniverseConstants;

[CustomEditor(typeof(Spaceship),true)]
public class SpaceshipEditor : Editor
{
    Spaceship ship;
    string shipOrbitalParamsFilePath;
    //SerializedObject orbParamsSerializedObjs;
    bool orbDataFoldoutBool;

    void OnEnable()
    {
        ship = (Spaceship)target;
        shipOrbitalParamsFilePath = Application.persistentDataPath + Filepaths.shipToLoad_orbitalParams;
        CheckCreate_OrbitalParams_SO();
        orbDataFoldoutBool = ship.orbitalParams.orbDataFoldoutBool;
    }
    void CheckCreate_OrbitalParams_SO()
    {
        if(ship.orbitalParams == null || !File.Exists(shipOrbitalParamsFilePath)) {
            ship.orbitalParams = ScriptableObject.CreateInstance<OrbitalParams>();
            OrbitalParams.WriteToFileOrbitalParamsSaveData(ship.orbitalParams, shipOrbitalParamsFilePath);
        }
        else if(!Application.isPlaying) {
            JsonUtility.FromJsonOverwrite(File.ReadAllText(shipOrbitalParamsFilePath), ship.orbitalParams);
        }
    }

    void OnInspectorUpdate()
    {
        this.Repaint();
    }
    public override void OnInspectorGUI()
    {
        //orbParamsSerializedObjs.Update();
        if (!EditorGUIUtility.wideMode)
            EditorGUIUtility.wideMode = true;

        using(var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
        }

        if(!ship.spawnAs_UI_SC) {
            Draw_OrbitalParams_Editor(ship.orbitalParams);
            Handle_Info_Panel_Foldout();
        }
        //orbParamsSerializedObjs.ApplyModifiedProperties();
    }

    void Draw_OrbitalParams_Editor(OrbitalParams orbParams)
    {
        if(orbParams != null) {
            orbParams.orbDataFoldoutBool = EditorGUILayout.InspectorTitlebar(orbParams.orbDataFoldoutBool, orbParams);
            using(var check = new EditorGUI.ChangeCheckScope()) {
                if(orbParams.orbDataFoldoutBool)
                    Create_OrbitalParams_Editor(orbParams);
            }
        }
        if(GUI.changed) {
            string filepath = OrbitalParams.WriteToFileOrbitalParamsSaveData(ship.orbitalParams, shipOrbitalParamsFilePath);
            Debug.Log("OrbitalParams successfully saved at: '" + shipOrbitalParamsFilePath + "'.");
        }
    }
    void Create_OrbitalParams_Editor(OrbitalParams orbParams)
    {
        //orbParams.orbitedBody = (CelestialBody)EditorGUILayout.ObjectField("Orbited body", orbParams.orbitedBody, typeof(CelestialBody), true);
        orbParams.orbitedBodyName = EditorGUILayout.TextField("Orbited body name", orbParams.orbitedBodyName);
        orbParams.orbitDefType = (OrbitalTypes.orbitDefinitionType)EditorGUILayout.EnumPopup("Orbit definition type", orbParams.orbitDefType);
        orbParams.bodyPosType = (OrbitalTypes.bodyPositionType)EditorGUILayout.EnumPopup("Reference angle on its orbit", orbParams.bodyPosType);
        orbParams.orbParamsUnits = (OrbitalTypes.orbitalParamsUnits)EditorGUILayout.EnumPopup("Orbit units", orbParams.orbParamsUnits);
        EditorGUI.BeginDisabledGroup(true);
        orbParams.orbitRealPredType = (OrbitalTypes.typeOfOrbit)EditorGUILayout.EnumPopup("Type of orbit", orbParams.orbitRealPredType);
        orbParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.realOrbit;
        EditorGUI.EndDisabledGroup();

        string distUnit = "(km)";
        if(orbParams.orbParamsUnits.Equals(OrbitalTypes.orbitalParamsUnits.AU))
            distUnit = "(AU)";

        orbParams.selectedVectorsDir = (OrbitalTypes.typeOfVectorDir)EditorGUILayout.EnumFlagsField("Orbital directions to draw", orbParams.selectedVectorsDir);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Rendering Parameters", EditorStyles.boldLabel);
        orbParams.drawOrbit = EditorGUILayout.Toggle("Render orbit", orbParams.drawOrbit);
        orbParams.drawDirections = EditorGUILayout.Toggle("Render selected directions", orbParams.drawDirections);
        orbParams.orbitDrawingResolution = EditorGUILayout.IntField("Rendering resolution", orbParams.orbitDrawingResolution);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Shape of the Orbit", EditorStyles.boldLabel);
        switch(orbParams.orbitDefType)
        {
            case OrbitalTypes.orbitDefinitionType.rarp:
                orbParams.ra = EditorGUILayout.DoubleField("ra "+distUnit, orbParams.ra);
                orbParams.rp = EditorGUILayout.DoubleField("rp "+distUnit, orbParams.rp);
                break;
            case OrbitalTypes.orbitDefinitionType.rpe:
                orbParams.rp = EditorGUILayout.DoubleField("rp "+distUnit, orbParams.rp);
                orbParams.e = EditorGUILayout.DoubleField("e", orbParams.e);
                break;
            case OrbitalTypes.orbitDefinitionType.pe:
                orbParams.p = EditorGUILayout.DoubleField("p "+distUnit, orbParams.p);
                orbParams.e = EditorGUILayout.DoubleField("e", orbParams.e);
                break;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Rotation of the Plane of the Orbit", EditorStyles.boldLabel);
        orbParams.i = EditorGUILayout.DoubleField("i (rad)", orbParams.i);
        orbParams.lAscN = EditorGUILayout.DoubleField("Longitude of the Ascending Node (rad)", orbParams.lAscN);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Rotation of the Orbit in its Plane", EditorStyles.boldLabel);
        orbParams.omega = EditorGUILayout.DoubleField("Argument of the periapsis (rad)", orbParams.omega);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Position on the orbit", EditorStyles.boldLabel);
        switch(orbParams.bodyPosType)
        {
            case OrbitalTypes.bodyPositionType.nu:
                orbParams.nu = EditorGUILayout.DoubleField("True Anomaly (rad)", orbParams.nu);
                break;
            case OrbitalTypes.bodyPositionType.M:
                orbParams.M = EditorGUILayout.DoubleField("Mean Anomaly (rad)", orbParams.M);
                break;
            case OrbitalTypes.bodyPositionType.E:
                orbParams.E = EditorGUILayout.DoubleField("Eccentric Anomaly (rad)", orbParams.E);
                break;
            case OrbitalTypes.bodyPositionType.L:
                orbParams.L = EditorGUILayout.DoubleField("Mean Longitude (rad)", orbParams.L);
                break;
            case OrbitalTypes.bodyPositionType.t:
                orbParams.t = EditorGUILayout.DoubleField("Time at perihelion passage (s)", orbParams.t);
                break;
        }
    }

    void Handle_Info_Panel_Foldout()
    {
        ship.inspectorFoldout_showInfo = EditorGUILayout.Foldout(ship.inspectorFoldout_showInfo, "Show info");
        if(!ship.inspectorFoldout_showInfo)
            return;
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_relativeAcc"), new GUIContent("Relative Acc (km/s)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_relativeVel"), new GUIContent("Absolute Velocity (km/s)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_realPosition"), new GUIContent("Unity World Position"));
        EditorGUI.EndDisabledGroup();
    }
}