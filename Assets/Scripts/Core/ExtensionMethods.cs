using UnityEngine;
using System.Collections;
using Mathd_Lib;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods
{
    //Even though they are used like normal methods, extension
    //methods must be declared static. Notice that the first
    //parameter has the 'this' keyword followed by a Transform
    //variable. This variable denotes which class the extension
    //method becomes a part of.
    //------------------------
    //------------------------
    /// <summary>
    /// Convert the float from the Sim scaling system to the Universe scaling system
    /// </summary>
    public static float ToUni(this float val)
    {
        return val*UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the double from the Sim scaling system to the Universe scaling system
    /// </summary>
    public static double ToUni(this double val)
    {
        return val*UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the int from the Sim scaling system to the Universe scaling system
    /// </summary>
    public static int ToUni(this int val)
    {
        return val*UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector2 from the Sim scaling system to the Universe scaling system
    /// </summary>
    public static Vector2 ToUni(this Vector2 val)
    {
        return val*UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector2d from the Sim scaling system to the Universe scaling system
    /// </summary>
    public static Vector2d ToUni(this Vector2d val)
    {
        return val*UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector3 from the Sim scaling system to the Universe scaling system
    /// </summary>
    public static Vector3 ToUni(this Vector3 val)
    {
        return val*UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector3d from the Sim scaling system to the Universe scaling system
    /// </summary>
    public static Vector3d ToUni(this Vector3d val)
    {
        return val*UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector4 from the Sim scaling system to the Universe scaling system
    /// </summary>
    public static Vector4 ToUni(this Vector4 val)
    {
        return val*UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector4d from the Sim scaling system to the Universe scaling system
    /// </summary>
    public static Vector4d ToUni(this Vector4d val)
    {
        return val*UniverseConstants.sim2uni;
    }
    
    public static Velocity ToUni(this Velocity val)
    {
        return new Velocity(val.val*UniverseConstants.sim2uni, val.unit);
    }
    //------------------------
    //------------------------
    /// <summary>
    /// Convert the float from the Universe scaling system to the Sim scaling system 
    /// </summary>
    public static float ToSim(this float val)
    {
        return val/UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the double from the Universe scaling system to the Sim scaling system
    /// </summary>
    public static double ToSim(this double val)
    {
        return val/UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the int from the Universe scaling system to the Sim scaling system
    /// </summary>
    public static int ToSim(this int val)
    {
        return val/UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector2 from the Universe scaling system to the Sim scaling system
    /// </summary>
    public static Vector2 ToSim(this Vector2 val)
    {
        return val/UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector2d from the Universe scaling system to the Sim scaling system
    /// </summary>
    public static Vector2d ToSim(this Vector2d val)
    {
        return val/UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector3 from the Universe scaling system to the Sim scaling system
    /// </summary>
    public static Vector3 ToSim(this Vector3 val)
    {
        return val/UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector3d from the Universe scaling system to the Sim scaling system
    /// </summary>
    public static Vector3d ToSim(this Vector3d val)
    {
        return val/UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector4 from the Universe scaling system to the Sim scaling system
    /// </summary>
    public static Vector4 ToSim(this Vector4 val)
    {
        return val/UniverseConstants.sim2uni;
    }
    /// <summary>
    /// Convert the Vector4d from the Universe scaling system to the Sim scaling system
    /// </summary>
    public static Vector4d ToSim(this Vector4d val)
    {
        return val/UniverseConstants.sim2uni;
    }
    
    public static Velocity ToSim(this Velocity val)
    {
        return new Velocity(val.val/UniverseConstants.sim2uni, val.unit);
    }

}