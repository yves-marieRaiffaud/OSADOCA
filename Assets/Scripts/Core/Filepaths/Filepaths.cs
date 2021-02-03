using System.Collections;
using System.Collections.Generic;

// Class that gathers every filepaths needed in the code
public static class Filepaths
{
    //------------------------------------------------------------------------------
    //------------------------------------------------------------------------------
    // RELATIVE TO THE 'resources' FOLDER. TO USE WITH 'Resources.Load<>()'
    //public const string RSC_defaultHeightMap = "CelestialBody/default_heightMap";

    public const string RSC_orbitMaterial = "OrbitalMechanics/OrbitMat";

    public const string RSC_PlanetsMaterials = "CelestialBody/Materials/";
    // with the name of the planet afterwards for 'DEBUG_UIPlanetaryMaps'
    public const string RSC_UIPlanetaryMaps = "CelestialBody/UI_PLanetary_Maps/";
    //------------------------------------------------------------------------------
    //------------------------------------------------------------------------------
    // USING THE 'APPLICATION.PERSISTENTDATAPATH'
    public const string shipToLoad_orbitalParams = "/shipToLoad_orbitalParams.json";

    public const string celestBody_Folder = "/CelestialBodies/OrbitalParams/";
    public const string celestBodySettingsFile = "/CelestialBodySettings.json";
    public const string orbitalParamsFile = "/OrbitalParams.json";
    
    //public const string shipToLoad_settings = "/shipToLoad_settings.json"; 
    public const string simulation_settings = "/simulation_settings.json";

    public const string userAdded_launchPads = "/custom_launchPads.json";

    public const string comsPanel_params = "/communication_remoteControl.json";
    
    public const string simUI_Folder = "/Sim_UI";
    public const string simUI_displayOptions = "/displayOptions_MSDropdown.json";

    public const string ma_Grids_FolderPath = "Mission_Analysis/Grids/";
    public const string ma_Grids_ManifestPath = "Mission_Analysis/Grids/manifest";
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