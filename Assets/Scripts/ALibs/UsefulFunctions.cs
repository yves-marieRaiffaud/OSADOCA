using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;

public static class UsefulFunctions
{
    public static float mapInRange(Vector2 inputRange, Vector2 outputRange, float valueToMap)
    {
        float a1 = inputRange.x;
        float a2 = inputRange.y;
        float b1 = outputRange.x;
        float b2 = outputRange.y;

        float remapped_val = b1 + (valueToMap - a1)*(b2-b1)/(a2-a1);
        return remapped_val;
    }

    public enum isInRangeFlags { both_included, both_excluded, lowerIncluded_UpperExcluded, lowerExcluded_UpperIncluded };
    public static bool isInRange(float val, float lowerBound, float upperBound, isInRangeFlags flag=isInRangeFlags.both_included)
    {
        if(flag == isInRangeFlags.both_included)
        {
            if(val <= upperBound && val >= lowerBound) { return true; }
            else{ return false; }
        }

        else if(flag == isInRangeFlags.both_excluded)
        {
            if(val < upperBound && val > lowerBound) { return true; }
            else{ return false; }
        }

        else if(flag == isInRangeFlags.lowerExcluded_UpperIncluded)
        {
            if(val <= upperBound && val > lowerBound) { return true; }
            else{ return false; }
        }

        else if(flag == isInRangeFlags.lowerIncluded_UpperExcluded)
        {
            if(val < upperBound && val >= lowerBound) { return true; }
            else{ return false; }
        }
        return false;
    }

    public static bool isInRange(double val, double lowerBound, double upperBound, isInRangeFlags flag=isInRangeFlags.both_included)
    {
        if(flag == isInRangeFlags.both_included)
        {
            if(val <= upperBound && val >= lowerBound) { return true; }
            else{ return false; }
        }

        else if(flag == isInRangeFlags.both_excluded)
        {
            if(val < upperBound && val > lowerBound) { return true; }
            else{ return false; }
        }

        else if(flag == isInRangeFlags.lowerExcluded_UpperIncluded)
        {
            if(val <= upperBound && val > lowerBound) { return true; }
            else{ return false; }
        }

        else if(flag == isInRangeFlags.lowerIncluded_UpperExcluded)
        {
            if(val < upperBound && val >= lowerBound) { return true; }
            else{ return false; }
        }
        return false;
    }


    public static float linearInterpolation(float x1, float y1, float x3, float y3, float xValToGuessY)
    {
        return (y3-y1)/(x3-x1) * (xValToGuessY-x1) + y1;
    }

    public static bool FloatsAreEqual(float a, float b, float tolerance=0.00001f)
    {
        if(Mathf.Abs(a - b) <= tolerance) { return true; }
        else { return false; }
    }

    public static bool DoublesAreEqual(double a, double b, double tolerance=Mathd.Epsilon)
    {
        if(Mathd.Abs(a - b) <= tolerance) { return true; }
        else { return false; }
    }

    public static float UnwrapAngle(float angle)
    {
        if(angle < 180f)
            return angle;
        return angle-360;
    }

    public static UniverseRunner.goTags CastStringToGoTags(string goTag)
    {
        if(goTag.Equals(UniverseRunner.goTags.Planet.ToString()))
        {
            return UniverseRunner.goTags.Planet;
        }
        else if(goTag.Equals(UniverseRunner.goTags.Spaceship.ToString()))
        {
            return UniverseRunner.goTags.Spaceship;
        }
        else {
            return UniverseRunner.goTags.Star;
        }
    }

    public static bool GoTagAndStringAreEqual(UniverseRunner.goTags tags, string stringToCompare)
    {
        if(tags.ToString().Equals(stringToCompare))
        {
            return true;
        }
        else { return false; }
    }

    public static bool StringIsOneOfTheTwoTags(UniverseRunner.goTags tag1, UniverseRunner.goTags tag2, string stringToCompare)
    {
        if(GoTagAndStringAreEqual(tag1, stringToCompare) || GoTagAndStringAreEqual(tag2, stringToCompare))
        {
            return true;
        }
        else { return false; }
    }

    public static GameObject CreateAssignGameObject(string gameObjectName)
    {
        if(GameObject.Find(gameObjectName) == null)
        {
            return new GameObject(gameObjectName);
        }
        else{
            return GameObject.Find(gameObjectName);
        }
    }


    public static GameObject CreateAssignGameObject(string gameObjectName, GameObject parent)
    {
        GameObject retunedGO = UsefulFunctions.CreateAssignGameObject(gameObjectName);
        if(retunedGO.transform.parent != parent.transform)
        {
            retunedGO.transform.parent = parent.transform;
        }
        return retunedGO;
    }

    public static GameObject CreateAssignGameObject(string gameObjectName, System.Type componentType)
    {
        if(GameObject.Find(gameObjectName) == null)
        {
            return new GameObject(gameObjectName, componentType);
        }
        else{
            return GameObject.Find(gameObjectName);
        }
    }

    public static GameObject CreateAssignGameObject(string gameObjectName, PrimitiveType primitiveType)
    {
        if(GameObject.Find(gameObjectName) == null)
        {
            GameObject go = GameObject.CreatePrimitive(primitiveType);
            go.name = gameObjectName;
            return go;
        }
        else{
            return GameObject.Find(gameObjectName);
        }
    }

    public static GameObject CreateAssignGameObject(string gameObjectName, PrimitiveType primitiveType, Vector3 position)
    {
        if(GameObject.Find(gameObjectName) == null)
        {
            GameObject go = GameObject.CreatePrimitive(primitiveType);
            go.name = gameObjectName;
            go.transform.position = position;
            return go;
        }
        else{
            return GameObject.Find(gameObjectName);
        }
    }

    public static GameObject CreateAssignGameObject(string gameObjectName, PrimitiveType primitiveType, Vector3 position, GameObject alignToParent=null, bool addToUniversePhysicsObj=false)
    {
        GameObject go;
        if(GameObject.Find(gameObjectName) == null)
        {
            go = GameObject.CreatePrimitive(primitiveType);
            go.name = gameObjectName;
            go.transform.position = position;
        }
        else{
            go = GameObject.Find(gameObjectName);
            go.transform.position = position;
        }

        if(alignToParent != null)
        {
            UsefulFunctions.AlignGameObjectWithParentPos(go, alignToParent);
        }

        if(addToUniversePhysicsObj)
        {
            UniverseRunner universe = GameObject.Find("UniverseRunner").GetComponent<UniverseRunner>();
            universe.physicsObjArray.Add(go.transform);
        }
        return go;
    }


    public static Component CreateAssignComponent(System.Type componentType, GameObject goToCheck)
    {
        if(goToCheck.GetComponent(componentType) == null)
        {
            goToCheck.AddComponent(componentType);
        }
        return goToCheck.GetComponent(componentType);
    }


    public static Vector3d AlignPositionVecWithParentPos(Vector3d positionVecToAlign, Vector3 parentPos)
    {
        return positionVecToAlign + new Vector3d(parentPos);
    }
    public static Vector3d AlignPositionVecWithParentPos(Vector3d positionVecToAlign, Vector3d parentPos)
    {
        return positionVecToAlign + parentPos;
    }

    public static void AlignGameObjectWithParentPos(GameObject gameObjectToAlign, GameObject parentRef)
    {
        gameObjectToAlign.transform.position += parentRef.transform.position;
    }
    public static void AlignGameObjectWithParentPos(string nameOfGameObjectToAlign, GameObject parentRef)
    {
        GameObject goToAlign = GameObject.Find(nameOfGameObjectToAlign);
        goToAlign.transform.position += parentRef.transform.position;
    }

    public static void parentObj(GameObject obj, GameObject target)
    {
        obj.transform.parent = target.transform;
    }

    public static bool TransformBodyHasTagName(Transform body, string tagName)
    {
        if(body.tag == tagName) { return true; }
        else{ return false; }
    }

    public static void DrawVector(Vector3d vectorDir, Vector3d startPoint, float vectorLength=1000f, string nameGameObject="NewVector", float lineWidth=50f)
    {
        if(!Vector3d.IsValid(vectorDir) || !Vector3d.IsValid(startPoint)) {
            return;
        }
        else {
            // Check if GameObject already Exists
            GameObject dirGO = UsefulFunctions.CreateAssignGameObject(nameGameObject);
            dirGO.layer = 10; // 'Orbit' layer
            LineRenderer dirLR = (LineRenderer) UsefulFunctions.CreateAssignComponent(typeof(LineRenderer), dirGO);

            Vector3[] pos = new Vector3[2];
            pos[0] = (Vector3) startPoint;
            pos[1] = (Vector3)startPoint + (Vector3) (vectorLength*vectorDir);

            dirLR.useWorldSpace = false;
            dirLR.positionCount = pos.Length;
            dirLR.SetPositions(pos);
            dirLR.widthCurve = AnimationCurve.Constant(0f, 1f, lineWidth);
            dirLR.sharedMaterial = Resources.Load("OrbitMaterial", typeof(Material)) as Material;
        }
    }

    public static Texture2D RotateTexture180(Texture2D originalTexture)
    {
        Color32[] textureColors = originalTexture.GetPixels32();

        System.Array.Reverse(textureColors, 0, textureColors.Length);

        Texture2D rotatedTexture = new Texture2D(originalTexture.width, originalTexture.height);
        rotatedTexture.SetPixels32(textureColors);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    public static void DebugCreateTexturedGameObject(string goName, Texture2D texture, Vector3 goScale)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = goName;
        plane.transform.localScale = goScale;
        MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Unlit/Transparent"));
        renderer.material = material;
        renderer.material.mainTexture = texture;
    }

}
