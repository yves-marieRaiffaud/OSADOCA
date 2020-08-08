using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mathd_Lib;
using pressure = Units.pressure;
using temperature = Units.temperature;
using angle = Units.angle;
using time = Units.time;
using distance = Units.distance;
using TFunc = System.Func<dynamic, dynamic, dynamic>;

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
    public const double au2u = 100_000d; // 1 AU (150M km) translated into the unity coordinates system. 1au == 1000f
    public const double u2au = 1d/au2u;

    // u2pl=2f ==> 1 unity unit == 2km of the radius of a planet
    public const double u2pl = 2d; // Conversion between the planetary scale and the unit coordinate system.
    public const double pl2u = 1d/u2pl;

    // Only 6 levels in this array as the first distance is '+infinity' for level 0
    // level 0 is +infinity and will not be modified
    // Order of the array's ratios : 'level1', 'level2', 'level3', 'level4', 'level5', 'level6'
    // Values corresponds to the radiusU. New distance is ths 'radiusU * ratio'
    public static float[] ratioCelestBodiesLODDistances = new float[] { 0.8f, 0.3f, 0.05f, 0.005f, 0.0007f, 0.00015f };
    public const string sphereNoLODTemplate_GO = "Sphere_noLOD_Template";

    // u2sh=0.005 ==> 1 unity unit == 10 m of the spaceship (with u2pl == 2d == 2km)
    public const double u2sh = 10d / (u2pl * km2m);
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
    public static Dictionary<string, UnitInterface> sunBaseParams = new Dictionary<string, UnitInterface> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , new Distance(695_700d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , new Distance(695_700d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , new DoubleNoDim(20_000d) },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , new Distance(100_000d, distance.km) }, //TO CHANGE
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(0d, angle.degree) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(2_192_832d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new DoubleNoDim(13_271_244.0018d) },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , new Distance(0d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , new Distance(0d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(0d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , new Angle(0d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , new Angle(0d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO CHANGE

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degree_C) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , new DoubleNoDim(0d) }, // x10^(-6)
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , new DoubleNoDim(0d) }
    };

    public static Dictionary<string, UnitInterface> mercuryBaseParams = new Dictionary<string, UnitInterface> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , new Distance(2_439.7d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , new Distance(2_439.7d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , new Distance(100_000d, distance.km) }, //TO CHANGE
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(0.034d, angle.degree) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(5_065_560d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new DoubleNoDim(2.2032d) },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , new Distance(0.466697d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , new Distance(0.307499d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(3.38d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , new Angle(48.331d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , new Angle(29.124d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO CHANGE

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degree_C) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , new DoubleNoDim(50.3d) }, // x10^(-6)
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , new DoubleNoDim(1d) }
    };

    public static Dictionary<string, UnitInterface> venusBaseParams = new Dictionary<string, UnitInterface> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , new Distance(6_051.8d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , new Distance(6_051.8d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , new Distance(0d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(2.64d, angle.degree) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(-20_997_360d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new DoubleNoDim(32.4859d) },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , new Distance(0.728213d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , new Distance(0.718440d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(3.39d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , new Angle(76.680d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , new Angle(54.884d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degree_C) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , new DoubleNoDim(1d) }
    };

    public static Dictionary<string, UnitInterface> earthBaseParams = new Dictionary<string, UnitInterface> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , new Distance(6_378.137d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , new Distance(6_356.752d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , new DoubleNoDim(298.257223563d) },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , new Distance(120_000d, distance.km) }, // TO CHANNGE
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(23.4392811d, angle.degree) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(86_164.09d, time.s) }, // Indicate a positive value for a rotation of the planet in prograde rotation. A negative value for a retrograde rotation
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new DoubleNoDim(39.86004418d) },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , new Distance(1.067d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , new Distance(0.92329d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(7.155d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , new Angle(174.9d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , new Angle(288.1d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO CHANGE

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(1_015d, Units.pressure.hPa) }, // hPa
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(1.217d) }, // kg.m-3
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(288d, temperature.degree_K) }, // Average temp, in K
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(100d) }, // Karman line, in km
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , new DoubleNoDim(8.848d) }, // for the height map, in km

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , new DoubleNoDim(1_082.63d) }, // x10^(-6)
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , new DoubleNoDim(1d) }
    };

    public static Dictionary<string, UnitInterface> marsBaseParams = new Dictionary<string, UnitInterface> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , new Distance(3396.2d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , new Distance(3376.2d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , new DoubleNoDim(16.9779d) },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , new Distance(0d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(25.19d, angle.degree) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(88_642.44d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new DoubleNoDim(4.282837d) },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , new Distance(1.666d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , new Distance(1.382d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(1.85d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , new Angle(49.558d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , new Angle(286.502d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degree_C) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , new DoubleNoDim(1d) }
    };

    public static Dictionary<string, UnitInterface> jupiterBaseParams = new Dictionary<string, UnitInterface> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , new Distance(71_492d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , new Distance(66_854d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , new DoubleNoDim(15.4154d) },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , new Distance(0d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(3.13d, angle.degree) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(35_730d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new DoubleNoDim(12_668.6534d) },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , new Distance(5.4588d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , new Distance(4.9501d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(1.304d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , new Angle(100.464d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , new Angle(273.867d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degree_C) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , new DoubleNoDim(0d) }
    };

    public static Dictionary<string, UnitInterface> saturnBaseParams = new Dictionary<string, UnitInterface> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , new Distance(60_268d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , new Distance(54_364d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , new DoubleNoDim(10.2082d) },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , new Distance(0d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(26.73d, angle.degree) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(38_361.6d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new DoubleNoDim(3_793.1187d) },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , new Distance(10.1238d ,distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , new Distance(9.0412d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(2.485d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , new Angle(113.665d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , new Angle(339.392d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           ,  new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degree_C) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , new DoubleNoDim(0d) }
    };

    public static Dictionary<string, UnitInterface> uranusBaseParams = new Dictionary<string, UnitInterface> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , new Distance(25_559d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , new Distance(24_973d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , new DoubleNoDim(43.61098d) },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , new Distance(0d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(82.33d, angle.degree) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(-62_064d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new DoubleNoDim(579.3939d) },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , new Distance(20.11d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , new Distance(18.33d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(0.772d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , new Angle(74.006d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , new Angle(96.998857d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degree_C) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , new DoubleNoDim(0d) }
    };

    public static Dictionary<string, UnitInterface> neptuneBaseParams = new Dictionary<string, UnitInterface> {
        { CelestialBodyParamsBase.planetaryParams.radius.ToString()             , new Distance(24_764d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()        , new Distance(24_341d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.inverseFlattening.ToString()  , new DoubleNoDim(58.54808d) },
        { CelestialBodyParamsBase.planetaryParams.radiusSOI.ToString()          , new Distance(0d, distance.km) },
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(28.32d, angle.degree) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(57_996d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new DoubleNoDim(683.6529d) },

        { CelestialBodyParamsBase.orbitalParams.aphelion.ToString()             , new Distance(30.33d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.perihelion.ToString()           , new Distance(29.81d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(0.0113d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.longAscendingNode.ToString()    , new Angle(131.784d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.perihelionArg.ToString()        , new Angle(276.336d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degree_C) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.jnParams.j2.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j3.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j4.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j5.ToString()                        , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.jnParams.j6.ToString()                        , new DoubleNoDim(0d) },

        { CelestialBodyParamsBase.otherParams.isRockyBody.ToString()            , new DoubleNoDim(0d) }
    };

    public static readonly Dictionary <planets, Dictionary<string, UnitInterface>> planetsDict = new Dictionary<planets, Dictionary<string, UnitInterface>>
    {
        { planets.Sun      , sunBaseParams },
        { planets.Mercury  , mercuryBaseParams },
        { planets.Venus    , venusBaseParams },
        { planets.Earth    , earthBaseParams },
        { planets.Mars     , marsBaseParams },
        { planets.Jupiter  , jupiterBaseParams },
        { planets.Saturn   , saturnBaseParams },
        { planets.Uranus   , uranusBaseParams },
        { planets.Neptune  , neptuneBaseParams }
    };

    public static readonly Dictionary <planets, Dictionary<PlanetsFunctions.chosenFunction, TFunc>> planetsFncsDict = new Dictionary<planets, Dictionary<PlanetsFunctions.chosenFunction, TFunc>>
    {
        { planets.Sun      , null },
        { planets.Mercury  , null },
        { planets.Venus    , null },
        { planets.Earth    , PlanetsFunctions.earthFcnsDict },
        { planets.Mars     , null },
        { planets.Jupiter  , null },
        { planets.Saturn   , null },
        { planets.Uranus   , null },
        { planets.Neptune  , null }
    };
}