using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

[CreateAssetMenu()]
public class CelestialBodySettings : ScriptableObject, FlyingObjSettings
{
    public bool bodySettingsEditorFoldout;
    public bool orbitInfoShowPredictedOrbitInfo;
    public bool usePredifinedPlanets;

    public UniCsts.planets chosenPredifinedPlanet;

    public double radius = 6370d; // km
    public double radiusU; // Rendered radius in unity, == radius * UniCsts.pl2u

    public OrbitPlane equatorialPlane; // Equatorial plane represented by two vector (forward & right) and a point on the plane

    public double soiRadius = 120_000f; // km
    public double massRatio; // ratio celestialBodyMass/EarthMass

    public double rotationSpeed = 10f; //Rotation speed of the planet, in degree/s
    public Vector3d rotationAxis = new Vector3d(0d, 1d, 0d);

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

    public Dictionary<UniCsts.orbitalParams, double> refDictOrbitalParams;
    public double mu; // in kg.m.s-2 without E13
    public List<CelestialBody> gravBodies = new List<CelestialBody>(); // List of celestial bodies influencing this Celestial Body with their gravitational force (celestial body within their SOIs)
    public Material bodyMaterial;
}
