using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CelestialBodyParamsBase
{
    // SurfacePressure in Pa, surfaceTemp in K, radiusSOI in radius of the planetary body
    // Axial tilt: the angle between the body equatorial plane and its orbital plane, in °.
    // Radius is the equatorial radius
    [SerializeField] public enum planetaryParams { radius, polarRadius, inverseFlattening, radiusSOI, axialTilt, siderealRotPeriod, mu, massEarthRatio };

    [SerializeField] public enum orbitalParams { aphelion, perihelion, i, longAscendingNode, perihelionArg, trueAnomaly };

    // 'highestBumpAlt' is in km. Specify the highest altitude in km to scale the grayscale height map when drawing the celestialBody
    [SerializeField] public enum biomeParams { surfPressure, surfDensity, surfTemp, maxAtmoHeight, highestBumpAlt, pressureFunc};

    // Jn of the CelestialBody for the unequal gravitational potential
    [SerializeField] public enum jnParams { j2, j3, j4, j5, j6 };

    // 'isRockyBody': bool: 0=false. 1=true.
    [SerializeField] public enum otherParams { isRockyBody };
}

