using UnityEngine;
using Mathd_Lib;
using System;

public class ReferenceFramesList : MonoBehaviour
{
    ReferenceFrame _ECEF;
    public ReferenceFrame ECEF
    {
        get {
            return _ECEF;
        }
    }

    ReferenceFrame _ECI;
    public ReferenceFrame ECI
    {
        get {
            return _ECI;
        }
    }

    ReferenceFrame _HECS;
    public ReferenceFrame HECS
    {
        get {
            return _HECS;
        }
    }
    Quaternion hecsInitRot;

    Matrix4x4 _ECI_2_HECS;
    public Matrix4x4 ECI_2_HECS
    {
        get {
            return _ECI_2_HECS;
        }
    }
    public Matrix4x4 HECS_2_ECI
    {
        get {
            if(ECI_2_HECS != null)
                return _ECI_2_HECS.inverse;
            else
                return new Matrix4x4(Vector4.zero,Vector4.zero,Vector4.zero,Vector4.zero);
        }
    }


    public ReferenceFramesList()
    {
        GameObject earthGO = GameObject.Find("Earth");
        // 'Earth-Centered-Earth-Fixed' frame == repère terrestre vrai : https://en.wikipedia.org/wiki/ECEF
        _ECEF = ReferenceFrame.New_ReferenceFrame(Vector3.zero, Vector3.right, Vector3.up, ReferenceFrame.SpecifiedVectors.XZ, earthGO, "ECEF");

        // 'Eart-centrered Inertial'/'Conventional Celestial Reference System' frame == repère celeste moyen : https://gssc.esa.int/navipedia/index.php/Conventional_Celestial_Reference_System
        Vector3 vernalDir = (Vector3) ((UniCsts.pv_j2000 - earthGO.transform.position).normalized);
        _ECI = ReferenceFrame.New_ReferenceFrame(Vector3.zero, vernalDir, Vector3.up, ReferenceFrame.SpecifiedVectors.XZ, earthGO, "ECI");

        GameObject sunGO = GameObject.Find("Sun");
        // Heliocentric Coordinates System == repère héliocentrique
        _HECS = ReferenceFrame.New_ReferenceFrame(Vector3.zero, Vector3.right, Vector3.up, ReferenceFrame.SpecifiedVectors.XZ, sunGO, "HECS");

        ECEF.PrintFrame(true);
        ECI.PrintFrame(true);
        HECS.PrintFrame(true);

        Init_TransformationMatrices();
        SaveInertialParentedFramesRot();
    }

    void SaveInertialParentedFramesRot()
    {
        hecsInitRot = HECS.frameGO.transform.rotation;
    }

    void LateUpdate()
    {
        // Making sure that every frame that are parented to a celestialBody/spaceship AND that should not rotate with it keep their initial rotation
        // Making sure these frames keep their inertial property

        // HECS is parented to the Sun but should not rotate with it (but should move with it)
        HECS.frameGO.transform.rotation = hecsInitRot;
    }

    void Init_TransformationMatrices()
    {
        // alpha is the obliquity to the ecliptic, in degrees
        float alpha = (float)UniCsts.planetsDict[UniCsts.planets.Earth][CelestialBodyParamsBase.planetaryParams.axialTilt.ToString()].value;
        float ca = Mathf.Cos(alpha*(float)UniCsts.deg2rad);
        float sa = Mathf.Sin(alpha*(float)UniCsts.deg2rad);
        _ECI_2_HECS = new Matrix4x4(new Vector4(1f,0f), new Vector4(0f,ca,-sa), new Vector4(0f,sa,ca), Vector4.zero);
    }

    public Vector3d Transform_ECI_2_HECS(Vector3 vectorIn_ECI_Frame)
    {
        Vector4 vec = new Vector4(vectorIn_ECI_Frame.x, vectorIn_ECI_Frame.y, vectorIn_ECI_Frame.z, 0f);
        vec = ECI_2_HECS*vec;
        return new Vector3d(vec.x, vec.y, vec.z);
    }

    public Vector3d Transform_HECS_2_ECI(Vector3 vectorIn_HECS_Frame)
    {
        Vector4 vec = new Vector4(vectorIn_HECS_Frame.x, vectorIn_HECS_Frame.y, vectorIn_HECS_Frame.z, 0f);
        vec = HECS_2_ECI*vec;
        return new Vector3d(vec.x, vec.y, vec.z);
    }

}