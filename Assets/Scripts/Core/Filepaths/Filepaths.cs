using System.Collections;
using System.Collections.Generic;

// Class that gathers every filepaths needed in the code
public static class Filepaths
{
    //------------------------------------------------------------------------------
    //------------------------------------------------------------------------------
    // RELATIVE TO THE 'resources' FOLDER. TO USE WITH 'Resources.Load<>()'
    public const string DEBUG_shipOrbitalParams_0 = "Spaceship/Rocket/"; // part 0 of the string
    public const string DEBUG_shipOrbitalParams_2 = "_OrbitalParams.asset"; // part 2 of the string

    public const string DEBUG_planetOrbitalParams_0 = "CelestialBody/OrbitalParams/";
    public const string DEBUG_planetOrbitalParams_2 = ".asset";
    public const string DEBUG_planetSettings_0 = "CelestialBody/Settings/";

    public const string DEBUG_defaultHeightMap = "CelestialBody/default_heightMap";

    public const string DEBUG_orbitMaterial = "OrbitalMechanics/OrbitMaterial";
    // with the name of the planet afterwards for 'DEBUG_UIPlanetaryMaps'
    public const string DEBUG_UIPlanetaryMaps = "CelestialBody/UIPlanetary_Maps/";
    //------------------------------------------------------------------------------
    //------------------------------------------------------------------------------
    public const string shipToLoad_orbitalParams = "/shipToLoad_orbitalParams.json";

    public const string celestBody_Folder = "/CelestialBodies/OrbitalParams/";
    public const string celestBodySettingsFile = "/CelestialBodySettings.json";
    public const string orbitalParamsFile = "/OrbitalParams.json";
    
    //public const string shipToLoad_settings = "/shipToLoad_settings.json"; 
    public const string simulation_settings = "/simulation_settings.json";
    //------------------------------------------------------------------------------
    public const string userAdded_launchPads = "/custom_launchPads.json";
    //------------------------------------------------------------------------------
    //------------------------------------------------------------------------------
    public enum EngineLayersName { Spaceship, CelestialBody, Orbit, StarDome, SpaceshipVectors };
    public static Dictionary<EngineLayersName, int> engineLayersToInt = new Dictionary<EngineLayersName, int>() {
        { EngineLayersName.Spaceship       , 8 },
        { EngineLayersName.CelestialBody   , 9 },
        { EngineLayersName.Orbit           , 10 },
        { EngineLayersName.StarDome        , 11 },
        { EngineLayersName.SpaceshipVectors, 12 },
    };
}