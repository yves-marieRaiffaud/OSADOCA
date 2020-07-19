using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Mathd_Lib;

public static class UniCsts
{
    public const double G = 6.67430d; //E-11f; // m3 kg−1 s−2
    public const double earthMass = 5.97d; //E24 kg
    public const double earthMassExponent = 1_000_000_000_000_000_000_000_000d; // E24

    public const double µExponent = 10_000_000_000_000d; // == E13
    //public const double µEarth = 39.86004418d; // E13 m3.s-2

    public const double rad2deg = 180d/Mathd.PI;
    public const double deg2rad = 1d/rad2deg;

    // Conversion between meters and km
    public const double m2km = 0.001d;
    public const double km2m = 1_000d;
    // Conversion between ton and kg
    public const double t2kg = 1_000d;
    public const double kf2t = 0.001d;

    public const double au2km = 149_597_870.7d; // km
    public const double km2au = 1/au2km;

    // u2au = 1000 ==> 1000 unity units == 1 AU
    public const double au2u = 20_000d; // 1 AU (150M km) translated into the unity coordinates system. 1au == 1000f
    public const double u2au = 1d/au2u;

    // u2pl=2f ==> 1 unity unit == 2km of the radius of a planet
    public const double u2pl = 2d; // Conversion between the planetary scale and the unit coordinate system.
    public const double pl2u = 1d/u2pl;

    // u2sh=1f ==> 1 unity unit == 1 m of the spaceship
    public const double u2sh = 1d;
    public const double sh2u = 1d/u2sh;

    // m2real=1000f ==> 1 mass unit == 1000f kg in real data
    public const double massU2real = 1_000d;
    public const double real2massU = 1d/massU2real;

    // Max distance before offseting every object to bring the camera back at the origin
    public const double dstThreshold = 10_000d;

    // Vernal Point, very high value on x so that it is located at the infinity
    public static readonly  Vector3d pv = new Vector3d(1e100d, 0d, 0d); // SHOULD BE CHANGED TO SOMETHING ELSE

    //===============================================================================================
    //===============================================================================================
    // PLANETS - STARS RELATED PARAMETERS
    public enum planets { Sun, Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune }
    //public enum orbitalParams { aphelion, perihelion, i, longAscendingNode, perihelionArg, trueAnomaly };
    
    /*// SurfacePressure in Pa, surfaceTemp in K, radiusSOI in radius of the planetary body
    // Axial tilt: the angle between the body equatorial plane and its orbital plane, in °.
    public enum planetaryParams { inverseFlattening, surfacePressure, surfaceTemp, maxAtmoHeight, radiusSOI, axialTilt,
    radius, rotationSpeed, mu };

    public enum biomeParams { surfPressure, surfDensity,  aa};

    // Jn of the CelestialBody for the unequal gravitational potential
    public enum jnParams { j2, j3, j4, j5, j6 };*/
    
    // All distance in AU or degrees
    // i is inclination with respect to the Sun's equator

    public static Dictionary<string, double> sunBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 8_000d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 100_000d }, //TO CHANGE
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 13_271_244.0018d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 0d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d }, // TO CHANGE

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d }, // x10^(-6)
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

    };

    public static Dictionary<string, double> mercuryBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 2_439.7d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 100_000d }, //TO CHANGE
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0.034d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 5_065_560d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 2.2032d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0.466697d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0.307499d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 3.38d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 48.331d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 29.124d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d }, // TO CHANGE

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 50.3d }, // x10^(-6)
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

    };

    public static Dictionary<string, double> venusBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 0d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 0d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d },

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

    };

    public static Dictionary<string, double> earthBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 6_378d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 298.257223563d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 120_000d }, // TO CHANNGE
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 23.4392811d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 86_164.09d }, // Indicate a positive value for a rotation of the planet in prograde rotation. A negative value for a retrograde rotation
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 39.86004418d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 1.067d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0.92329d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 7.155d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 174.9d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 288.1d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d }, // TO CHANGE

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 1_015d }, // hPa
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 1.217d }, // kg.m-3
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 288d }, // Average temp, in K
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 100d }, // Karman line, in km
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , 8.848d }, // for the height map, in km

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 1_082.63d }, // x10^(-6)
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

    };

    public static Dictionary<string, double> marsBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 0d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 0d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d },

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

    };

    public static Dictionary<string, double> jupiterBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 0d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 0d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d },

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

    };

    public static Dictionary<string, double> saturnBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 0d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 0d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d },

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

    };

    public static Dictionary<string, double> uranusBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 0d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 0d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d },

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

    };

    public static Dictionary<string, double> neptuneBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 0d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 0d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 0d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()           , 0d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d },

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

    };

    public static readonly Dictionary <planets, Dictionary<string, double>> planetsDict = new Dictionary<planets, Dictionary<string, double>>
    {
        { planets.Sun, sunBaseParams },
        { planets.Mercury, mercuryBaseParams },
        { planets.Venus,   venusBaseParams },
        { planets.Earth,   earthBaseParams },
        { planets.Mars,    marsBaseParams },
        { planets.Jupiter, jupiterBaseParams },
        { planets.Saturn,  saturnBaseParams },
        { planets.Uranus,  uranusBaseParams },
        { planets.Neptune, neptuneBaseParams }
    };



}