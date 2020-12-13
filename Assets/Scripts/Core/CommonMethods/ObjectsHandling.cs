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

    }
}