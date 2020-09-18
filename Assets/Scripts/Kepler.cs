using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.Linq;

public static class Kepler
{
    /// <summary>
    /// r should be in meters and retrieved with the method 'Get_RadialVec' from the FlyingObjCommonParams interface.
    /// Returns the acceleration of 'orbitingBody' in m/s^2
    /// </summary>
    public static Vector3d GravitationalAcc<T1>(CelestialBody pullingBody, T1 orbitingBody, Vector3d r, bool saveAccToOrbitingBodyParam)
    where T1: FlyingObjCommonParams
    {
        double dstPow3 = Mathd.Pow(r.magnitude, 3); // m^3
        double mu = pullingBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()].value;
        Vector3d acc =  - UniCsts.µExponent * mu * r / dstPow3; // m.s-2

        if(!Vector3d.IsValid(acc) || UsefulFunctions.DoublesAreEqual(dstPow3, 0d)) {
            Debug.LogError("Acc is not valid or distance between the pulling body and the target body is null");
            acc = Vector3d.positiveInfinity;
        }
        if(saveAccToOrbitingBodyParam) {
            orbitingBody.orbitedBodyRelativeAcc = acc;
        }
        return acc;
    }

    /*
    /// <summary>
    /// Compute gravitationnal pull from a CelestialBody on a Spaceship or a CelestialBody
    /// T1: The type of the argument 'orbitingBody': either 'Spaceship' or 'CelestialBody'
    /// T2: The type of the orbitingBody Settings variable: either 'SpaceshipSettings' or 'CelestialBodySettings'
    /// </summary>
    /// <param name="pullingBody">A CelestialBody that is pulling the orbiting body</param>
    /// <param name="orbitingBody">The orbiting body, either a 'Spaceship' or a 'CelestialBody'</param>
    public Vector3d ComputeGravitationalAcc<T1, T2>(CelestialBody pullingBody, T1 orbitingBody, T2 settings, bool saveAccToOrbitingBodyParam)
    where T1: FlyingObjCommonParams where T2: FlyingObjSettings
    {
        Vector3d pullinBodyPos = new Vector3d(pullingBody.transform.position);
        Transform castOrbitingBodyTr = orbitingBody._gameObject.transform; // Spaceship Tr or CelestialBody Tr

        Vector3d r = new Vector3d(castOrbitingBodyTr.position) - pullinBodyPos;
        double scalingFactor = UniCsts.u2au * UniCsts.au2km; // km, for planets

        if(orbitingBody.orbitalParams.orbParamsUnits == OrbitalTypes.orbitalParamsUnits.km_degree &&
            orbitingBody.orbitalParams.orbitedBodyName == pullingBody.name)
        {
            scalingFactor = UniCsts.u2pl; // km, for spaceships
        }

        r *= scalingFactor; // km
        double dstPow3 = Mathd.Pow(r.magnitude, 3); // km^3
        double mu = pullingBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()].value;
        Vector3d acc =  - Mathd.Pow(10,7) * mu * r / dstPow3; // m.s-2

        if(!Vector3d.IsValid(acc) || UsefulFunctions.DoublesAreEqual(dstPow3, 0d)) {
            Debug.LogError("Acc is not valid or distance between the pulling body and the target body is null");
            acc = Vector3d.positiveInfinity;
        }
        if(saveAccToOrbitingBodyParam) {
            orbitingBody.orbitedBodyRelativeAcc = acc;
        }
        return acc;
    }
    */
}