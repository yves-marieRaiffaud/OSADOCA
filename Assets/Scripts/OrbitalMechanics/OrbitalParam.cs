using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

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
}