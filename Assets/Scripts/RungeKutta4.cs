using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

public class RungeKutta4<T>
where T: FlyingObjCommonParams
{
    CelestialBody orbitedBody;
    double orbitedBody_mu;
    T orbitingBody;
    // Timestep
    double h;
    // State array, first row is rVec, second row is vVec
    Vector3d[] _X;
    /// <summary>
    /// State array at current time, first row is radialVec, second row is velocityVec
    /// </summary>
    public Vector3d[] X
    {
        get {
            return _X;
        }
    }

    public RungeKutta4(CelestialBody _pullingBody, T _orbitingBody, double _timestep)
    {
        orbitedBody = _pullingBody;
        orbitedBody_mu = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()].value;
        orbitingBody = _orbitingBody;
        h = _timestep;

        if(_X == null)
            _X = new Vector3d[2];

        _X[0] = orbitingBody.Get_RadialVec()*UniCsts.km2m; // m
        _X[1] = orbitingBody.orbitedBodyRelativeVel; // m/s
        PrintState();
    }

    /// <summary>
    /// Integrate the equation of motion.
    /// Inputs: 'rVec': radial vector of the body to propagate.
    ///         'vVec': velocity vector of the body to propagate.
    ///         't': integration time.
    ///         'h': integration timestep.
    /// </summary>
    public Vector3d[] Step()
    {
        Vector3d[] temp = _X;
        Vector3d[] k1 = GetKRKV(temp, true);

        temp[0] = _X[0] + k1[0]*h/2d;
        temp[1] = _X[1] + k1[1]*h/2d;
        Vector3d[] k2 = GetKRKV(temp, true);

        temp[0] = _X[0] + k2[0]*h/2d;
        temp[1] = _X[1] + k2[1]*h/2d;
        Vector3d[] k3 = GetKRKV(temp, true);

        temp[0] = _X[0] + k3[0]*h;
        temp[1] = _X[1] + k3[1]*h;
        Vector3d[] k4 = GetKRKV(temp, true);

        _X[0] += (h/6d) * (k1[0] + 2*k2[0] + 2*k3[0] + k4[0]);
        _X[1] += (h/6d) * (k1[1] + 2*k2[1] + 2*k3[1] + k4[1]);

        return _X;
    }

    private Vector3d[] GetKRKV(Vector3d[] X, bool saveToBodyAcc)
    {
        Vector3d r = X[0]; // m
        Vector3d v = X[1]; // m/s

        Vector3d kr = Kepler.RawGravitationalAcc(r, orbitedBody_mu);

        Vector3d[] krkv = new Vector3d[2];
        krkv[0] = v; // m/s
        krkv[1] = kr; // m.s^-2
        return krkv;
    }

    public string PrintState()
    {
        string txt = orbitingBody._gameObject.name +  " rk4 State:\n[" + _X[0][0] + ";" + _X[0][1] + ";" + _X[0][2] + "]\n" + "[" + _X[1][0] + ";" + _X[1][1] + ";" + _X[1][2] + "]";
        Debug.Log(txt);
        return txt;
    }
}