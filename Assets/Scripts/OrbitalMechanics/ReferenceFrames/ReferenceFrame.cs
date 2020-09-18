using UnityEngine;
using Mathd_Lib;
using System;

public class ReferenceFrame
{
    public enum SpecifiedVectors { XY, XZ, YZ };

    Vector3d _origin;
    public Vector3d origin
    {
        get {
            return _origin;
        }
    }

    Vector3d _xVec;
    public Vector3d xVec
    {
        get {
            return _xVec;
        }
    }

    Vector3d _yVec;
    public Vector3d yVec
    {
        get {
            return _yVec;
        }
    }

    Vector3d _zVec;
    public Vector3d zVec
    {
        get {
            return _zVec;
        }
    }

    SpecifiedVectors _specifiedVectors;
    public SpecifiedVectors specifiedVectors
    {
        get {
            return _specifiedVectors;
        }
    }

    GameObject _attachedBody;
    public GameObject attachedBody
    {
        get {
            return _attachedBody;
        }
    }

    string _name;
    public string name
    {
        get {
            return _name;
        }
    }

    /// <summary>
    /// If 'attachedToBody' is NOT specified, the 'originPoint', 'vec1' and 'vec2' vectors must be specified in the UNITY WORLD SPACE (which is inertial)
    /// </summary>
    ReferenceFrame(Vector3d originPoint, Vector3d vec1, Vector3d vec2, SpecifiedVectors _inputVectors, string _frameName)
    {
        _origin = originPoint;
        vec1 = vec1.normalized;
        vec2 = vec2.normalized;

        _name = _frameName;

        switch(_inputVectors)
        {
            case SpecifiedVectors.XY:
                _xVec = vec1;
                _yVec = vec2;
                break;
            case SpecifiedVectors.XZ:
                _xVec = vec1;
                _zVec = vec2;
                break;
            case SpecifiedVectors.YZ:
                _yVec = vec1;
                _zVec = vec2;
                break;
        }
        ComputeThirdAxis();
    }
    
    void ComputeThirdAxis()
    {
        // Compute the third axis to have a complete direct frame
        switch(_specifiedVectors)
        {
            case SpecifiedVectors.XY:
                _zVec = Vector3d.Cross(_xVec, _yVec).normalized;
                break;
            case SpecifiedVectors.XZ:
                _yVec = Vector3d.Cross(_zVec, _xVec).normalized;
                break;
            case SpecifiedVectors.YZ:
                _xVec = Vector3d.Cross(_yVec, _zVec).normalized;
                break;
        }
    }

    /// <summary>
    /// If 'attachedToBody' is NOT specified, the 'originPoint', 'vec1' and 'vec2' vectors must be specified in the UNITY WORLD SPACE (which is inertial)
    /// </summary>
    public static ReferenceFrame New_ReferenceFrame(Vector3 originPoint, Vector3 vec1, Vector3 vec2, SpecifiedVectors _inputVectors, string _frameName)
    {
        ReferenceFrame frame = new ReferenceFrame(new Vector3d(originPoint), new Vector3d(vec1), new Vector3d(vec2), _inputVectors, _frameName);
        return frame;
    }

    /// <summary>
    /// If 'attachedToBody' is specified with a non-null GameObject, the 'originPoint', 'vec1' and 'vec2' vectors must be specified in the 'attachedToBody' LOCAL SPACE
    /// </summary>
    public static ReferenceFrame New_ReferenceFrame(Vector3 originPoint, Vector3 vec1, Vector3 vec2, SpecifiedVectors _inputVectors, GameObject attachedToBody, string _frameName)
    {
        ReferenceFrame frame = new ReferenceFrame(new Vector3d(originPoint), new Vector3d(vec1), new Vector3d(vec2), _inputVectors, _frameName);

        if(attachedToBody != null) {
            frame._attachedBody = attachedToBody;
            GameObject frameTemplate = GameObject.Find("FrameTemplate");
            GameObject createdFrame = (GameObject) GameObject.Instantiate(frameTemplate, frame._attachedBody.transform.position, frame._attachedBody.transform.rotation, frame._attachedBody.transform);
            createdFrame.gameObject.name = _frameName;
        }
        return frame;
    }

}