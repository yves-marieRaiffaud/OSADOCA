using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using Fncs = UsefulFunctions;

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

    public static class OrbitalParamsConversion
    {
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute the true anomaly 'nu' in RAD from a radial vector 'r', an excentricity vector 'e' and a velocity vector 'v'
        /// </summary>
        public static double rve2nu(Vector3d r, Vector3d e, Vector3d v) {
            double nu = Mathd.Acos(Vector3d.Dot(e, r) / (e.magnitude * r.magnitude));
            if(Vector3d.Dot(r, v) < 0d)
                return 2d*Mathd.PI - nu;
            else
                return nu;
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute the true anomaly 'nu' in RAD from the passed excentricity 'e' and the eccentric anomaly 'E' in RAD
        /// </summary>
        public static double eE2nu(double e, double E) {
            return 2d * Mathd.Atan((Mathd.Sqrt((1d+e)/(1d-e)) * Mathd.Tan(E/2d)));
        }

        /// <summary>
        /// Computes the Mean anomaly 'M' in RAD from the eccentric anomaly 'E'
        /// </summary>
        public static double eE2M(double e, double E) {
            return E - e*Mathd.Sin(E);
        }

        /// <summary>
        /// Compute the norm of the radial vector 'r' from the norm of the semi-major axis 'a', the norm of the excentricty 'e' and the true anomaly 'nu' in RAD
        /// </summary>
        public static double aenu2r(double a, double e, double nu) {
            return a*(1d-e*e)/(1d + Mathd.Cos(nu));
        }
    }
}