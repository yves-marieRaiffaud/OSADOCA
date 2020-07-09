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

    // Vernal Point
    public static readonly  Vector3d pv = Vector3d.right; // SHOULD BE CHANGED TO SOMETHING ELSE

    //===============================================================================================
    //===============================================================================================
    // PLANETS - STARS RELATED PARAMETERS
    public enum planets { Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune }
    public enum stars { Sun };
    public enum orbitalParams { aphelion, perihelion, i, longAscendingNode, perihelionArg, trueAnomaly };
    // All distance in AU or degrees
    // i is inclination with respect to the Sun's equator

    public static readonly Dictionary <orbitalParams, double> mercuryOrbitalParams = new Dictionary<orbitalParams, double> {
        { orbitalParams.aphelion         , 0.466697d},
        { orbitalParams.perihelion       , 0.307499d },
        { orbitalParams.i                , 3.38d },
        { orbitalParams.longAscendingNode, 48.331d },
        { orbitalParams.perihelionArg    , 29.124d },
        { orbitalParams.trueAnomaly      , 0d}, // VALUE TO SET ACCORDING TO J200
    };

    public static readonly Dictionary <orbitalParams, double> venusOrbitalParams = new Dictionary<orbitalParams, double> {
        { orbitalParams.aphelion         , 0.728213d},
        { orbitalParams.perihelion       , 0.718440d },
        { orbitalParams.i                , 3.86d },
        { orbitalParams.longAscendingNode, 76.680d },
        { orbitalParams.perihelionArg    , 54.884d },
        { orbitalParams.trueAnomaly      , 0d}, // VALUE TO SET ACCORDING TO J200
    };

    public static readonly Dictionary <orbitalParams, double> earthOrbitalParams = new Dictionary<orbitalParams, double> {
        { orbitalParams.aphelion         , 1.067d},
        { orbitalParams.perihelion       , 0.92329d },
        { orbitalParams.i                , 7.155d },
        { orbitalParams.longAscendingNode, 174.9d },
        { orbitalParams.perihelionArg    , 288.1d },
        { orbitalParams.trueAnomaly      , 0d}, // VALUE TO SET ACCORDING TO J200
    };

    public static readonly Dictionary <orbitalParams, double> marsOrbitalParams = new Dictionary<orbitalParams, double> {
        { orbitalParams.aphelion         , 1.666d},
        { orbitalParams.perihelion       , 1.382d },
        { orbitalParams.i                , 5.65d },
        { orbitalParams.longAscendingNode, 49.558d },
        { orbitalParams.perihelionArg    , 286.502d },
        { orbitalParams.trueAnomaly      , 0d}, // VALUE TO SET ACCORDING TO J200
    };

    public static readonly Dictionary <orbitalParams, double> jupiterOrbitalParams = new Dictionary<orbitalParams, double> {
        { orbitalParams.aphelion         , 5.4588d},
        { orbitalParams.perihelion       , 4.9501d },
        { orbitalParams.i                , 6.09d },
        { orbitalParams.longAscendingNode, 100.464d },
        { orbitalParams.perihelionArg    , 273.867d },
        { orbitalParams.trueAnomaly      , 0d}, // VALUE TO SET ACCORDING TO J200
    };

    public static readonly Dictionary <orbitalParams, double> saturnOrbitalParams = new Dictionary<orbitalParams, double> {
        { orbitalParams.aphelion         , 10.1238d},
        { orbitalParams.perihelion       , 9.0412d },
        { orbitalParams.i                , 5.51d },
        { orbitalParams.longAscendingNode, 113.665d },
        { orbitalParams.perihelionArg    , 339.392d },
        { orbitalParams.trueAnomaly      , 0d}, // VALUE TO SET ACCORDING TO J200
    };

    public static readonly Dictionary <orbitalParams, double> uranusOrbitalParams = new Dictionary<orbitalParams, double> {
        { orbitalParams.aphelion         , 20.11d},
        { orbitalParams.perihelion       , 18.33d },
        { orbitalParams.i                , 6.48d },
        { orbitalParams.longAscendingNode, 74.006d },
        { orbitalParams.perihelionArg    , 96.998857d },
        { orbitalParams.trueAnomaly      , 0d}, // VALUE TO SET ACCORDING TO J200
    };

    public static readonly Dictionary <orbitalParams, double> neptuneOrbitalParams = new Dictionary<orbitalParams, double> {
        { orbitalParams.aphelion         , 30.33d },
        { orbitalParams.perihelion       , 29.81d },
        { orbitalParams.i                , 6.43d },
        { orbitalParams.longAscendingNode, 131.784d },
        { orbitalParams.perihelionArg    , 276.336d },
        { orbitalParams.trueAnomaly      , 0d }, // VALUE TO SET ACCORDING TO J200
    };

    public static readonly Dictionary <planets, Dictionary<orbitalParams,double>> planetsDict = new Dictionary<planets, Dictionary<orbitalParams, double>>
    {
        { planets.Mercury, mercuryOrbitalParams },
        { planets.Venus, venusOrbitalParams },
        { planets.Earth, earthOrbitalParams },
        { planets.Mars, marsOrbitalParams },
        { planets.Jupiter, jupiterOrbitalParams },
        { planets.Saturn, saturnOrbitalParams },
        { planets.Uranus, uranusOrbitalParams },
        { planets.Neptune, neptuneOrbitalParams }
    };


}