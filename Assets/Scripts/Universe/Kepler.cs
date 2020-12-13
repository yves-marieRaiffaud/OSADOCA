using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
//using System.IO;
//using System.Linq;

public static class Kepler
{
    public static Vector3d GetGravAcc(Vector3 shipPos, Vector3 orbitedBodyPos, double orbitedBodyMu)
    {
        return GetGravAcc(new Vector3d(shipPos), new Vector3d(orbitedBodyPos), orbitedBodyMu);
    }

    public static Vector3d GetGravAcc(Vector3d shipPos, Vector3d orbitedBodyPos, double orbitedBodyMu)
    {
        Vector3d r = (shipPos - orbitedBodyPos)*1000d;
        return -orbitedBodyMu * r/Mathd.Pow(r.magnitude, 3);
    }
}