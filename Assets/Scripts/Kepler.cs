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
        double mu = pullingBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()].value;
        Vector3d acc = RawGravitationalAcc(r, mu);

        if(acc == Vector3d.positiveInfinity)
            Debug.LogError("Acc is not valid or distance between the pulling body and the target body is null");

        if(saveAccToOrbitingBodyParam)
            orbitingBody.orbitedBodyRelativeAcc = acc;

        return acc;
    }

    /// <summary>
    /// Method for the non-main Thread. 'r' should be in meters and retrieved with the method 'Get_RadialVec' from the FlyingObjCommonParams interface. 'mu' should be retrived from the orbited CelestialBody 'planetBaseParamsDict'
    /// Returns the acceleration of 'orbitingBody' in m/s^2
    /// </summary>
    public static Vector3d RawGravitationalAcc(Vector3d r, double mu)
    {
        double dstPow3 = Mathd.Pow(r.magnitude, 3); // m^3
        Vector3d acc = -UniCsts.µExponent * mu * r / dstPow3; // m.s^-2

        if(!Vector3d.IsValid(acc) || UsefulFunctions.DoublesAreEqual(dstPow3, 0d))
            acc = Vector3d.positiveInfinity;
        return acc;
    }

    public static class OrbitalParamsConversion
    {
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute the true anomaly 'nu' in RAD from a radial vector 'r' in m, an excentricity vector 'e' and a velocity vector 'v' in m/s
        /// </summary>
        public static double rv2nu(Vector3d r, Vector3d e, Vector3d v) {
            double nu = Mathd.Acos(Vector3d.Dot(e, r) / (e.magnitude * r.magnitude));
            if(Vector3d.Dot(r, v) < 0d)
                return 2d*Mathd.PI - nu;
            else
                return nu;
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute the true anomaly 'nu' in RAD from the passed eccentricity 'e' and the eccentric anomaly 'E' in RAD
        /// </summary>
        public static double E2nu(double E, double e) {
            return 2d * Mathd.Atan((Mathd.Sqrt((1d+e)/(1d-e)) * Mathd.Tan(E/2d)));
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the eccentric anomaly 'E' in RAD from the true anomaly 'nu' in RAD and the orbit's eccentricity 'e'
        /// </summary>
        public static double nu2E(double nu, double e) {
            return 2d * Mathd.Atan(Mathd.Sqrt((1d-e)/(1d+e)) * Mathd.Tan(nu/2d));
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean anomaly 'M' in RAD from the mean anomaly 'M' in RAD and the orbit's eccentricity 'e'. Compute the eccentric anomaly iN RAD as an intermediate variable in the conversion. First returned value is the true anomaly 'nu' in RAD. Second returned value is the eccentric anomaly in RAD (calculated as an intermediate variable)
        /// </summary>
        public static (double,double) M2nu(double M, double e) {
            double E = M2E(M, e);
            return (E2nu(E, e), E);
        }

        public static double nu2t(double nu, double e, double T) {
            (double, double) items = nu2M(nu, e);
            double M = items.Item1;
            return MT2t(M, T);
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute iteratively the eccentric anomaly 'E' in RAD from the mean anomaly 'M' in RAD and from the orbit's eccentricity. Optional parameter is the tolerance value to define when the iterative result of E is acceptable
        /// </summary>
        public static double M2E(double M, double e, double tolerance=0.001d) {
            double Ei = M;
            if(e > 0.8f)
                Ei = Mathd.PI;
            double rhs = 1d;
            int count = 0;
            while(Mathd.Abs(rhs) > tolerance) {
                rhs = (Ei - (M + e*Mathd.Sin(Ei))) / (1d - e*Mathd.Cos(Ei));
                Ei = Ei - rhs;
                count++;
            }
            return Ei;
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the Mean anomaly 'M' in RAD from the eccentric anomaly 'E' in RAD and the orbit's eccentricity 'e'
        /// </summary>
        public static double E2M(double E, double e) {
            return E - e*Mathd.Sin(E);
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean anomaly 'M' in RAD from the specified true anomaly 'nu' in RAD and the orbit's eccentricity 'e'. First returned value is the mean anomaly 'M' in RAD. Second returned value is the eccentric anomaly in RAD (calculated as an intermediate variable)
        /// </summary>
        public static (double,double) nu2M(double nu, double e) {
            double E = nu2E(nu, e);
            return (E2M(E, e), E);
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean anomaly 'M' in RAD from the time of fligth (in seconds) from the orbit's origin and from the orbital period 'T' in seconds. Performs a simple cross-multiplication
        /// </summary>
        public static double t2M(double t, double T) {
            return t * 2d*Mathd.PI / T;
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the true anomaly 'nu' in RAD from the time of flight (in seconds) from the orbit's origin, from the orbital period T in second, and the orbit's eccentricity. First returned value is the true anomaly 'nu' in RAD. Second returned value is the eccentric anomaly 'E' in RAD (calculated as an intermediate variable). Third returned value is the mean anomaly 'M' in RAD
        /// </summary>
        public static (double, double, double) t2nuEM(double t, double T, double e) {
            double M = t2M(t, T);
            (double,double) nu_E = M2nu(M, e);
            return (nu_E.Item1, nu_E.Item2, M);
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the time in second from the orbit's origin from the mean anomaly 'M' in RAD and the orbital period 'T' in seconds. Performs a simple cross mulitplication
        /// </summary>
        public static double MT2t(double M, double T) {
            return T*M/(2d*Mathd.PI);
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean longitude 'L' in RAD from the longitude of the ascending node 'lAscN' in RAD, the argument of the perihelion 'omega' in RAD and the mean anomaly 'M' in RAD.
        /// </summary>
        public static double lAscN_omegaM2L(double lAscN, double omega, double M) {
                return lAscN + omega + M;
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean anomaly 'M' in RAD from the longitude of the ascending node 'lAscN' in RAD, the argument of the perihelion 'omega' in RAD and the mean longitude 'L' in RAD.
        /// </summary>
        public static double L2M(double L, double lAscN, double omega) {
            return L - lAscN - omega;
        }

        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute the norm of the radial vector 'r' from the norm of the semi-major axis 'a', the norm of the excentricty 'e' and the true anomaly 'nu' in RAD
        /// </summary>
        public static double aenu2r(double a, double e, double nu) {
            return a*(1d-e*e)/(1d + Mathd.Cos(nu));
        }
    }

}