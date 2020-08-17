using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Mathd_Lib;
using System.Globalization;
using System.IO;

[System.Serializable]
public class LaunchPad
{
    //========================
    public bool _isCustomLP;
    public bool isCustomLP
    {
        get {
            return _isCustomLP;
        }
    }

    public string _name;
    public string name
    {
        get {
            return _name;
        }
    }

    public string _planet;
    public string planet
    {
        get {
            return _planet;
        }
    }

    public string _country;
    public string country
    {
        get {
            return _country;
        }
    }

    public string _operationalDate;
    public string operationalDate
    {
        get {
            return _operationalDate;
        }
    }
    
    public string _supervision;
    public string supervision
    {
        get {
            return _supervision;
        }
    }
    
    public double _latitude;
    public double latitude
    {
        get {
            return _latitude;
        }
    }

    public double _longitude;
    public double longitude
    {
        get {
            return _longitude;
        }
    }
    
    public double _eastwardBoost;
    public double eastwardBoost
    {
        get {
            return _eastwardBoost;
        }
    }
    //========================
    //========================
    public static string newLaunchPadDefaultName = "New LaunchPad";
    public enum launchPadParams { isCustomLP, refPlanet, name, country, operationalDate, supervision, longitude, latitude, eastwardBoost };
    //========
    private Vector2 lp_XYMapPos; // Position on the map of the planet of the launch pad (x & y coordinate)
    public Dictionary<launchPadParams, string> launchPadParamsDict;
    //=====================================
    //=====================================
    public LaunchPad(Dictionary<launchPadParams, string> ref_launchPadParamsDict)
    {
        // Constructor used when data is created from the passed dictionary
        // The variables and properties are assigned thanks to the values of the dictionary
        launchPadParamsDict = ref_launchPadParamsDict;
        InitVariables();
        UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.latitude], out _latitude);
        UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.longitude], out _longitude);
        ComputeEastwardBoost();
    }

    public LaunchPad()
    {
        // Constructor used when data is loaded from the JSON
        // The variables and properties are initialized, and then the dictionary is rebuilt from these values
        RebuildDict();
        UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.latitude], out _latitude);
        UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.longitude], out _longitude);
    }
    //=====================================
    //=====================================
    public void RebuildDict()
    {
        if(launchPadParamsDict == null)
            launchPadParamsDict = GetEmptyLaunchPadDict(_planet);
        launchPadParamsDict[launchPadParams.country] = _country;
        launchPadParamsDict[launchPadParams.isCustomLP] = _isCustomLP.ToString();
        launchPadParamsDict[launchPadParams.latitude] = UsefulFunctions.DoubleToString(_latitude);
        launchPadParamsDict[launchPadParams.longitude] = UsefulFunctions.DoubleToString(_longitude);
        launchPadParamsDict[launchPadParams.name] = _name;
        launchPadParamsDict[launchPadParams.operationalDate] = _operationalDate;
        launchPadParamsDict[launchPadParams.refPlanet] = _planet;
        launchPadParamsDict[launchPadParams.supervision] = _supervision;
        launchPadParamsDict[launchPadParams.eastwardBoost] = UsefulFunctions.DoubleToString(_eastwardBoost);
    }

    public void InitVariables()
    {
        _name = launchPadParamsDict[launchPadParams.name];
        _planet = launchPadParamsDict[launchPadParams.refPlanet];
        _country = launchPadParamsDict[launchPadParams.country];
        _operationalDate = launchPadParamsDict[launchPadParams.operationalDate];
        _supervision = launchPadParamsDict[launchPadParams.supervision];
        //=====
        if(launchPadParamsDict[launchPadParams.isCustomLP].Equals("1"))
            _isCustomLP = true;
        else
            _isCustomLP = false;
        //=====
        double lat = double.NaN;
        bool parseOk = UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.latitude], out lat);
        if(parseOk)
            _latitude = lat;
        else
            _latitude = double.NaN;
        //=====
        double longi = double.NaN;
        bool parseOkLong = UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.longitude], out longi);
        if(parseOkLong)
            _longitude = longi;
        else
            _longitude = double.NaN;
        //=====
    }

    public double ComputeEastwardBoost()
    {
        // Eastward boost (effet de fronde) is useful when launching satellites to an equatorial orbit (such as GEO)
        // Eastward boost computed in m/s
        string planetName = launchPadParamsDict[launchPadParams.refPlanet];
        double equaRadius = UniCsts.planetsDict[UsefulFunctions.CastStringTo_Unicsts_Planets(planetName)][CelestialBodyParamsBase.planetaryParams.radius.ToString()].value;
        double polarRadius = UniCsts.planetsDict[UsefulFunctions.CastStringTo_Unicsts_Planets(planetName)][CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()].value;

        double lat = _latitude * UniCsts.deg2rad;

        // First compute the geocentric radius depending on the latitude of the launch pad
        double geocentricRadius = ComputeGeocentricRadius(equaRadius, polarRadius, lat);
        geocentricRadius *= UniCsts.km2m;

        // Finally compute the eastward boost
        double eastward_boost = Mathd.Cos(lat) * 2d * Mathd.PI * geocentricRadius / 86400d; // in m/s
        _eastwardBoost = eastward_boost;
        
        // Adding the value to the dictionary, or modifying the existing key
        if(launchPadParamsDict.ContainsKey(launchPadParams.eastwardBoost))
        {
            launchPadParamsDict[launchPadParams.eastwardBoost] = eastward_boost.ToString("G", CultureInfo.InvariantCulture);
        }
        else {
            launchPadParamsDict.Add(launchPadParams.eastwardBoost, eastward_boost.ToString("G", CultureInfo.InvariantCulture));
        }
        return eastward_boost;
    }

    public static double ComputeGeocentricRadius(double equa_radius, double polar_radius, double lat)
    {
        // equatorialRadius & polarRadius in km 
        // Latitude in radian
        double numerator = Mathd.Pow(equa_radius*equa_radius * Mathd.Cos(lat), 2) + Mathd.Pow(polar_radius*polar_radius * Mathd.Sin(lat), 2);
        double denominator = Mathd.Pow(equa_radius * Mathd.Cos(lat), 2) + Mathd.Pow(polar_radius * Mathd.Sin(lat), 2);
        double geocentric_radius = Mathd.Sqrt(numerator / denominator);
        return geocentric_radius;
    }

    public Vector2 LatitudeLongitude_2_XY(Vector2 xy_mapSize)
    {
        // Asuming the map is correctly centered
        // 'xy_mapSize' is the width and height of the destination map (i.e the earth flat map on which the sprites of coordinates X/Y should be drawn)

        double lat, longitude;
        bool latSuccess = UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.latitude], out lat);
        bool longSuccess = UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.longitude], out longitude);
        //=========
        Vector3 worldPos = (Vector3)LaunchPad.LatitudeLongitude_to_3DWorldUNITPoint(lat+90d, longitude);
        //=========
        // Convert the 3D point on the map
        // Computed 'pixel_w' & "pixel_h' are relative to the bottom left corner of the map
        float tmpPos = worldPos.z;
        worldPos.z = worldPos.x;
        worldPos.x = tmpPos;
        //=========
        float pixel_w = (0.5f + Mathf.Atan2(worldPos.x, worldPos.z) / (2f*Mathf.PI)) * xy_mapSize.x;
        float pixel_h = (0.5f - Mathf.Asin(worldPos.y) / Mathf.PI) * xy_mapSize.y;

        return new Vector2(pixel_w, pixel_h);
    }

    public static Vector3d LatitudeLongitude_to_3DWorldUNITPoint(double latitude, double longitude)
    {
        // Convert lat & long to a 3D point on a unit
        // Latitude and longitude passed in degrees
        // First converting angles to radians
        latitude *= UniCsts.deg2rad;
        longitude *= UniCsts.deg2rad;
        //======
        Vector3d worldPos;
        worldPos.x = Mathd.Sin(latitude) * Mathd.Cos(longitude);
        worldPos.y = Mathd.Cos(latitude);
        worldPos.z = Mathd.Sin(latitude) * Mathd.Sin(longitude);
        return worldPos;
    }

    public Vector2 Get_Lat_Long()
    {
        // Returns a vector2 of the latitude (as float) and the longitude (as float)
        double latitude, longitude;
        UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.latitude], out latitude);
        UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.longitude], out longitude);
        return new Vector2((float)latitude, (float)longitude);
    }

    public static (LaunchPad,bool) GetLaunchPadFromName(string launchPadName, UniCsts.planets planetOfTheLaunchPad, bool alsoSearchInCustomLaunchPads)
    {
        // 'Item1' is the returned LaunchPad (or null if it does not exist)
        // 'Item2' is the boolean indicating if the returned LaunchPad is a customOne (thus loaded from the customLaunchPads JSON)
        //==================================
        if(alsoSearchInCustomLaunchPads)
        {
            // Checking custom launchPads
            string filepath = Application.persistentDataPath + Filepaths.userAdded_launchPads;
            bool customLPFileExit = true;
            if(!File.Exists(filepath)) {
                customLPFileExit = false;
            }
            if(customLPFileExit)
            {
                LaunchPad[] loadedLPs = JsonHelper.FromJson<LaunchPad>(File.ReadAllText(filepath));
                foreach(LaunchPad lp in loadedLPs)
                {
                    if(lp.name.Equals(launchPadName))
                        return (lp, false);
                }
            }
        }
        if(!LaunchPadList.launchPadsDict.ContainsKey(planetOfTheLaunchPad))
        {
            // The specified planet has NOT a dictionary of launchPads
            return (null, false);
        }
        Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> dict = LaunchPadList.launchPadsDict[planetOfTheLaunchPad];
        if(dict.ContainsKey(launchPadName))
        {
            return (new LaunchPad(dict[launchPadName]), false);
        }
        // Could not find the desired launchPad
        return (null, false);
    }

    public static LaunchPad[] GetEveryLaunchPadsOfPlanet(UniCsts.planets planetOfTheLaunchPad, bool includeCustomLaunchPads)
    {
        // Returns an array of launchPads included in the specified planet
        //==================================
        List<LaunchPad> lpList = new List<LaunchPad>();
        //=======
        if(includeCustomLaunchPads)
        {
            // Checking custom launchPads
            string filepath = Application.persistentDataPath + Filepaths.userAdded_launchPads;
            if(!File.Exists(filepath)) {}
            else
            {
                LaunchPad[] loadedLPs = JsonHelper.FromJson<LaunchPad>(File.ReadAllText(filepath));
                foreach(LaunchPad lp in loadedLPs)
                {
                    if(lp.planet.Equals(planetOfTheLaunchPad.ToString()))
                        lpList.Add(lp);
                }
            }
        }

        if(!LaunchPadList.launchPadsDict.ContainsKey(planetOfTheLaunchPad)) {
            // The specified planet has NOT a dictionary of launchPads
            return lpList.ToArray();
        }
        Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> dict = LaunchPadList.launchPadsDict[planetOfTheLaunchPad];
        foreach(Dictionary<LaunchPad.launchPadParams, string> lpDict in dict.Values)
        {
            lpList.Add(new LaunchPad(lpDict));
        }
        return lpList.ToArray();
    }

    public static string[] GetEveryLaunchPadsNamesOfPlanet(UniCsts.planets planetOfTheLaunchPad, bool includeCustomLaunchPads)
    {
        // Returns an array of launchPads included in the specified planet
        //==================================
        List<string> lpList = new List<string>();
        //=======
        if(includeCustomLaunchPads)
        {
            // Checking custom launchPads
            string filepath = Application.persistentDataPath + Filepaths.userAdded_launchPads;
            if(!File.Exists(filepath)) {}
            else
            {
                LaunchPad[] loadedLPs = JsonHelper.FromJson<LaunchPad>(File.ReadAllText(filepath));
                foreach(LaunchPad lp in loadedLPs)
                {
                    if(lp.planet.Equals(planetOfTheLaunchPad.ToString()))
                        lpList.Add(lp.name);
                }
            }
        }

        if(!LaunchPadList.launchPadsDict.ContainsKey(planetOfTheLaunchPad)) {
            // The specified planet has NOT a dictionary of launchPads
            return lpList.ToArray();
        }
        Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> dict = LaunchPadList.launchPadsDict[planetOfTheLaunchPad];
        foreach(Dictionary<LaunchPad.launchPadParams, string> lpDict in dict.Values)
        {
            lpList.Add(lpDict[launchPadParams.name]);
        }
        return lpList.ToArray();
    }

    public static Vector2d XY_2_LatitudeLongitude(Vector2 xymousePosVec, Vector2 xy_mapSize)
    {
        double latitude = (double)(xymousePosVec.y * 2f / xy_mapSize.y) * 90d;
        // LONGITUDE IS NOT VERY ACCURATE, SHOULD DIG THIS FORMULA
        double longitude = (double)(xymousePosVec.x * 2f / xy_mapSize.x) * 180d;
        return new Vector2d(latitude, longitude);
    }

    public static Dictionary<launchPadParams, string> GetEmptyLaunchPadDict(string planetName)
    {
        Dictionary<LaunchPad.launchPadParams, string> templateDict = new Dictionary<LaunchPad.launchPadParams, string>() {
            { LaunchPad.launchPadParams.isCustomLP, "1" },
            { LaunchPad.launchPadParams.refPlanet, planetName },
            { LaunchPad.launchPadParams.name, newLaunchPadDefaultName },
            { LaunchPad.launchPadParams.country, "" },
            { LaunchPad.launchPadParams.operationalDate, "" },
            { LaunchPad.launchPadParams.supervision, "" },
            { LaunchPad.launchPadParams.latitude, "0" },
            { LaunchPad.launchPadParams.longitude, "0" },
            { LaunchPad.launchPadParams.eastwardBoost, "" }
        };
        return templateDict;
    }
}
