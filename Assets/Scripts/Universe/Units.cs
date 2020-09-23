using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mathd_Lib;
using TFunc = System.Func<dynamic, dynamic, dynamic>;

public static class PlanetsFunctions
{
    public enum chosenFunction { pressureEvolution };
    public static Dictionary<chosenFunction, TFunc> earthFcnsDict = new Dictionary<chosenFunction, TFunc>() {
        { chosenFunction.pressureEvolution, (surfPressure, inputAlt) => {return EarthPressureEvolution(surfPressure, inputAlt);} }
    };
    //======
    //======
    public static Pressure EarthPressureEvolution(Pressure surfacePressure, double inputAltitude) {
        // inputAltitude is the altitude in km from the surface of the planet
        double pressureAtAlt = surfacePressure.value / (Mathd.Pow(2, (inputAltitude/5d)));
        Pressure newPressure = new Pressure(pressureAtAlt, surfacePressure.unit);
        return newPressure;
    }
}
//==================
//==================
//==================
public interface UnitInterface
{
    double value {get; set;}
}
public class UnitsClass<T> : UnitInterface
{
    public double value {get; set;}
    public T unit {get; set;}

    public UnitsClass(double _value, T _unit) {
        value = _value;
        unit = _unit;
    }
    public override string ToString() {
        return UsefulFunctions.DoubleToString(value) + " " + unit.ToString();
    }
    public string ToString(int significantDigits) {
        return UsefulFunctions.DoubleToSignificantDigits(value, significantDigits) + " " + unit.ToString();
    }
}

public class DoubleNoDim : UnitsClass<Units.noDimension>
{
    public DoubleNoDim(double _value) : base(_value, Units.noDimension.NoDim) { }
};

public class Pressure : UnitsClass<Units.pressure>
{
    public Pressure(double _value, Units.pressure _unit) : base(_value, _unit) { }
    public Pressure ConvertTo(Units.pressure outputUnit) {
        return new Pressure(Units.ConvertPressure(unit, outputUnit, value), outputUnit);
    }
};

public class Distance : UnitsClass<Units.distance>
{
    public Distance(double _value, Units.distance _unit) : base(_value, _unit) { }
    public Distance ConvertTo(Units.distance outputUnit) {
        return new Distance(Units.ConvertDistance(unit, outputUnit, value), outputUnit);
    }
};

public class Angle : UnitsClass<Units.angle>
{
    public Angle(double _value, Units.angle _unit) : base(_value, _unit) { }
    public Angle ConvertTo(Units.angle outputUnit) {
        return new Angle(Units.ConvertAngle(unit, outputUnit, value), outputUnit);
    }
};

public class Time_Class : UnitsClass<Units.time>
{
    public Time_Class(double _value, Units.time _unit) : base(_value, _unit) { }
    public Time_Class ConvertTo(Units.time outputUnit) {
        return new Time_Class(Units.ConvertTime(unit, outputUnit, value), outputUnit);
    }
};

public class Velocity : UnitsClass<Units.velocity>
{
    public Velocity(double _value, Units.velocity _unit) : base(_value, _unit) { }
    public Velocity ConvertTo(Units.velocity outputUnit) {
        return new Velocity(Units.ConvertVelocity(unit, outputUnit, value), outputUnit);
    }
};

public class Acceleration : UnitsClass<Units.acceleration>
{
    public Acceleration(double _value, Units.acceleration _unit) : base(_value, _unit) { }
    public Acceleration ConvertTo(Units.acceleration outputUnit) {
        return new Acceleration(Units.ConvertAcceleration(unit, outputUnit, value), outputUnit);
    }
};

public class Force : UnitsClass<Units.force>
{
    public Force(double _value, Units.force _unit) : base(_value, _unit) { }
    public Force ConvertTo(Units.force outputUnit) {
        return new Force(Units.ConvertForce(unit, outputUnit, value), outputUnit);
    }
};

public class Temperature : UnitsClass<Units.temperature>
{
    public Temperature(double _value, Units.temperature _unit) : base(_value, _unit) { }
    public Temperature ConvertTo(Units.temperature outputUnit) {
        return new Temperature(Units.ConvertTemperature(unit, outputUnit, value), outputUnit);
    }
};

public class Mass : UnitsClass<Units.mass>
{
    public Mass(double _value, Units.mass _unit) : base(_value, _unit) { }
    public Mass ConvertTo(Units.mass outputUnit) {
        return new Mass(Units.ConvertMass(unit, outputUnit, value), outputUnit);
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
    public enum velocity { m_s, km_s, km_h, AU_h, AU_s };
    private static Dictionary<velocity, double> velocityUnitsCoefs = new Dictionary<velocity, double> {
        { velocity.m_s, 1d },
        { velocity.km_s, 1000d },
        { velocity.km_h, 1000d/3600d },
        { velocity.AU_h, 149_597_870_000.7d/3600d },
        { velocity.AU_s, 149_597_870_000.7d },
    };
    public static double ConvertVelocity(velocity inputUnit, velocity outputUnit, double valueToConvert)
    {
        return valueToConvert * velocityUnitsCoefs[inputUnit] / velocityUnitsCoefs[outputUnit];
    }
    //=============================================================
    //=======================ACCELERATION==========================
    //=============================================================
    public enum acceleration { m_s2, km_s2, km_h2 };
    private static Dictionary<acceleration, double> accelerationUnitsCoefs = new Dictionary<acceleration, double> {
        { acceleration.m_s2, 1d },
        { acceleration.km_s2, 1000d },
        { acceleration.km_h2, 1000d/(3600d*3600d) }
    };
    public static double ConvertAcceleration(acceleration inputUnit, acceleration outputUnit, double valueToConvert)
    {
        return valueToConvert * accelerationUnitsCoefs[inputUnit] / accelerationUnitsCoefs[outputUnit];
    }
    //=============================================================
    //=======================FORCE=================================
    //=============================================================
    public enum force { Pa_m2  };
    private static Dictionary<force, double> forceUnitsCoefs = new Dictionary<force, double> {
        { force.Pa_m2, 1d },
    };
    public static double ConvertForce(force inputUnit, force outputUnit, double valueToConvert)
    {
        return valueToConvert * forceUnitsCoefs[inputUnit] / forceUnitsCoefs[outputUnit];
    }
    //=============================================================
    //=======================TEMPERATURE===========================
    //=============================================================
    public enum temperature { degree_C, degree_K };
    public static double ConvertTemperature(temperature inputUnit, temperature outputUnit, double valueToConvert)
    {
        if(inputUnit == outputUnit) { return valueToConvert; }
        else if(inputUnit == temperature.degree_K && outputUnit == temperature.degree_C) {
            return valueToConvert - 273.15d;
        }
        else {
            return valueToConvert + 273.15d;
        }
    }


    public enum mass { kg, g, T, solarMass, earthMass, pound, once };
    private static Dictionary<mass, double> massUnitsCoefs = new Dictionary<mass, double> {
        { mass.kg, 1d },
        { mass.g, 0.001d },
        { mass.T, 1_000d },
        { mass.solarMass, UniCsts.planetsDict[UniCsts.planets.Sun]["massEarthRatio"].value*UniCsts.earthMass*UniCsts.earthMassExponent },
        { mass.earthMass, UniCsts.earthMass*UniCsts.earthMassExponent },
        { mass.pound, 0.45359237d },
        { mass.once, 0.02834952313d }
    };
    public static double ConvertMass(mass inputUnit, mass outputUnit, double valueToConvert)
    {
        return valueToConvert * massUnitsCoefs[inputUnit] / massUnitsCoefs[outputUnit];
    }
}