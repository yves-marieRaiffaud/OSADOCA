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
using mass = Units.mass;
using gravConst = Units.gravConstant;
using TFunc = System.Func<dynamic, dynamic, dynamic>;

public static class CelestialBodiesConstants
{
    // PLANETS - STARS RELATED PARAMETERS
    public enum planets { None, Sun, Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune }

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
        { CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()   , new Time_Class(0.0d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new GravConstant(13_271_244.0018d, gravConst.m3s2) }, // m3/s2
        { CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()     , new DoubleNoDim(333_000d) },

        { CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()      , new String_Unit("Sun")  },
        { CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()             , new Distance(0d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.periapsis.ToString()            , new Distance(0d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(0d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.lAscN.ToString()                , new Angle(0d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()         , new Angle(0d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO CHANGE

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degreeC) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , new DoubleNoDim(0d) },

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
        { CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()   , new Time_Class(87.97d, time.day) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new GravConstant(2.2032d, gravConst.m3s2) }, // m3/s2
        { CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()     , new DoubleNoDim(0.0553d) },

        { CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()      , new String_Unit("Sun")  },
        { CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()             , new Distance(0.466697d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.periapsis.ToString()            , new Distance(0.307499d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(3.38d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.lAscN.ToString()                , new Angle(48.331d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()         , new Angle(29.124d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO CHANGE

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degreeC) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , new DoubleNoDim(0d) },

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
        { CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()   , new Time_Class(224.7d, time.day) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new GravConstant(32.4859d, gravConst.m3s2) }, // m3/s2
        { CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()     , new DoubleNoDim(0.815d) },

        { CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()      , new String_Unit("Sun")  },
        { CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()             , new Distance(0.728213d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.periapsis.ToString()            , new Distance(0.718440d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(3.39d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.lAscN.ToString()                , new Angle(76.680d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()         , new Angle(54.884d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degreeC) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , new DoubleNoDim(0d) },

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
        { CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()          , new Angle(23.4392811d, angle.degree) }, // Also called the obliquity to the ecliptic
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(86_164.09d, time.s) }, // Indicate a positive value for a rotation of the planet in prograde rotation. A negative value for a retrograde rotation
        { CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()   , new Time_Class(365.26, time.day) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new GravConstant(39.86004418d, gravConst.m3s2) }, // m3/s2
        { CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()     , new DoubleNoDim(1d) },

        { CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()      , new String_Unit("Sun")  },
        { CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()             , new Distance(1.067d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.periapsis.ToString()            , new Distance(0.92329d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(7.155d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.lAscN.ToString()                , new Angle(174.9d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()         , new Angle(288.1d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO CHANGE

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(1_015d, Units.pressure.hPa) }, // hPa
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(1.217d) }, // kg.m-3
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(288d, temperature.degreeK) }, // Average temp, in K
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
        { CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()   , new Time_Class(686.98d, time.day) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(88_642.44d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new GravConstant(4.282837d, gravConst.m3s2) }, // m3/s2
        { CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()     , new DoubleNoDim(0.107d) },

        { CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()      , new String_Unit("Sun")  },
        { CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()             , new Distance(1.666d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.periapsis.ToString()            , new Distance(1.382d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(1.85d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.lAscN.ToString()                , new Angle(49.558d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()         , new Angle(286.502d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degreeC) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , new DoubleNoDim(0d) },

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
        { CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()   , new Time_Class(11.86d, time.earthYear) },
        { CelestialBodyParamsBase.planetaryParams.siderealRotPeriod.ToString()  , new Time_Class(35_730d, time.s) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new GravConstant(12_668.6534d, gravConst.m3s2) }, // m3/s2
        { CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()     , new DoubleNoDim(317.83d) },

        { CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()      , new String_Unit("Sun")  },
        { CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()             , new Distance(5.4588d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.periapsis.ToString()            , new Distance(4.9501d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(1.304d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.lAscN.ToString()                , new Angle(100.464d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()         , new Angle(273.867d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degreeC) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , new DoubleNoDim(0d) },

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
        { CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()   , new Time_Class(29.46d, time.earthYear) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new GravConstant(3_793.1187d, gravConst.m3s2) }, // m3/s2
        { CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()     , new DoubleNoDim(95.16d) },

        { CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()      , new String_Unit("Sun")  },
        { CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()             , new Distance(10.1238d ,distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.periapsis.ToString()            , new Distance(9.0412d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(2.485d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.lAscN.ToString()                , new Angle(113.665d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()         , new Angle(339.392d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           ,  new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degreeC) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , new DoubleNoDim(0d) },

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
        { CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()   , new Time_Class(84d, time.earthYear) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new GravConstant(579.3939d, gravConst.m3s2) }, // m3/s2
        { CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()     , new DoubleNoDim(14.54d) },

        { CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()      , new String_Unit("Sun")  },
        { CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()             , new Distance(20.11d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.periapsis.ToString()            , new Distance(18.33d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(0.772d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.lAscN.ToString()                , new Angle(74.006d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()         , new Angle(96.998857d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degreeC) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , new DoubleNoDim(0d) },

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
        { CelestialBodyParamsBase.planetaryParams.revolutionPeriod.ToString()   , new Time_Class(164.8d, time.earthYear) },
        { CelestialBodyParamsBase.planetaryParams.mu.ToString()                 , new GravConstant(683.6529d, gravConst.m3s2) }, // m3/s2
        { CelestialBodyParamsBase.planetaryParams.massEarthRatio.ToString()     , new DoubleNoDim(17.15d) },

        { CelestialBodyParamsBase.orbitalParams.orbitedBodyName.ToString()      , new String_Unit("Sun")  },
        { CelestialBodyParamsBase.orbitalParams.apoapsis.ToString()             , new Distance(30.33d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.periapsis.ToString()            , new Distance(29.81d, distance.AU) },
        { CelestialBodyParamsBase.orbitalParams.i.ToString()                    , new Angle(0.0113d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.lAscN.ToString()                , new Angle(131.784d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.periapsisArg.ToString()         , new Angle(276.336d, angle.degree) },
        { CelestialBodyParamsBase.orbitalParams.trueAnomaly.ToString()          , new Angle(0d, angle.degree) }, // TO MODIFY

        { CelestialBodyParamsBase.biomeParams.surfPressure.ToString()           , new Pressure(0d, pressure.atm) },
        { CelestialBodyParamsBase.biomeParams.surfDensity.ToString()            , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.surfTemp.ToString()               , new Temperature(0d, temperature.degreeC) },
        { CelestialBodyParamsBase.biomeParams.maxAtmoHeight.ToString()          , new DoubleNoDim(0d) },
        { CelestialBodyParamsBase.biomeParams.highestBumpAlt.ToString()         , new DoubleNoDim(0d) },

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