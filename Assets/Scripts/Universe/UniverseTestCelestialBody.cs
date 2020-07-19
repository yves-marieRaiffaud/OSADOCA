using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UniverseTestCelestialBody : MonoBehaviour
{
    public GameObject playerCamera; // Camera attached to the debugging spacecraft
    //=========================================
    void Start()
    {
        CelestialBody celestBody = GameObject.Find("EarthPlanet").GetComponent<CelestialBody>();
        celestBody.AwakeCelestialBody(UniCsts.planetsDict[celestBody.settings.chosenPredifinedPlanet]);
    }
}