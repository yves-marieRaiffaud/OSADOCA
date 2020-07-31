using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Mathd_Lib;

public static class UniCsts
{
    public const int UI_SIGNIFICANT_DIGITS = 6;
    //==========================================================================================
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

    public const double m2km2au2u = m2km * km2au * au2u; // To convert meters to unity units for CELESTIALBODIES
    public const double m2km2pl2u = m2km * pl2u; // To convert meters to unity units for SPACESHIPS

    // u2au = 1000 ==> 1000 unity units == 1 AU
    public const double au2u = 60_000d; // 1 AU (150M km) translated into the unity coordinates system. 1au == 1000f
    public const double u2au = 1d/au2u;

    // u2pl=2f ==> 1 unity unit == 2km of the radius of a planet
    public const double u2pl = 20d; // Conversion between the planetary scale and the unit coordinate system.
    public const double pl2u = 1d/u2pl;

    // Only 6 levels in this array as the first distance is '+infinity' for level 0
    // level 0 is +infinity and will not be modified
    // Order of the array's ratios : 'level1', 'level2', 'level3', 'level4', 'level5', 'level6'
    // Values corresponds to the radiusU. New distance is ths 'radiusU * ratio'
    public static float[] ratioCelestBodiesLODDistances = new float[] { 0.8f, 0.3f, 0.05f, 0.005f, 0.0007f, 0.00015f };
    public const string sphereNoLODTemplate_GO = "Sphere_noLOD_Template";

    // u2sh=1f ==> 1 unity unit == 1 m of the spaceship
    public const double u2sh = 1d;
    public const double sh2u = 1d/u2sh;

    // m2real=1000f ==> 1 mass unit == 1000f kg in real data
    public const double massU2real = 1_000d;
    public const double real2massU = 1d/massU2real;

    // Max distance before offseting every object to bring the camera back at the origin
    public const double dstThreshold = 300d;

    // Vernal Point, very high value on x so that it is located at the infinity
    public static readonly  Vector3d pv = new Vector3d(1e100d, 0d, 0d); // SHOULD BE CHANGED TO SOMETHING ELSE

    //===============================================================================================
    //===============================================================================================
    // PLANETS - STARS RELATED PARAMETERS
    public enum planets { None, Sun, Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune }
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
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 695_700d },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , 695_700d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 20_000d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 100_000d }, //TO CHANGE
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 2_192_832d },
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

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 0d }
    };

    public static Dictionary<string, double> mercuryBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 2_439.7d },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , 2_439.7d },
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

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 1d }
    };

    public static Dictionary<string, double> venusBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 6_051.8d },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , 6_051.8d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 0d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 2.64d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , -20_997_360d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 32.4859d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 0.728213d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 0.718440d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 3.39d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 76.680d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 54.884d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 1d }
    };

    public static Dictionary<string, double> earthBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 6_378.137d },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , 6_356.752d },
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

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 1d }
    };

    public static Dictionary<string, double> marsBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 3396.2d },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , 3376.2d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 16.9779d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 25.19d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 88_642.44d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 4.282837d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 1.666d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 1.382d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 1.85d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 49.558d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 286.502d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 1d }
    };

    public static Dictionary<string, double> jupiterBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 71_492d },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , 66_854d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 15.4154d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 3.13d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 35_730d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 12_668.6534d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 5.4588d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 4.9501d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 1.304d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 100.464d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 273.867d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 0d }
    };

    public static Dictionary<string, double> saturnBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 60_268d },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , 54_364d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 10.2082d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 26.73d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 38_361.6d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 3_793.1187d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 10.1238d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 9.0412d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 2.485d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 113.665d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 339.392d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 0d }
    };

    public static Dictionary<string, double> uranusBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 25_559d },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , 24_973d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 43.61098d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 82.33d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , -62_064d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 579.3939d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 20.11d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 18.33d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0.772d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 74.006d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 96.998857d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 0d }
    };

    public static Dictionary<string, double> neptuneBaseParams = new Dictionary<string, double> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , 24_764d },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , 24_341d },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , 58.54808d },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , 0d },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , 28.32d },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , 57_996d },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , 683.6529d },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , 30.33d },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , 29.81d },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , 0.0113d },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , 131.784d },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , 276.336d },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , 0d }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , 0d },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , 0d },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , 0d },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , 0d },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , 0d },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , 0d },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , 0d }
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