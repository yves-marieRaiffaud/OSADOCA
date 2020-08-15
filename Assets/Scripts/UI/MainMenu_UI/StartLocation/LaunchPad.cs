using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Mathd_Lib;
using System.Globalization;

public class LaunchPad
{
    public static string newLaunchPadDefaultName = "New LaunchPad";
    public enum launchPadParams { isCustomLP, refPlanet, name, country, operationalDate, supervision, longitude, latitude, eastwardBoost };

    private Vector2 lp_XYMapPos; // Position on the map of the planet of the launch pad (x & y coordinate)
    public Dictionary<launchPadParams, string> launchPadParamsDict;

    public LaunchPad(Dictionary<launchPadParams, string> ref_launchPadParamsDict)
    {
        launchPadParamsDict = ref_launchPadParamsDict;
        string planetName = launchPadParamsDict[launchPadParams.refPlanet];

        if(!launchPadParamsDict.ContainsKey(launchPadParams.eastwardBoost)
            && launchPadParamsDict[launchPadParams.latitude] != ""
            && launchPadParamsDict[launchPadParams.longitude] != "")
        {
            ComputeEastwardBoost();
        }
    }

    public double ComputeEastwardBoost()
    {
        // Eastward boost (effet de fronde) is useful when launching satellites to an equatorial orbit (such as GEO)
        // Eastward boost computed in m/s
        string planetName = launchPadParamsDict[launchPadParams.refPlanet];
        double equaRadius = UniCsts.planetsDict[UsefulFunctions.CastStringTo_Unicsts_Planets(planetName)][CelestialBodyParamsBase.planetaryParams.radius.ToString()].value;
        double polarRadius = UniCsts.planetsDict[UsefulFunctions.CastStringTo_Unicsts_Planets(planetName)][CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()].value;

        double lat;
        UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.latitude], out lat);
        lat *= UniCsts.deg2rad;

        // First compute the geocentric radius depending on the latitude of the launch pad
        double geocentricRadius = ComputeGeocentricRadius(equaRadius, polarRadius, lat);
        geocentricRadius *= UniCsts.km2m;

        // Finally compute the eastward boost
        double eastward_boost = Mathd.Cos(lat) * 2d * Mathd.PI * geocentricRadius / 86400d; // in m/s
        
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

    public static LaunchPad GetLaunchPadFromName(string launchPadName, UniCsts.planets planetOfTheLaunchPad)
    {
        Dictionary<string, Dictionary<LaunchPad.launchPadParams, string>> dict = LaunchPadList.launchPadsDict[planetOfTheLaunchPad];
        return new LaunchPad(dict[launchPadName]);
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

    public string GetJSONLaunchPadPropertyName()
    {
        // Returns the string of the LaunchPad property Name that is used in the JSON file to save the custom user added launchPads
        return launchPadParamsDict[launchPadParams.name] + "_" + launchPadParamsDict[launchPadParams.refPlanet];
    } 

    public string LaunchPadDataToString()
    {
        string output = "";

        // Addign the first line of the new JSON object: the name of its property is its launchPad name and the name of the refPlanet
        string propertyName = GetJSONLaunchPadPropertyName();
        output += "\n\t\"" + propertyName + "\" : \n\t{\n";
        
        int count = 0;
        foreach(KeyValuePair<launchPadParams, string> keyValuePair in launchPadParamsDict)
        {
            if(count != 0) {
                output += ",\n";
            }
            output +=  "\t\t\"" + keyValuePair.Key.ToString() + "\" : \"" + keyValuePair.Value.ToString() + "\"";
            count++;
        }
        output += "\n\t}\n";
        return output;
    }
}
