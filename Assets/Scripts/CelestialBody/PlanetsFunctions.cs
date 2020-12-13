using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mathd_Lib;
using TFunc = System.Func<dynamic, dynamic, dynamic>;

public static class PlanetsFunctions
{
    public enum chosenFunction { pressureEvolution };
    public static Dictionary<chosenFunction, TFunc> earthFcnsDict = new Dictionary<chosenFunction, TFunc>()
    {
        { chosenFunction.pressureEvolution, (surfPressure, inputAlt) => {return EarthPressureEvolution(surfPressure, inputAlt);} }
    };
    //----------------------------------------------
    //----------------------------------------------
    public static Pressure EarthPressureEvolution(Pressure surfacePressure, double inputAltitude)
    {
        // inputAltitude is the altitude in km from the surface of the planet
        double pressureAtAlt = surfacePressure.val / (Mathd.Pow(2, (inputAltitude/5d)));
        Pressure newPressure = new Pressure(pressureAtAlt, surfacePressure.unit);
        return newPressure;
    }
}