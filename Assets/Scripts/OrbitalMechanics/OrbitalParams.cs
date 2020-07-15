using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

[System.Serializable, CreateAssetMenu()]
public class OrbitalParams: ScriptableObject
{
    public enum orbitDefinitionType { rarp, rpe, pe }; // Specify the parameters to use to define the orbit
    public enum bodyPositionType { nu, m0, l0, t0 }; // Specify the parameter to use to position the body on its orbit
    public enum orbitalParamsUnits { km_degree, AU_degree };

    public orbitDefinitionType orbitDefType; // Only in the custom editor
    public bodyPositionType bodyPosType; // Only in the custom editor
    public orbitalParamsUnits orbParamsUnits; 
    public typeOfOrbit orbitRealPredType; // Only in the custom editor
    public typeOfVectorDir selectedVectorsDir=(typeOfVectorDir)0; // Selected vectors to draw, only in the custom editor

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
    public double m0; // Mean Anomaly, degree
    public double l0; // Mean Longitude, degree
    public double t0; // Time at perihelion passage
    //==============
    public double period; // Orbtial period, seconds
    //==============
    public Vector3d vp; // Vernal Point
    public Vector3d vpAxisRight; // Perpendicular vector of vp
    public Vector3d vpAxisUp; // Perpendicualr vector of vp and vpAxisRight
    public Vector3d ascendingNodeLineDir; // Ascending Nodes Line
    public Vector3d apogeeLineDir; // Apsides Line
    //==============
    public OrbitPlane orbitPlane;
    //==============
    [System.Flags] public enum typeOfVectorDir { vernalPoint=2, vpAxisRight=4, vpAxisUp=8, apogeeLine=16, ascendingNodeLine=32, tangentialVec=64, radialVec=128, velocityVec=256, accelerationVec=512 };
    public Dictionary <typeOfVectorDir, string> suffixVectorDir = new Dictionary<typeOfVectorDir, string>(){
        {typeOfVectorDir.vernalPoint,       "VP_"},
        {typeOfVectorDir.vpAxisRight,       "VPAxisRight_"},
        {typeOfVectorDir.vpAxisUp,          "VPAxisUp_"},
        {typeOfVectorDir.apogeeLine,        "Apsides_"},
        {typeOfVectorDir.ascendingNodeLine, "Nodes_"},
        {typeOfVectorDir.tangentialVec, "Tangential_"},
        {typeOfVectorDir.radialVec, "Radial_"},
        {typeOfVectorDir.velocityVec, "Velocity_"},
        {typeOfVectorDir.accelerationVec, "Acceleration_"}
    };
    //==============
    // if typeOfOrbit is 'realOrbit', the variables of this class are computed then copied to the associated OrbitalParams class
    public enum typeOfOrbit { realOrbit, predictedOrbit };
    public Dictionary <typeOfOrbit, string> suffixOrbitType = new Dictionary<typeOfOrbit, string>(){
        {typeOfOrbit.realOrbit,      "Real"},
        {typeOfOrbit.predictedOrbit, "Predicted"}
    };

    //==============
    public bool showInfoPanel=false;
}