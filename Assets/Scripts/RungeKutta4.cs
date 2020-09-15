using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
using System.Linq;

public class RungeKutta4<T>
where T: FlyingObjCommonParams
{
    CelestialBody orbitedBody;
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
        orbitingBody = _orbitingBody;
        h = _timestep;

        if(_X == null)
            _X = new Vector3d[2];
        
        _X[0] = orbitingBody.Get_RadialVec()*1_000d; // m
        _X[1] = orbitedBody.orbitedBodyRelativeVel; // m/s
    }

    public string PrintState()
    {
        string txt = "[" + _X[0][0] + ";" + _X[0][1] + ";" + _X[0][2] + "]\n" + "[" + _X[1][0] + ";" + _X[1][1] + ";" + _X[1][2] + "]";
        Debug.Log(txt);
        return txt;
    }

    /// <summary>
    /// Integrate the equation of motion.
    /// Inputs: 'rVec': radial vector of the body to propagate.
    ///         'vVec': velocity vector of the body to propagate.
    ///         't': integration time.
    ///         'h': integration timestep.
    /// </summary>
    public void Step()
    {
        Vector3d[] tmp = _X;

        Vector3d[] k1 = GetKRKV(tmp);
        tmp[0] = tmp[0] + k1[0]*h/2d;
        tmp[1] = tmp[1] + k1[1]*h/2d;

        Vector3d[] k2 = GetKRKV(tmp);
        tmp[0] = tmp[0] + k2[0]*h/2d;
        tmp[1] = tmp[1] + k2[1]*h/2d;

        Vector3d[] k3 = GetKRKV(tmp);
        tmp[0] = tmp[0] + k3[0]*h;
        tmp[1] = tmp[1] + k3[1]*h;

        Vector3d[] k4 = GetKRKV(tmp);

        _X[0] += (h/6d)*(k1[0] + 2*k2[0] + 2*k3[0] + k4[0]);
        _X[1] += (h/6d)*(k1[1] + 2*k2[1] + 2*k3[1] + k4[1]);
    }

    private Vector3d[] GetKRKV(Vector3d[] X)
    {
        Vector3d r = X[0];
        Vector3d v = X[1];

        Vector3d kr = Kepler.GravitationalAcc<T>(orbitedBody, orbitingBody, r, false);

        Vector3d[] krkv = new Vector3d[2];
        krkv[0] = v;
        krkv[1] = kr;
        return krkv;
    }

    /*
    nbSteps = 1000;
    h = 20;
    disp('Initial conditions : ');
    X = [x0;v0]
    for t=1:nbSteps
        X = rk4Orbit(X,h);
    end

    function X = rk4Orbit(X,h)
        % X is the state matrix
        % h is the timestep
        
        k1 = GetKRKV(X);        % k1
        k2 = GetKRKV(X+k1*h/2); % k2
        k3 = GetKRKV(X+k2*h/2); % k3
        k4 = GetKRKV(X+k3*h);   % k2
        
        X = X + (h/6)*(k1 + 2*k2 + 2*k3 + k4);
    end

    function krkv = GetKRKV(X)
        r = X(1,:);
        v = X(2,:);
        
        muEarth = 3.98e14;
        kr = (-r*muEarth/norm(r)^3);
        krkv = [v;kr];
    end
    */
}