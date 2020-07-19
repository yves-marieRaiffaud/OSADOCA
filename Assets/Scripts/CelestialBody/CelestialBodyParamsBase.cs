using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyParamsBase
{
    // SurfacePressure in Pa, surfaceTemp in K, radiusSOI in radius of the planetary body
    // Axial tilt: the angle between the body equatorial plane and its orbital plane, in °.
    public enum planetaryParams { radius, inverseFlattening, radiusSOI, axialTilt, siderealRotPeriod, mu };

    public enum orbitalParams { aphelion, perihelion, i, longAscendingNode, perihelionArg, trueAnomaly };

    // 'highestBumpAlt' is in km. Specify the highest altitude in km to scale the grayscale height map when drawing the celestialBody
    public enum biomeParams { surfPressure, surfDensity, surfTemp, maxAtmoHeight, highestBumpAlt};

    // Jn of the CelestialBody for the unequal gravitational potential
    public enum jnParams { j2, j3, j4, j5, j6 };
}

