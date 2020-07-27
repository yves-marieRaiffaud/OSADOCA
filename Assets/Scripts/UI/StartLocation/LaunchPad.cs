using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Mathd_Lib;
using System.Globalization;

public class LaunchPad
{
    public enum launchPadParams { refPlanet, name, country, operationalDate, supervision, longitude, latitude, eastwardBoost };

    private Vector2 lp_XYMapPos; // Position on the map of the planet of the launch pad (x & y coordinate)
    public Dictionary<launchPadParams, string> launchPadParamsDict;

    public LaunchPad(Dictionary<launchPadParams, string> ref_launchPadParamsDict)
    {
        launchPadParamsDict = ref_launchPadParamsDict;
        string planetName = launchPadParamsDict[launchPadParams.refPlanet];
        if(!launchPadParamsDict.ContainsKey(launchPadParams.eastwardBoost))
        {
            ComputeEastwardBoost(UniCsts.planetsDict[UsefulFunctions.CastStringTo_Unicsts_Planets(planetName)]);
        }
    }

    private double ComputeEastwardBoost(Dictionary<string, double> refPlanetDict)
    {
        // Eastward boost (effet de fronde) is useful when launching satellites to an equatorial orbit (such as GEO)
        // Eastward boost computed in m/s
        double lat;
        UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.latitude], out lat);
        lat *= UniCsts.deg2rad;

        // First compute the geocentric radius depending on the latitude of the launch pad
        double geocentricRadius = ComputeGeocentricRadius(refPlanetDict, lat);
        geocentricRadius *= UniCsts.km2m;

        // Finally compute the eastward boost
        double eastward_boost = Mathd.Cos(lat) * 2d * Mathd.PI * geocentricRadius / 86400d; // in m/s
        launchPadParamsDict.Add(launchPadParams.eastwardBoost, eastward_boost.ToString("G", CultureInfo.InvariantCulture));
        return eastward_boost;
    }

    private double ComputeGeocentricRadius(Dictionary<string, double> refPlanetDict, double lat)
    {
        // equatorialRadius & polarRadius in km 
        // Latitude in radian
        double equa_radius = refPlanetDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
        double polar_radius = refPlanetDict[CelestialBodyParamsBase.planetaryParams.polarRadius.ToString()];

        double numerator = Mathd.Pow(equa_radius*equa_radius * Mathd.Cos(lat), 2) + Mathd.Pow(polar_radius*polar_radius * Mathd.Sin(lat), 2);
        double denominator = Mathd.Pow(equa_radius * Mathd.Cos(lat), 2) + Mathd.Pow(polar_radius * Mathd.Sin(lat), 2);
        double geocentric_radius = Mathd.Sqrt(numerator / denominator);
        return geocentric_radius;
    }

    public Vector2 LatitudeLongitude_2_XY(Vector2 xy_mapSize)
    {
        // 'xy_mapSize' is the width and height of the destination map (i.e the earth flat map on which the sprites of coordinates X/Y should be drawn)

        double lat, longitude;
        bool latSuccess = UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.latitude], out lat);
        bool longSuccess = UsefulFunctions.ParseStringToDouble(launchPadParamsDict[launchPadParams.longitude], out longitude);

        // COnvert lat & long to a 3D point on a unit sphere
        Vector3 worldPos = (Quaternion.AngleAxis((float)longitude, Vector3.up) * Quaternion.AngleAxis((float)lat, Vector3.right) * new Vector3(0,0,1));

        // Convert the 3D point on the map
        // Computed 'pixel_w' & "pixel_h' are relative to the bottom left corner of the map
        float pixel_w = (0.5f + Mathf.Atan2(worldPos.x, worldPos.z) / (2f*Mathf.PI)) * xy_mapSize.x;
        float pixel_h = (0.5f - Mathf.Asin(worldPos.y) / Mathf.PI) * xy_mapSize.y;

        return new Vector2(pixel_w, pixel_h);
    }
}
