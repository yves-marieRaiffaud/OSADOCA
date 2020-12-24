using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using planets = CelestialBodiesConstants.planets;

public static class LaunchPadList
{
    public static Dictionary<LaunchPad.launchPadParams, string> GetOriginLaunchPadDict(string planetName)
    {
        Dictionary<LaunchPad.launchPadParams, string> originPointDict = new Dictionary<LaunchPad.launchPadParams, string>() {
            { LaunchPad.launchPadParams.isCustomLP, "0" },
            { LaunchPad.launchPadParams.refPlanet, planetName },
            { LaunchPad.launchPadParams.name, "Origin (0°,0°)" },
            { LaunchPad.launchPadParams.country, "-" },
            { LaunchPad.launchPadParams.operationalDate, "-" },
            { LaunchPad.launchPadParams.supervision, "-" },
            { LaunchPad.launchPadParams.latitude, "0.0" },
            { LaunchPad.launchPadParams.longitude, "0.0" }
        };
        return originPointDict;
    }
    //------------------------------------------------
    //------------------------------------------------
    private static Dictionary<LaunchPad.launchPadParams, string> kourouDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Kourou" },
        { LaunchPad.launchPadParams.country, "French Guiana" },
        { LaunchPad.launchPadParams.operationalDate, "1968 - " },
        { LaunchPad.launchPadParams.supervision, "CNES-ESA" },
        { LaunchPad.launchPadParams.latitude, "5.23739" },
        { LaunchPad.launchPadParams.longitude, "-52.76950" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> wenchangDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Whenchang Satellite Launch Center" },
        { LaunchPad.launchPadParams.country, "China" },
        { LaunchPad.launchPadParams.operationalDate, "2016 - " },
        { LaunchPad.launchPadParams.supervision, "CNSA" },
        { LaunchPad.launchPadParams.latitude, "19.365217" },
        { LaunchPad.launchPadParams.longitude, "110.57408" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> sohaeDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Sohae" },
        { LaunchPad.launchPadParams.country, "North Korea" },
        { LaunchPad.launchPadParams.operationalDate, "2012 - " },
        { LaunchPad.launchPadParams.supervision, "NADA" },
        { LaunchPad.launchPadParams.latitude, "39.660" },
        { LaunchPad.launchPadParams.longitude, "124.705" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> semnanDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Semnan" },
        { LaunchPad.launchPadParams.country, "Iran" },
        { LaunchPad.launchPadParams.operationalDate, "2009 - " },
        { LaunchPad.launchPadParams.supervision, "ISA" },
        { LaunchPad.launchPadParams.latitude, "35.234631" },
        { LaunchPad.launchPadParams.longitude, "53.920941" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> yasnyDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Yasni Cosmodrome" },
        { LaunchPad.launchPadParams.country, "Russia" },
        { LaunchPad.launchPadParams.operationalDate, "2006 - " },
        { LaunchPad.launchPadParams.supervision, "ROSCOSMOS" },
        { LaunchPad.launchPadParams.latitude, "51.20706" },
        { LaunchPad.launchPadParams.longitude, "59.85003" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> esrangeDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Esrange" },
        { LaunchPad.launchPadParams.country, "Sweden" },
        { LaunchPad.launchPadParams.operationalDate, "1972 - " },
        { LaunchPad.launchPadParams.supervision, "SSC" },
        { LaunchPad.launchPadParams.latitude, "67.89342" },
        { LaunchPad.launchPadParams.longitude, "21.10429" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> capeCanaveralDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Cape Canaveral Air Force Station" },
        { LaunchPad.launchPadParams.country, "USA" },
        { LaunchPad.launchPadParams.operationalDate, "1956 - " },
        { LaunchPad.launchPadParams.supervision, "United States Space Force" },
        { LaunchPad.launchPadParams.latitude, "28.46675" },
        { LaunchPad.launchPadParams.longitude, "-80.55852" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> vanderbergDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Vandenberg Air Force Base" },
        { LaunchPad.launchPadParams.country, "USA" },
        { LaunchPad.launchPadParams.operationalDate, "1958 - " },
        { LaunchPad.launchPadParams.supervision, "United States Space Force" },
        { LaunchPad.launchPadParams.latitude, "34.77204" },
        { LaunchPad.launchPadParams.longitude, "-120.60124" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> kennedyDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Kennedy Space Center" },
        { LaunchPad.launchPadParams.country, "USA" },
        { LaunchPad.launchPadParams.operationalDate, "1963 - " },
        { LaunchPad.launchPadParams.supervision, "NASA" },
        { LaunchPad.launchPadParams.latitude, "28.6082" },
        { LaunchPad.launchPadParams.longitude, "-80.6040" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> mojaveDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "Mojave Air and Space Port" },
        { LaunchPad.launchPadParams.country, "USA" },
        { LaunchPad.launchPadParams.operationalDate, "2004 - " },
        { LaunchPad.launchPadParams.supervision, "Mojave Air and Space Port" },
        { LaunchPad.launchPadParams.latitude, "35.05910" },
        { LaunchPad.launchPadParams.longitude, "-118.14880" }
    };
    private static Dictionary<LaunchPad.launchPadParams, string> whiteSandDict = new Dictionary<LaunchPad.launchPadParams, string>() {
        { LaunchPad.launchPadParams.isCustomLP, "0" },
        { LaunchPad.launchPadParams.refPlanet, "Earth" },
        { LaunchPad.launchPadParams.name, "White Sands Missile Range" },
        { LaunchPad.launchPadParams.country, "USA" },
        { LaunchPad.launchPadParams.operationalDate, "1946 - " },
        { LaunchPad.launchPadParams.supervision, "US Army" },
        { LaunchPad.launchPadParams.latitude, "32.56460" },
        { LaunchPad.launchPadParams.longitude, "-106.35908" }
    };
    //------------------------------------------------
    //------------------------------------------------
    private static Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> earth_launchPads = new Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>>() {
        { "Kourou"                             , kourouDict },
        { "Whenchang Satellite Launch Center"  , wenchangDict },
        { "Sohae"                              , sohaeDict },
        { "Semnan"                             , semnanDict },
        { "Yasni Cosmodrome"                   , yasnyDict },
        { "Esrange"                            , esrangeDict },
        { "Cape Canaveral Air Force Station"   , capeCanaveralDict },
        { "Vandenberg Air Force Base"          , vanderbergDict },
        { "Kennedy Space Center"               , kennedyDict },
        { "Mojave Air and Space Port"          , mojaveDict },
        { "White Sands Missile Range"          , whiteSandDict },
        { "Origin (0°,0°)"                     , GetOriginLaunchPadDict(planets.Earth.ToString()) }
    };
    //------------------------------------------------
    //------------------------------------------------
    private static Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> mercury_launchPads = new Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>>() {
        { "Origin (0°,0°)", GetOriginLaunchPadDict(planets.Mercury.ToString()) }
    };
    //------------------------------------------------
    //------------------------------------------------
    private static Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> venus_launchPads = new Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>>() {
        { "Origin (0°,0°)", GetOriginLaunchPadDict(planets.Venus.ToString()) }
    };
    //------------------------------------------------
    //------------------------------------------------
    private static Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> mars_launchPads = new Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>>() {
        { "Origin (0°,0°)", GetOriginLaunchPadDict(planets.Mars.ToString()) }
    };
    //------------------------------------------------
    //------------------------------------------------
    public static Dictionary<planets, Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>>> launchPadsDict = new Dictionary<planets, Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>>>() {
        { planets.Mercury, mercury_launchPads },
        { planets.Venus, venus_launchPads },
        { planets.Earth, earth_launchPads },
        { planets.Mars, mars_launchPads }
    };
}