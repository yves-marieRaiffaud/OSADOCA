using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class CelestialBodyParamsBase
{
    // SurfacePressure in Pa, surfaceTemp in K, radiusSOI in radius of the planetary body
    // Axial tilt: the angle between the body equatorial plane and its orbital plane, in ° or RAD.
    // Radius is the equatorial radius
    [SerializeField] public enum planetaryParams { radius, polarRadius, inverseFlattening, radiusSOI, axialTilt, siderealRotPeriod, revolutionPeriod, mu, massEarthRatio };

    [SerializeField] public enum orbitalParams { orbitedBodyName, apoapsis, periapsis, i, lAscN, periapsisArg, trueAnomaly };

    // 'highestBumpAlt' is in km. Specify the highest altitude in km to scale the grayscale height map when drawing the celestialBody
    [SerializeField] public enum biomeParams { surfPressure, surfDensity, surfTemp, maxAtmoHeight, highestBumpAlt, pressureFunc};

    // Jn of the CelestialBody for the unequal gravitational potential
    [SerializeField] public enum jnParams { j2, j3, j4, j5, j6 };

    // 'isRockyBody': bool: 0=false. 1=true.
    [SerializeField] public enum otherParams { isRockyBody };
}

[Serializable]
public class CelestialBodySettings : ScriptableObject
{
    [NonSerialized] public bool bodySettingsBoolFoldout=true;
    //-------------------
    [SerializeField] public PlanetaryParams planetaryParams;
    [SerializeField] public Light_CelestBodyOrbitalParams lightOrbitalParams;
    [SerializeField] public SurfaceParams surfaceParams;
    [SerializeField] public CelestBody_Jn jns;

    /// <summary>
    /// Function called to save the specified CelestialBodySettings 'data' to disk by writing the CelestialBodySettings JSON file that will be read.
    /// </summary>
    public static string WriteToFile_CelestialBodySettings_SaveData(CelestialBodySettings data, string filepath)
    {
        // Save the file and returns the filepath
        File.WriteAllText(filepath, JsonUtility.ToJson(data, true));
        return filepath;
    }
}

[Serializable]
public class PlanetaryParams
{
    [SerializeField] public Distance radius;
    [SerializeField] public Distance polarRadius;
    [SerializeField] public Distance radiusSOI;
    [SerializeField] public DoubleNoDim inverseFlattening;
    [SerializeField] public Angle axialTilt;
    [SerializeField] public Time_Class siderealRotPeriod;
    [SerializeField] public Time_Class revolutionPeriod;
    [SerializeField] public GravConstant mu;
    [SerializeField] public DoubleNoDim massEarthRatio;
    [SerializeField] public bool isRockyBody;
}

[Serializable]
public class Light_CelestBodyOrbitalParams
{
    [SerializeField] public String_Unit orbitedBodyName;
    [SerializeField] public Distance apoapsis;
    [SerializeField] public Distance periapsis;
    [SerializeField] public Angle i;
    [SerializeField] public Angle lAscN;
    [SerializeField] public Angle periapsisArg;
    [SerializeField] public Angle trueAnomaly;
}

[Serializable]
public class SurfaceParams
{
    [SerializeField] public Pressure surfPressure;
    [SerializeField] public DoubleNoDim surfDensity;
    [SerializeField] public Temperature surfTemp;
    [SerializeField] public DoubleNoDim maxAtmoHeight;
    [SerializeField] public DoubleNoDim highestBumpAlt;
}

[Serializable]
public class CelestBody_Jn
{
    [SerializeField] public DoubleNoDim j2;
    [SerializeField] public DoubleNoDim j3;
    [SerializeField] public DoubleNoDim j4;
    [SerializeField] public DoubleNoDim j5;
    [SerializeField] public DoubleNoDim j6;
}