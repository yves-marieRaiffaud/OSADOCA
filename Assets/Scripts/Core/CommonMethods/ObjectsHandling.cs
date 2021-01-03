using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using Mathd_Lib;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Universe;

namespace CommonMethods
{
    public static class ObjectsHandling
    {
        public static GameObject CreateAssignGameObject(string gameObjectName)
        {
            if(GameObject.Find(gameObjectName) == null)
                return new GameObject(gameObjectName);
            else
                return GameObject.Find(gameObjectName);
        }
        public static GameObject CreateAssignGameObject(string gameObjectName, GameObject parent)
        {
            GameObject retunedGO = CommonMethods.ObjectsHandling.CreateAssignGameObject(gameObjectName);
            if(parent != null && retunedGO.transform.parent != parent.transform)
                retunedGO.transform.parent = parent.transform;
            return retunedGO;
        }
        public static GameObject CreateAssignGameObject(string gameObjectName, System.Type componentType)
    {
        if(GameObject.Find(gameObjectName) == null)
            return new GameObject(gameObjectName, componentType);
        else
            return GameObject.Find(gameObjectName);
    }

        public static Component CreateAssignComponent(System.Type componentType, GameObject goToCheck)
        {
            if(goToCheck.GetComponent(componentType) == null)
                goToCheck.AddComponent(componentType);
            return goToCheck.GetComponent(componentType);
        }

        public static void ParentObj(GameObject obj, GameObject target)
        {
            obj.transform.parent = target.transform;
        }

        public static UniverseRunner.goTags Str_2_GoTags(string goTag)
        {
            if(goTag.Equals(UniverseRunner.goTags.Planet.ToString()))
                return UniverseRunner.goTags.Planet;
            else if(goTag.Equals(UniverseRunner.goTags.Spaceship.ToString()))
                return UniverseRunner.goTags.Spaceship;
            else
                return UniverseRunner.goTags.Star;
        }
        public static bool GoTagAndStringAreEqual(UniverseRunner.goTags tags, string stringToCompare)
        {
            if(tags.ToString().Equals(stringToCompare))
                return true;
            else
                return false;
        }
        public static bool StringIsOneOfTheTwoTags(UniverseRunner.goTags tag1, UniverseRunner.goTags tag2, string stringToCompare)
        {
            if(GoTagAndStringAreEqual(tag1, stringToCompare) || GoTagAndStringAreEqual(tag2, stringToCompare))
                return true;
            else
                return false;
        }

        public static CelestialBodiesConstants.planets Str_2_Planet(string goTag)
        {
            (CelestialBodiesConstants.planets,bool) res = Generic_Str_2_Enum<CelestialBodiesConstants.planets>(goTag);
            if(res.Item2)
                return res.Item1; // If we found the Enum object mathcing 'stringEnum'
            else
                return CelestialBodiesConstants.planets.Earth; // Default value to return
        }

        internal static (T,bool) Generic_Str_2_Enum<T>(string enumString)
        where T : Enum
        {
            // return the correpsonding or the default Enum object, and the bool indicating if an exact match has been found, or if the default value is returned
            string[] stringArr = Enum.GetNames(typeof(T));
            for(int idx=0; idx<stringArr.Length; idx++) {
                if(enumString.Equals(stringArr[idx]))
                    return ((T)Enum.ToObject(typeof(T), idx), true); // 'true' to indicate that the enum object matching the string 'enumString' has been found
            }
            return ((T)Enum.ToObject(typeof(T), 0), false); // 'false' to indicate that the default value is returned
        }
        internal static (T,bool) Generic_Str_2_FlagedEnum<T>(string enumString)
        {
            // return the correpsonding or the default Enum object, and the bool indicating if an exact match has been found, or if the default value is returned
            string[] stringArr = Enum.GetNames(typeof(T));
            int stringIdx = 0;
            if(enumString.Equals(stringArr[stringIdx]))
                return ((T)Enum.ToObject(typeof(T), 0), true);

            stringIdx=1;
            for(int idx=1; idx< Math.Pow(2,stringArr.Length); idx*=2) {
                if(enumString.Equals(stringArr[stringIdx]))
                    return ((T)Enum.ToObject(typeof(T), idx), true); // 'true' to indicate that the enum object matching the string 'enumString' has been found
                stringIdx += 1;
            }
            return ((T)Enum.ToObject(typeof(T), 0), false); // 'false' to indicate that the default value is returned
        }
        public static OrbitalTypes.orbitDefinitionType Str_2_OrbitDefinitionType(string stringEnum)
        {
            (OrbitalTypes.orbitDefinitionType,bool) res = Generic_Str_2_Enum<OrbitalTypes.orbitDefinitionType>(stringEnum);
            if(res.Item2)
                return res.Item1; // If we found the Enum object mathcing 'stringEnum'
            else
                return OrbitalTypes.orbitDefinitionType.rarp; // Default value to return
        }
        public static OrbitalTypes.orbitalParamsUnits Str_2_OrbitalParamsUnit(string stringEnum)
        {
            (OrbitalTypes.orbitalParamsUnits,bool) res = Generic_Str_2_Enum<OrbitalTypes.orbitalParamsUnits>(stringEnum);
            if(res.Item2)
                return res.Item1; // If we found the Enum object mathcing 'stringEnum'
            else
                return OrbitalTypes.orbitalParamsUnits.km; // Default value to return
            
        }
        public static OrbitalTypes.bodyPositionType Str_2_BodyPosType(string stringEnum)
        {
            (OrbitalTypes.bodyPositionType,bool) res = Generic_Str_2_Enum<OrbitalTypes.bodyPositionType>(stringEnum);
            if(res.Item2)
                return res.Item1; // If we found the Enum object mathcing 'stringEnum'
            else
                return OrbitalTypes.bodyPositionType.nu; // Default value to return
        }

        public static void Check_Create_Directory(string filepath, bool printToDebugLog)
        {
            if(!Directory.Exists(filepath)) {
                Directory.CreateDirectory(filepath);
                if(printToDebugLog)
                    Debug.LogFormat("Created directory at path: '{0}'", filepath);
            }
        }

    }
}