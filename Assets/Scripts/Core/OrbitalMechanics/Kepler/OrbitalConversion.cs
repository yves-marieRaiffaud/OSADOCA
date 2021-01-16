using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using UniCsts = UniverseConstants;

namespace Kepler.OrbitalConversions
{
    public static class OrbitShape
    {
        /// <summary>
        /// Computes the orbit's eccentricity from the apoapsis and periapsis radius (from center to center)
        /// </summary>
        public static double rarp_2_e(double ra, double rp) {
            return (ra-rp)/(ra+rp);
        }
        /// <summary>
        /// Computes the orbit's periapsis radius from the apoapsis radius and the eccentricity
        /// </summary>
        public static double erp_2_ra(double e, double rp) {
            return rp*(1+e)/(1-e);
        }

        /// <summary>
        /// Computes the orbit's semilatus rectum 'p' from the apoapsis and periapsis radius (from center to center)
        /// </summary>
        public static double rarp_2_p(double ra, double rp) {
            return 2*ra*rp/(ra+rp);
        }
        /// <summary>
        /// Computes the orbit's semilatus rectum from the eccentricity and the periapsis radius
        /// </summary>
        public static double erp_2_p(double e, double rp) {
            return rp*(1+e);
        }

        /// <summary>
        /// Computes the orbit's periapsis radius from the eccentricity and the semilatus rectum
        /// </summary>
        public static double ep_2_rp(double e, double p) {
            return p/(1+e);
        }

        /// <summary>
        /// Computes the orbit's semi-major axis from the apoapsis and periapsis radius
        /// </summary>
        public static double rarp_2_a(double ra, double rp) {
            return (ra+rp)/2;
        }
        /// <summary>
        /// Computes the orbit's semi-minor axis from the semilatus rectum and the semi-major axis
        /// </summary>
        public static double pa_2_b(double p, double a) {
            return Mathd.Sqrt(p*a);
        }
        /// <summary>
        /// Computes the foci distance from the orbit center, from the eccentricity and the semi-major axis
        /// </summary>
        public static double ea_2_c(double e, double a) {
            return e*a;
        }
        
    }

    public static class OrbitPosition
    {
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute the true anomaly 'nu' in RAD from a radial vector 'r' in m, an excentricity vector 'e' and a velocity vector 'v' in m/s
        /// </summary>
        public static double rv_2_nu(Vector3d r, Vector3d e, Vector3d v) {
            double nu = Mathd.Acos(Vector3d.Dot(e, r) / (e.magnitude * r.magnitude));
            if(Vector3d.Dot(r, v) < 0d)
                return 2d*Mathd.PI - nu;
            else
                return nu;
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute the true anomaly 'nu' in RAD from the passed eccentricity 'e' and the eccentric anomaly 'E' in RAD
        /// </summary>
        public static double E_2_nu(double E, double e) {
            return 2d * Mathd.Atan((Mathd.Sqrt((1d+e)/(1d-e)) * Mathd.Tan(E/2d)));
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the eccentric anomaly 'E' in RAD from the true anomaly 'nu' in RAD and the orbit's eccentricity 'e'
        /// </summary>
        public static double nu_2_E(double nu, double e) {
            return 2d * Mathd.Atan(Mathd.Sqrt((1d-e)/(1d+e)) * Mathd.Tan(nu/2d));
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean anomaly 'M' in RAD from the mean anomaly 'M' in RAD and the orbit's eccentricity 'e'. Compute the eccentric anomaly iN RAD as an intermediate variable in the conversion. First returned value is the true anomaly 'nu' in RAD. Second returned value is the eccentric anomaly in RAD (calculated as an intermediate variable)
        /// </summary>
        public static (double,double) M_2_nu(double M, double e) {
            double E = M_2_E(M, e);
            return (E_2_nu(E, e), E);
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute the time before/after perigee (depending on the sign of t) from the true anomaly 'nu', the orbit's eccentricity 'e' and the orbital period 'T' in seconds.
        /// Returns the time t in seconds.
        /// </summary>
        public static double nu2t(double nu, double e, double T) {
            (double, double) items = nu_2_M(nu, e);
            double M = items.Item1;
            return MT_2_t(M, T);
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute iteratively the eccentric anomaly 'E' in RAD from the mean anomaly 'M' in RAD and from the orbit's eccentricity. Optional parameter is the tolerance value to define when the iterative result of E is acceptable
        /// </summary>
        public static double M_2_E(double M, double e, double tolerance=0.001d) {
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
        public static double E_2_M(double E, double e) {
            return E - e*Mathd.Sin(E);
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean anomaly 'M' in RAD from the specified true anomaly 'nu' in RAD and the orbit's eccentricity 'e'. First returned value is the mean anomaly 'M' in RAD. Second returned value is the eccentric anomaly in RAD (calculated as an intermediate variable)
        /// </summary>
        public static (double,double) nu_2_M(double nu, double e) {
            double E = nu_2_E(nu, e);
            return (E_2_M(E, e), E);
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean anomaly 'M' in RAD from the time of fligth (in seconds) from the orbit's origin and from the orbital period 'T' in seconds. Performs a simple cross-multiplication
        /// </summary>
        public static double t_2_M(double t, double T) {
            return t * 2d*Mathd.PI / T;
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the true anomaly 'nu' in RAD from the time of flight (in seconds) from the orbit's origin, from the orbital period T in second, and the orbit's eccentricity. First returned value is the true anomaly 'nu' in RAD. Second returned value is the eccentric anomaly 'E' in RAD (calculated as an intermediate variable). Third returned value is the mean anomaly 'M' in RAD
        /// </summary>
        public static (double, double, double) t_2_nuEM(double t, double T, double e) {
            double M = t_2_M(t, T);
            (double,double) nu_E = M_2_nu(M, e);
            return (nu_E.Item1, nu_E.Item2, M);
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the time in second from the orbit's origin from the mean anomaly 'M' in RAD and the orbital period 'T' in seconds. Performs a simple cross mulitplication
        /// </summary>
        public static double MT_2_t(double M, double T) {
            return T*M/(2d*Mathd.PI);
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean longitude 'L' in RAD from the longitude of the ascending node 'lAscN' in RAD, the argument of the perihelion 'omega' in RAD and the mean anomaly 'M' in RAD.
        /// </summary>
        public static double lAscN_omegaM_2_L(double lAscN, double omega, double M) {
                return lAscN + omega + M;
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Computes the mean anomaly 'M' in RAD from the longitude of the ascending node 'lAscN' in RAD, the argument of the perihelion 'omega' in RAD and the mean longitude 'L' in RAD.
        /// </summary>
        public static double L_2_M(double L, double lAscN, double omega) {
            return L - lAscN - omega;
        }
        /// <summary>
        /// ONLY FOR ELLIPCTIC ORBIT. Compute the norm of the radial vector 'r' from the norm of the semi-major axis 'a', the norm of the excentricty 'e' and the true anomaly 'nu' in RAD
        /// </summary>
        public static double aenu_2_r(double a, double e, double nu) {
            return a*(1d-e*e)/(1d + Mathd.Cos(nu));
        }
    }

    public static class OrbitTime
    {
        /// <summary>
        /// Computes orbital period in seconds from the semi-major axis in meters and the orbitedBody's standard gravitational parameter, in m3/s2 (without its µExponent)
        /// </summary>
        public static double amu_2_T(double a, double mu) {
            return 2d*Mathd.PI*Mathd.Sqrt(Mathd.Pow(a,3)/(mu*UniCsts.µExponent));
        }

        /// <summary>
        /// Computes the mean motion in rad/s from the orbital period in seconds
        /// </summary>
        public static double T_2_n(double T) {
            return 2d*Mathd.PI/T;
        }
        /// <summary>
        /// Computes the orbital period in seconds from the mean motion in rad/s
        /// </summary>
        public static double n_2_T(double n) {
            return 2d*Mathd.PI/n;
        }
        

    }
}