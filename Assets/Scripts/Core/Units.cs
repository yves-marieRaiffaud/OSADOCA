using System.Collections;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using UnityEngine;
using Mathd_Lib;
using MathOps = CommonMethods.MathsOps;

[Serializable]
public class DoubleNoDim : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
        set {
            _val=value;
        }
    }

    [SerializeField]
    Units.noDimension _unit;
    public Units.noDimension unit
    {
        get {
            return _unit;
        }
    }

    public DoubleNoDim(double _value) {
        _val = _value;
        _unit = Units.noDimension.NoDim;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.noDimension thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }
};

[Serializable]
public class Pressure : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
        set {
            _val=value;
        }
    }

    [SerializeField]
    Units.pressure _unit;
    public Units.pressure unit
    {
        get {
            return _unit;
        }
    }

    public Pressure(double value, Units.pressure unit) {
        _val = value;
        _unit = unit;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.pressure thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Pressure ConvertTo(Units.pressure outputUnit) {
        return new Pressure(Units.ConvertPressure(unit, outputUnit, val), outputUnit);
    }
};

[Serializable]
public class Distance : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
        set {
            _val=value;
        }
    }

    [SerializeField]
    Units.distance _unit;
    public Units.distance unit
    {
        get {
            return _unit;
        }
    }

    public Distance(double value, Units.distance unit) {
        _val = value;
        _unit = unit;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.distance thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Distance ConvertTo(Units.distance outputUnit) {
        return new Distance(Units.ConvertDistance(unit, outputUnit, val), outputUnit);
    }
};
[Serializable]
public class Distance3d : UnitInterface
{
    [SerializeField]
    Vector3d _val;
    public Vector3d val
    {
        get {
            return _val;
        }
    }
    public Vector3 val_Vec3
    {
        get {
            return (Vector3)_val;
        }
    }

    [SerializeField]
    Units.distance _unit;
    public Units.distance unit
    {
        get {
            return _unit;
        }
    }

    public Distance3d(double __dx, double __dy, double __dz, Units.distance unit) {
        _val.x = __dx;
        _val.y = __dy;
        _val.z = __dz;
        _unit = unit;
    }

    public Distance3d(Vector3d __dVec, Units.distance unit) {
        _val = __dVec;
        _unit = unit;
    }

    public override string ToString() {
        string dxStr = MathOps.DoubleToString(val.x);
        string dyStr = MathOps.DoubleToString(val.y);
        string dzStr = MathOps.DoubleToString(val.z);
        return String.Format("[{0}-{1}-{2}] {3}", dxStr, dyStr, dzStr, unit.ToString());
    }
    public string ToString(int significantDigits) {
        string dxStr = MathOps.DoubleToSignificantDigits(val.x, significantDigits);
        string dyStr = MathOps.DoubleToSignificantDigits(val.y, significantDigits);
        string dzStr = MathOps.DoubleToSignificantDigits(val.z, significantDigits);
        return String.Format("[{0}-{1}-{2}] {3}", dxStr, dyStr, dzStr, unit.ToString());
    }
    public bool HasUnit(Units.distance thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }
    public Distance3d EnsureUnit(Units.distance unitToEnsure) {
        if(!HasUnit(unitToEnsure))
            return ConvertTo(unitToEnsure);
        else
            return this;
    }

    public Distance3d ConvertTo(Units.distance outputUnit) {
        return Units.ConvertDistance3d(unit, outputUnit, val);
    }
};

[Serializable]
public class Angle : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
        set {
            _val=value;
        }
    }

    [SerializeField]
    Units.angle _unit;
    public Units.angle unit
    {
        get {
            return _unit;
        }
    }

    public Angle(double value, Units.angle unit) {
        _val = value;
        _unit = unit;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.angle thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Angle ConvertTo(Units.angle outputUnit) {
        return new Angle(Units.ConvertAngle(unit, outputUnit, val), outputUnit);
    }
};

[Serializable]
public class Time_Class : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
        set {
            _val=value;
        }
    }

    [SerializeField]
    Units.time _unit;
    public Units.time unit
    {
        get {
            return _unit;
        }
    }

    public Time_Class(double value, Units.time unit) {
        _val = value;
        _unit = unit;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.time thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Time_Class ConvertTo(Units.time outputUnit) {
        return new Time_Class(Units.ConvertTime(unit, outputUnit, val), outputUnit);
    }
};

[Serializable]
public class Velocity : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
    }

    [SerializeField]
    Units.velocity _unit;
    public Units.velocity unit
    {
        get {
            return _unit;
        }
    }

    public Velocity(double value, Units.velocity unit) {
        _val = value;
        _unit = unit;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.velocity thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Velocity ConvertTo(Units.velocity outputUnit) {
        return new Velocity(Units.ConvertVelocity(unit, outputUnit, val), outputUnit);
    }
};
[Serializable]
public class Velocity3d : UnitInterface
{
    [SerializeField]
    Vector3d _val;
    public Vector3d val
    {
        get {
            return _val;
        }
    }
    public Vector3 val_Vec3
    {
        get {
            return (Vector3)_val;
        }
    }

    [SerializeField]
    Units.velocity _unit;
    public Units.velocity unit
    {
        get {
            return _unit;
        }
    }

    public Velocity3d(double __vx, double __vy,double __vz, Units.velocity unit) {
        _val.x = __vx;
        _val.y = __vy;
        _val.z = __vz;
        _unit = unit;
    }
    public Velocity3d(Vector3d __vVec, Units.velocity unit) {
        _val = __vVec;
        _unit = unit;
    }

    public override string ToString() {
        string vxStr = MathOps.DoubleToString(val.x);
        string vyStr = MathOps.DoubleToString(val.y);
        string vzStr = MathOps.DoubleToString(val.x);
        return String.Format("[{0}-{1}-{2}] {3}", vxStr, vyStr, vzStr, unit.ToString());
    }
    public string ToString(int significantDigits) {
        string vxStr = MathOps.DoubleToSignificantDigits(val.x, significantDigits);
        string vyStr = MathOps.DoubleToSignificantDigits(val.y, significantDigits);
        string vzStr = MathOps.DoubleToSignificantDigits(val.z, significantDigits);
        return String.Format("[{0}-{1}-{2}] {3}", vxStr, vyStr, vzStr, unit.ToString());
    }
    public bool HasUnit(Units.velocity thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Velocity3d EnsureUnit(Units.velocity unitToEnsure) {
        if(!HasUnit(unitToEnsure))
            return ConvertTo(unitToEnsure);
        else
            return this;
    }

    public Velocity3d ConvertTo(Units.velocity outputUnit) {
        return Units.ConvertVelocity3d(unit, outputUnit, val);
    }
};

[Serializable]
public class Acceleration : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
    }

    [SerializeField]
    Units.acceleration _unit;
    public Units.acceleration unit
    {
        get {
            return _unit;
        }
    }

    public Acceleration(double value, Units.acceleration unit) {
        _val = value;
        _unit = unit;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.acceleration thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Acceleration ConvertTo(Units.acceleration outputUnit) {
        return new Acceleration(Units.ConvertAcceleration(unit, outputUnit, val), outputUnit);
    }
};
[Serializable]
public class Acceleration3d : UnitInterface
{
    [SerializeField]
    Vector3d _val;
    public Vector3d val
    {
        get {
            return _val;
        }
    }
    public Vector3 val_Vec3
    {
        get {
            return (Vector3)_val;
        }
    }

    [SerializeField]
    Units.acceleration _unit;
    public Units.acceleration unit
    {
        get {
            return _unit;
        }
    }

    public Acceleration3d(double __ax, double __ay, double __az, Units.acceleration unit) {
        _val.x = __ax;
        _val.y = __ay;
        _val.z = __az;
        _unit = unit;
    }

    public Acceleration3d(Vector3d __aVec, Units.acceleration unit) {
        _val = __aVec;
        _unit = unit;
    }

    public override string ToString() {
        string axStr = MathOps.DoubleToString(val.x);
        string ayStr = MathOps.DoubleToString(val.y);
        string azStr = MathOps.DoubleToString(val.z);
        return String.Format("[{0}-{1}-{2}] {3}", axStr, ayStr, azStr, unit.ToString());
    }
    public string ToString(int significantDigits) {
        string axStr = MathOps.DoubleToSignificantDigits(val.x, significantDigits);
        string ayStr = MathOps.DoubleToSignificantDigits(val.y, significantDigits);
        string azStr = MathOps.DoubleToSignificantDigits(val.z, significantDigits);
        return String.Format("[{0}-{1}-{2}] {3}", axStr, ayStr, azStr, unit.ToString());
    }
    public bool HasUnit(Units.acceleration thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }
    public Acceleration3d EnsureUnit(Units.acceleration unitToEnsure) {
        if(!HasUnit(unitToEnsure))
            return ConvertTo(unitToEnsure);
        else
            return this;
    }

    public Acceleration3d ConvertTo(Units.acceleration outputUnit) {
        return Units.ConvertAcceleration3d(unit, outputUnit, val);
    }
};

[Serializable]
public class Force : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
    }

    [SerializeField]
    Units.force _unit;
    public Units.force unit
    {
        get {
            return _unit;
        }
    }

    public Force(double value, Units.force unit) {
        _val = value;
        _unit = unit;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.force thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Force ConvertTo(Units.force outputUnit) {
        return new Force(Units.ConvertForce(unit, outputUnit, val), outputUnit);
    }
};
[Serializable]
public class Force3d : UnitInterface
{
    [SerializeField]
    Vector3d _val;
    public Vector3d val
    {
        get {
            return _val;
        }
    }
    public Vector3 val_Vec3
    {
        get {
            return (Vector3)_val;
        }
    }

    [SerializeField]
    Units.force _unit;
    public Units.force unit
    {
        get {
            return _unit;
        }
    }

    public Force3d(double __fx, double __fy,double __fz, Units.force unit) {
        _val.x = __fx;
        _val.y = __fy;
        _val.z = __fz;
        _unit = unit;
    }

    public Force3d(Vector3d __fVec, Units.force unit) {
        _val = __fVec;
        _unit = unit;
    }

    public override string ToString() {
        string fxStr = MathOps.DoubleToString(val.x);
        string fyStr = MathOps.DoubleToString(val.y);
        string fzStr = MathOps.DoubleToString(val.z);
        return String.Format("[{0}-{1}-{2}] {3}", fxStr, fyStr, fzStr, unit.ToString());
    }
    public string ToString(int significantDigits) {
        string fxStr = MathOps.DoubleToSignificantDigits(val.x, significantDigits);
        string fyStr = MathOps.DoubleToSignificantDigits(val.y, significantDigits);
        string fzStr = MathOps.DoubleToSignificantDigits(val.z, significantDigits);
        return String.Format("[{0}-{1}-{2}] {3}", fxStr, fyStr, fzStr, unit.ToString());
    }
    public bool HasUnit(Units.force thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }
    public Force3d EnsureUnit(Units.force unitToEnsure) {
        if(!HasUnit(unitToEnsure))
            return ConvertTo(unitToEnsure);
        else
            return this;
    }

    public Force3d ConvertTo(Units.force outputUnit) {
        return Units.ConvertForce3d(unit, outputUnit, val);
    }
};

[Serializable]
public class Temperature : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
    }

    [SerializeField]
    Units.temperature _unit;
    public Units.temperature unit
    {
        get {
            return _unit;
        }
    }

    public Temperature(double value, Units.temperature unit) {
        _val = value;
        _unit = unit;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.temperature thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Temperature ConvertTo(Units.temperature outputUnit) {
        return new Temperature(Units.ConvertTemperature(unit, outputUnit, val), outputUnit);
    }
};

[Serializable]
public class Mass : UnitInterface
{
    [SerializeField]
    double _val;
    public double val
    {
        get {
            return _val;
        }
    }

    [SerializeField]
    Units.mass _unit;
    public Units.mass unit
    {
        get {
            return _unit;
        }
    }

    public Mass(double value, Units.mass unit) {
        _val = value;
        _unit = unit;
    }

    public override string ToString() {
        return MathOps.DoubleToString(val) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return MathOps.DoubleToSignificantDigits(val, significantDigits) + " " + unit.ToString();
    }
    public bool HasUnit(Units.mass thoughtUnit) {
        bool outBool = (thoughtUnit.Equals(unit)) ? true : false;
        return outBool;
    }

    public Mass ConvertTo(Units.mass outputUnit) {
        return new Mass(Units.ConvertMass(unit, outputUnit, val), outputUnit);
    }
};

public static class Units
{
    public enum noDimension { NoDim };
    //=======================PRESSURE==============================
    //=============================================================
    public enum pressure { Pa, hPa, MPa, bar, atm };
    private static Dictionary<pressure, double> pressureUnitsCoefs = new Dictionary<pressure, double> {
        { pressure.Pa, 1d },
        { pressure.hPa, 100d },
        { pressure.MPa, 1_000_000d },
        { pressure.bar, 100_000d },
        { pressure.atm, 101_325d },
    };
    public static double ConvertPressure(pressure inputUnit, pressure outputUnit, double valueToConvert)
    {
        return valueToConvert * pressureUnitsCoefs[inputUnit] / pressureUnitsCoefs[outputUnit];
    }
    //=============================================================
    //=======================DISTANCE==============================
    //=============================================================
    public enum distance { cm, m, hm, km, AU };
    public const double KM2M = 1_000d;
    public const double M2KM = 0.001d;
    public const double AU2KM = 149_597_870.0007d;
    public const double KM2AU = 1d/149_597_870.0007d;
    public const double AU2M = 149_597_870000.7d;
    public const double M2AU = 1d/149_597_870000.7d;
    private static Dictionary<distance, double> distanceUnitsCoefs = new Dictionary<distance, double> {
        { distance.cm, 0.1d },
        { distance.m, 1d },
        { distance.hm, 100d },
        { distance.km, 1_000d },
        { distance.AU, 149_597_870_000.7d },
    };
    public static double ConvertDistance(distance inputUnit, distance outputUnit, double valueToConvert)
    {
        return valueToConvert * distanceUnitsCoefs[inputUnit] / distanceUnitsCoefs[outputUnit];
    }
     public static Distance3d ConvertDistance3d(distance inputUnit, distance outputUnit, Vector3d val)
    {
        double ratio = distanceUnitsCoefs[inputUnit] / distanceUnitsCoefs[outputUnit];
        return new Distance3d(val*ratio, outputUnit);
    }
    //=============================================================
    //=======================ANGLE=================================
    //=============================================================
    public enum angle { degree, radian };
    private static Dictionary<angle, double> angleUnitsCoefs = new Dictionary<angle, double> {
        { angle.degree, 1d },
        { angle.radian, Mathd.PI/180d },
    };
    public static double ConvertAngle(angle inputUnit, angle outputUnit, double valueToConvert)
    {
        return valueToConvert * angleUnitsCoefs[inputUnit] / angleUnitsCoefs[outputUnit];
    }
    //=============================================================
    //=======================TIME==================================
    //=============================================================
    public enum time { ms, s, min, h, day };
    private static Dictionary<time, double> timeUnitsCoefs = new Dictionary<time, double> {
        { time.ms, 1d/1_000d },
        { time.s, 1d },
        { time.min, 60d },
        { time.h, 3_600d },
        { time.day, 86_400d }
    };
    public static double ConvertTime(time inputUnit, time outputUnit, double valueToConvert)
    {
        return valueToConvert * timeUnitsCoefs[inputUnit] / timeUnitsCoefs[outputUnit];
    }
    //=============================================================
    //=======================VELOCITY==============================
    //=============================================================
    public enum velocity { ms, kms, kmh, AUh, AUs };
    private static Dictionary<velocity, double> velocityUnitsCoefs = new Dictionary<velocity, double> {
        { velocity.ms, 1d },
        { velocity.kms, 1000d },
        { velocity.kmh, 1000d/3600d },
        { velocity.AUh, 149_597_870_000.7d/3600d },
        { velocity.AUs, 149_597_870_000.7d },
    };
    public static double ConvertVelocity(velocity inputUnit, velocity outputUnit, double valueToConvert)
    {
        return valueToConvert * velocityUnitsCoefs[inputUnit] / velocityUnitsCoefs[outputUnit];
    }
    public static Velocity3d ConvertVelocity3d(velocity inputUnit, velocity outputUnit, Vector3d val)
    {
        double ratio = velocityUnitsCoefs[inputUnit] / velocityUnitsCoefs[outputUnit];
        return new Velocity3d(val*ratio, outputUnit);
    }
    //=============================================================
    //=======================ACCELERATION==========================
    //=============================================================
    public enum acceleration { ms2, kms2, kmh2 };
    private static Dictionary<acceleration, double> accelerationUnitsCoefs = new Dictionary<acceleration, double> {
        { acceleration.ms2, 1d },
        { acceleration.kms2, 1000d },
        { acceleration.kmh2, 1000d/(3600d*3600d) }
    };
    public static double ConvertAcceleration(acceleration inputUnit, acceleration outputUnit, double valueToConvert)
    {
        return valueToConvert * accelerationUnitsCoefs[inputUnit] / accelerationUnitsCoefs[outputUnit];
    }
    public static Acceleration3d ConvertAcceleration3d(acceleration inputUnit, acceleration outputUnit, Vector3d val)
    {
        double ratio = accelerationUnitsCoefs[inputUnit] / accelerationUnitsCoefs[outputUnit];
        return new Acceleration3d(val*ratio, outputUnit);
    }
    //=============================================================
    //=======================FORCE=================================
    //=============================================================
    public enum force { Pam2  };
    private static Dictionary<force, double> forceUnitsCoefs = new Dictionary<force, double> {
        { force.Pam2, 1d },
    };
    public static double ConvertForce(force inputUnit, force outputUnit, double valueToConvert)
    {
        return valueToConvert * forceUnitsCoefs[inputUnit] / forceUnitsCoefs[outputUnit];
    }
    public static Force3d ConvertForce3d(force inputUnit, force outputUnit, Vector3d val)
    {
        double ratio = forceUnitsCoefs[inputUnit] / forceUnitsCoefs[outputUnit];
        return new Force3d(val*ratio, outputUnit);
    }
    //=============================================================
    //=======================TEMPERATURE===========================
    //=============================================================
    public enum temperature { degreeC, degreeK };
    public static double ConvertTemperature(temperature inputUnit, temperature outputUnit, double valueToConvert)
    {
        if(inputUnit == outputUnit) { return valueToConvert; }
        else if(inputUnit == temperature.degreeK && outputUnit == temperature.degreeC) {
            return valueToConvert - 273.15d;
        }
        else {
            return valueToConvert + 273.15d;
        }
    }
    //=============================================================
    //=======================MASS==================================
    //=============================================================
    public enum mass { kg, g, T, solarMass, earthMass, pound, once };
    private static Dictionary<mass, double> massUnitsCoefs = new Dictionary<mass, double> {
        { mass.kg, 1d },
        { mass.g, 0.001d },
        { mass.T, 1_000d },
        //{ mass.solarMass, UniCsts.planetsDict[UniCsts.planets.Sun]["massEarthRatio"].value*UniCsts.earthMass*UniCsts.earthMassExponent },
        //{ mass.earthMass, UniCsts.earthMass*UniCsts.earthMassExponent },
        { mass.pound, 0.45359237d },
        { mass.once, 0.02834952313d }
    };
    public static double ConvertMass(mass inputUnit, mass outputUnit, double valueToConvert)
    {
        return valueToConvert * massUnitsCoefs[inputUnit] / massUnitsCoefs[outputUnit];
    }
}