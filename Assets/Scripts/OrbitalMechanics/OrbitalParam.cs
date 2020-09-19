using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mathd_Lib;
using Fncs = UsefulFunctions;

[CreateAssetMenu()]
public class OrbitalParams : ScriptableObject
{
    public string orbitedBodyName;
    public CelestialBody orbitedBody;

    public OrbitalTypes.orbitDefinitionType orbitDefType; // Only in the custom editor
    public OrbitalTypes.bodyPositionType bodyPosType; // Only in the custom editor
    public OrbitalTypes.orbitalParamsUnits orbParamsUnits; 
    public OrbitalTypes.typeOfOrbit orbitRealPredType; // Only in the custom editor
    public OrbitalTypes.typeOfVectorDir selectedVectorsDir=(OrbitalTypes.typeOfVectorDir)0; // Selected vectors to draw, only in the custom editor

    public bool drawOrbit=true;
    public bool drawDirections=false;
    public int orbitDrawingResolution=300;

    //==============
    // Shape of the orbit
    // Units of the parameters depend on the enum 'orbitalParamsUnits'
    public double ra; // km or AU, Radius of the aphelion: from the centre of the body to the aphelion of its orbit
    public double rp; // km or AU
    public double p; // km or AU
    public double e; // no unit
    public double a; // Semi-Major Axis, km or AU
    public double b; // Semi-Minor Axis, km or AU
    public double c; // Distance focus-centre of the ellispe, km or AU

    // Rotation of the plane of the orbit
    public double i; // Inclination, degree
    public double lAscN; // Longitude of the ascending node, degree
    
    // Rotation of the orbit in its plane
    public double omega; // Argument of the perihelion, degree

    // Position of the body on the ellipse
    public double nu; // True anomaly, degree
    public double M; // Mean Anomaly, degree
    public double E; // Eccentric anomaly, degree
    public double L; // Mean Longitude, degree
    public double t; // Time at perihelion passage
    //==============
    public double period; // Orbtial period, seconds
    //==============
    public Vector3d vp; // Vernal Point
    public Vector3d vpAxisRight; // Perpendicular vector of vp
    public Vector3d vpAxisUp; // Perpendicualr vector of vp and vpAxisRight
    public Vector3d ascendingNodeLineDir; // Ascending Nodes Line
    public Vector3d apogeeLineDir; // Apsides Line

    public OrbitPlane orbitPlane;
    //==============
    public Dictionary <OrbitalTypes.typeOfVectorDir, string> suffixVectorDir = new Dictionary<OrbitalTypes.typeOfVectorDir, string>(){
        {OrbitalTypes.typeOfVectorDir.vernalPoint,       "VP_"},
        {OrbitalTypes.typeOfVectorDir.vpAxisRight,       "VPAxisRight_"},
        {OrbitalTypes.typeOfVectorDir.vpAxisUp,          "VPAxisUp_"},
        {OrbitalTypes.typeOfVectorDir.apogeeLine,        "Apsides_"},
        {OrbitalTypes.typeOfVectorDir.ascendingNodeLine, "Nodes_"},
        {OrbitalTypes.typeOfVectorDir.tangentialVec, "Tangential_"},
        {OrbitalTypes.typeOfVectorDir.radialVec, "Radial_"},
        {OrbitalTypes.typeOfVectorDir.velocityVec, "Velocity_"},
        {OrbitalTypes.typeOfVectorDir.accelerationVec, "Acceleration_"}
    };
    
    public Dictionary <OrbitalTypes.typeOfOrbit, string> suffixOrbitType = new Dictionary<OrbitalTypes.typeOfOrbit, string>(){
        {OrbitalTypes.typeOfOrbit.realOrbit,      "Real"},
        {OrbitalTypes.typeOfOrbit.predictedOrbit, "Predicted"}
    };
    //==============
    public bool showInfoPanel=false;

    public struct OrbitalStateVector {
        ReferenceFrame frame;
        Vector3d r;
        Vector3d v;
        //Epoch epoch;

        public OrbitalStateVector(ReferenceFrame _frame, Vector3d _r, Vector3d _v/*, Epoch _epoch*/) {
            frame = _frame;
            r = _r;
            v = _v;
            //epoch = _epoch;
        }
    }
}

[System.Serializable]
public struct OrbitalParamsSaveData
{
    //=========================================
    public const int NB_PARAMS=17; // Won't be serialized and won't be saved. Used only to set the size of the array passed in the constructor
    //=========================================
    [SerializeField] private string orbitedBodyName;
    
    [SerializeField] private string orbitDefTypeInt;
    [SerializeField] private string bodyPosTypeInt;
    [SerializeField] private string orbParamsUnitsInt;
    [SerializeField] private string orbitRealPredTypeInt;
    [SerializeField] private string selectedVectordDirInt;

    [SerializeField] private string drawOrbitInt;
    [SerializeField] private string drawDirectionsInt;
    [SerializeField] private string orbitDrawingResolutionInt;

    [SerializeField] private string raDouble;
    [SerializeField] private string rpDouble;
    [SerializeField] private string pDouble;
    [SerializeField] private string eDouble;

    [SerializeField] private string iDouble;
    [SerializeField] private string lAscNDouble;
    [SerializeField] private string omegaDouble;
    [SerializeField] private string nuDouble;

    public OrbitalParamsSaveData(params string[] values)
    {
        if(values.Length != NB_PARAMS) {
            Debug.Log("The passed array to save the OrbitalParams has an incorrect size. Size should be " + NB_PARAMS + ", but passed array has size " + values.Length);
        }
        this.orbitedBodyName           = values[0];
        this.orbitDefTypeInt           = values[1];
        this.bodyPosTypeInt            = values[2];
        this.orbParamsUnitsInt         = values[3];
        this.orbitRealPredTypeInt      = values[4];
        this.selectedVectordDirInt     = values[5];
        this.drawOrbitInt              = values[6];
        this.drawDirectionsInt         = values[7];
        this.orbitDrawingResolutionInt = values[8];
        this.raDouble                  = values[9];
        this.rpDouble                  = values[10];
        this.pDouble                   = values[11];
        this.eDouble                   = values[12];
        this.iDouble                   = values[13];
        this.lAscNDouble               = values[14];
        this.omegaDouble               = values[15];
        this.nuDouble                  = values[16];
    }

    //========================================================================
    public static OrbitalParams LoadObjectFromJSON(string filepath)
    {
        OrbitalParamsSaveData loadedData = JsonUtility.FromJson<OrbitalParamsSaveData>(File.ReadAllText(filepath));
        
        OrbitalParams output = (OrbitalParams)ScriptableObject.CreateInstance<OrbitalParams>();
        output.orbitedBodyName = loadedData.orbitedBodyName;
        output.orbitDefType = (OrbitalTypes.orbitDefinitionType) Fncs.TryParse_String_To_Int(loadedData.orbitDefTypeInt, 0);
        output.bodyPosType = (OrbitalTypes.bodyPositionType) Fncs.TryParse_String_To_Int(loadedData.bodyPosTypeInt, 0);
        output.orbParamsUnits = (OrbitalTypes.orbitalParamsUnits) Fncs.TryParse_String_To_Int(loadedData.orbParamsUnitsInt, 0);
        output.orbitRealPredType = (OrbitalTypes.typeOfOrbit) Fncs.TryParse_String_To_Int(loadedData.orbitRealPredTypeInt, 0);
        output.selectedVectorsDir = (OrbitalTypes.typeOfVectorDir) Fncs.TryParse_String_To_Int(loadedData.selectedVectordDirInt, 0);

        output.drawOrbit = Fncs.TryParse_String_To_Bool(loadedData.drawOrbitInt, false);
        output.drawDirections = Fncs.TryParse_String_To_Bool(loadedData.drawDirectionsInt, false);

        output.orbitDrawingResolution = Fncs.TryParse_String_To_Int(loadedData.orbitDrawingResolutionInt, 300);
        UsefulFunctions.ParseStringToDouble(loadedData.raDouble, out output.ra);
        UsefulFunctions.ParseStringToDouble(loadedData.rpDouble, out output.rp);
        UsefulFunctions.ParseStringToDouble(loadedData.pDouble, out output.p);
        UsefulFunctions.ParseStringToDouble(loadedData.eDouble, out output.e);
        UsefulFunctions.ParseStringToDouble(loadedData.iDouble, out output.i);
        UsefulFunctions.ParseStringToDouble(loadedData.lAscNDouble, out output.lAscN);
        UsefulFunctions.ParseStringToDouble(loadedData.omegaDouble, out output.omega);
        UsefulFunctions.ParseStringToDouble(loadedData.nuDouble, out output.nu);
        
        return output;
    }
}
