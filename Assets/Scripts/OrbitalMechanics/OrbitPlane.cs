using System;
using Mathd_Lib;

[Serializable]
public class OrbitPlane
{
    public Vector3d forwardVec;
    public Vector3d rightVec;
    public Vector3d point;
    public Vector3d normal;

    public OrbitPlane(Vector3d fVec, Vector3d rVec, Vector3d p)
    {
        forwardVec = fVec;
        rightVec = rVec;
        point = p;
        NormalVec();
    }

    public OrbitPlane(double initVal)
    {
        forwardVec = rightVec = point = normal = new Vector3d(initVal, initVal, initVal);
    }

    public OrbitPlane()
    {
        forwardVec = rightVec = point = Vector3d.NaN();
    }

    public void AssignOrbitPlane(Vector3d fVec, Vector3d rVec, Vector3d p)
    {
        forwardVec = fVec;
        rightVec = rVec;
        point = p;
        NormalVec();
    }

    public void NormalVec()
    {
        normal = Vector3d.Cross(forwardVec, rightVec);
    }

    public static void PlaneIntersectPlane(OrbitPlane p1, OrbitPlane p2, out Vector3d outLinePoint, out Vector3d outLineVec)
    {
        // Return the intersection of two planes as a line, as a vector of direction of the line, and a point on this line
        outLineVec = Vector3d.Cross(p1.normal, p2.normal).normalized;
        if(!Vector3d.IsValidAndNoneZero(outLineVec)) {
            outLineVec = Vector3d.NaN();
        }

        Vector3d ldir = Vector3d.Cross(p2.normal, outLineVec);
        double numerator = Vector3d.Dot(p1.normal, ldir);
    
        if(Mathd.Abs(numerator) > 0.000001f){
        
            Vector3d plane1ToPlane2 = p1.point - p2.point;
            double t = Vector3d.Dot(p1.normal, plane1ToPlane2) / numerator;
            outLinePoint = p2.point + t * ldir;
        }
        else{
            outLinePoint = Vector3d.zero;
        }
    }
}