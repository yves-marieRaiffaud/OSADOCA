using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mathd_Lib;

public static class UniverseConstants
{
    public const int UI_SIGNIFICANT_DIGITS = 6;
    //-----------------------------------------------
    public const double G = 6.67430d; //E-11f; // m3 kg−1 s−2
    public const double earthMass = 5.97d; //E24 kg
    public const double earthMassExponent = 1_000_000_000_000_000_000_000_000d; // E24

    public const double µExponent = 10_000_000_000_000d; // == E13

    public const double rad2deg = 180d/Mathd.PI;
    public const double deg2rad = 1d/rad2deg;


    // Max distance before offseting every object to bring the camera back at the origin, in Unity units
    public const double dstThreshold = 500d;
}