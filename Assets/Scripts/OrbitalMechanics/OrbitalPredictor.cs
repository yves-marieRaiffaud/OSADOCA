using System;
using System.Collections;
using UnityEngine;
using Mathd_Lib;
using System.Collections.Generic;

[Serializable]
public class OrbitalPredictor
{
    FlyingObjCommonParams bodyToPredict;
    [SerializeField] CelestialBody orbitedBody;
    [SerializeField] Orbit linkedOrbit; // initial orbit that the predictor will use to predict its evolution 
    public Orbit predictedOrbit; // orbit that the predictor computed

    public OrbitalPredictor(FlyingObjCommonParams predictedBody, CelestialBody body, Orbit orbit)
    {
        bodyToPredict = predictedBody;
        orbitedBody = body;
        linkedOrbit = orbit;
        Debug.Log("bodyToPredict: " + bodyToPredict._gameObject.name + "; orbitedBody" + orbitedBody.name + "; orbit:" + orbit.suffixGO);
        smartPredictor();
    }

    public void DebugLog_Predictor()
    {
        Debug.LogFormat("predictedOrbit\t | linkedOrbit");
        Debug.LogFormat("orbitedBody: {0}; orbitingBody: {1}", orbitedBody.name, bodyToPredict._gameObject.name);
        Debug.LogFormat("p: {0}\n e: {1}\n i: {2}\n lAscN: {3}\n omega: {4}\n nu: {5}\n bodyPosType: {6}\n selectedVectorsDir: {7}",
        predictedOrbit.param.p, predictedOrbit.param.e, predictedOrbit.param.i, predictedOrbit.param.lAscN, predictedOrbit.param.omega, predictedOrbit.param.nu,
        predictedOrbit.param.bodyPosType, predictedOrbit.param.selectedVectorsDir);

        Debug.LogFormat("p: {0}\n e: {1}\n i: {2}\n lAscN: {3}\n omega: {4}\n nu: {5}\n bodyPosType: {6}\n selectedVectorsDir: {7}",
        linkedOrbit.param.p, linkedOrbit.param.e, linkedOrbit.param.i, linkedOrbit.param.lAscN, linkedOrbit.param.omega, linkedOrbit.param.nu,
        linkedOrbit.param.bodyPosType, linkedOrbit.param.selectedVectorsDir);
    }

    public void smartPredictor()
    {
        if(bodyToPredict == null || orbitedBody == null)
            return;

        Vector3d velocityVec = bodyToPredict.orbitedBodyRelativeVel;
        double speedMagn = velocityVec.magnitude;
        double circularSpeed = linkedOrbit.GetCircularOrbitalSpeed();
        double liberationSpeed = Mathd.Sqrt(2d) * circularSpeed;
        // Calculate orbital speed and compare it to the one needed to reach a circular orbit

        double µ = orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.mu.ToString()].value;
        Vector3d rVec = bodyToPredict.GetRelativeRealWorldPosition()*UniCsts.km2m;
        double r = rVec.magnitude;

        // Computing semi-major axis length
        double a = r * µ*Mathd.Pow(10,13) / (2*µ*Mathd.Pow(10,13) - r*Mathd.Pow(speedMagn,2)) * UniCsts.m2km;
        // Computing specific angular momentum vector, in m2.s-1 (perpendicular to the orbit plane)
        Vector3d h = Vector3d.Cross(rVec, velocityVec);
        // Computing eccentricity vector, pointing from the apoapsis to the periapsis
        Vector3d eVec = Vector3d.Cross(velocityVec, h) / (µ*UniCsts.µExponent) - rVec/r;
        
        double e = eVec.magnitude;
        double p = a*(1-e*e);
        
        OrbitalParams predOrbitParams = ScriptableObject.CreateInstance<OrbitalParams>();
        predOrbitParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.predictedOrbit;
        predOrbitParams.orbitDefType = OrbitalTypes.orbitDefinitionType.pe;

        switch(UsefulFunctions.CastStringToGoTags(bodyToPredict._gameObject.tag))
        {
            case UniverseRunner.goTags.Spaceship:
                predOrbitParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.km_degree;
                break;
            
            case UniverseRunner.goTags.Planet:
                predOrbitParams.orbParamsUnits = OrbitalTypes.orbitalParamsUnits.AU_degree;
                break;
        }

        predOrbitParams.p = p;
        predOrbitParams.e = e;
        predOrbitParams.i = linkedOrbit.param.i;
        predOrbitParams.lAscN = linkedOrbit.param.lAscN;
        predOrbitParams.omega = linkedOrbit.param.omega;
        predOrbitParams.nu = linkedOrbit.param.nu;
        predOrbitParams.bodyPosType = OrbitalTypes.bodyPositionType.nu;
        predOrbitParams.orbitDrawingResolution = 368;
        predOrbitParams.drawOrbit = true;
        predOrbitParams.drawDirections = true;
        predOrbitParams.selectedVectorsDir = (OrbitalTypes.typeOfVectorDir)254;

        predictedOrbit = new Orbit(predOrbitParams, orbitedBody, bodyToPredict._gameObject);

        if(predOrbitParams.drawDirections) {
            foreach(OrbitalTypes.typeOfVectorDir vectorDir in Enum.GetValues(typeof(OrbitalTypes.typeOfVectorDir))) {
                if (predOrbitParams.selectedVectorsDir.HasFlag(vectorDir)) {
                    if(vectorDir.Equals(OrbitalTypes.typeOfVectorDir.radialVec) || vectorDir.Equals(OrbitalTypes.typeOfVectorDir.tangentialVec))
                        predictedOrbit.DrawDirection(vectorDir, 0.1f, 50f, bodyToPredict._gameObject.transform.position);
                    else
                        predictedOrbit.DrawDirection(vectorDir, 0.1f, 50f);
                }
            }
        }
        //DebugLog_Predictor();
    }
}