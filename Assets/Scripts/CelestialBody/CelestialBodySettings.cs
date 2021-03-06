﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class CelestialBodySettings : ScriptableObject, FlyingObjSettings
{
    // Bool foldouts
    public bool bodySettingsEditorFoldout;
    public bool orbitInfoShowPredictedOrbitInfo;
    public bool usePredifinedPlanets;

    public UniCsts.planets chosenPredifinedPlanet;
    public Dictionary<string, UnitInterface> planetBaseParamsDict;

    public double radiusU; // Rendered radius in unity, == radius * UniCsts.pl2u
    public OrbitPlane equatorialPlane; // Equatorial plane represented by two vector (forward & right) and a point on the plane

    public double rotationSpeed = 0f; //Rotation speed of the planet, in degree/s, calculated from the sidereal rotation period

    public float cullingMinAngle = 1.2f; // radian
    public float planetRefreshPeriod = 0.5f; // s
    public float[] detailLevelDistances = new float[] {
        Mathf.Infinity,
        2000f,
        1000f,
        100f,
        10f,
        5f,
        1f,
    };

    public List<CelestialBody> gravBodies = new List<CelestialBody>(); // List of celestial bodies influencing this Celestial Body with their gravitational force (celestial body within their SOIs)
    public Material bodyMaterial;
    public Material sphereTemplateMaterial;
    public Texture2D heightMap;
}
