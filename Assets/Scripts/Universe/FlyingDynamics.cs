using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
//using System.IO;
//using System.Linq;

using KeplerGrav = Kepler.Gravitational;
using Universe;
using ObjHd = CommonMethods.ObjectsHandling;
using UniCsts = UniverseConstants;

namespace FlyingObjects
{
    public class FlyingDynamics
    {
        UniverseRunner universeRunner;
        public FlyingDynamics(UniverseRunner _universeRunner)
        {
            universeRunner = _universeRunner;
        }

        public void Init_FlyingObject<T1>(T1 body)
        where T1: Dynamic_Obj_Common
        {
            // INIT ORBIT
            body.orbit = new Orbit(body.orbitalParams, body.orbitalParams.orbitedBody, body._gameObject);

            // INIT POSITION
            Vector3d worldPos = Orbit.GetWorldPositionFromOrbit(body.orbit);
            body.realPosition = worldPos + new Vector3d(body.orbit.orbitedBody.transform.position);
            body._gameObject.transform.position = (Vector3)body.realPosition;
            body._rigidbody.MovePosition((Vector3)body.realPosition);
            Debug.Log("realPosition Init for " + body._gameObject.name + " = " + new Vector3d(body._gameObject.transform.position-body.orbit.orbitedBody.transform.position));

            // INIT ORBITAL SPEED
            //_relativeVel = new Vector3d(0d,0d,7.844d);
            Vector3d radVec = body.orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.radialVec);
            double radVel = body.orbit.GetRadialSpeed().ConvertTo(Units.velocity.kms).val;
            Vector3d tangVec = body.orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.tangentialVec);
            double tangVel = body.orbit.GetTangentialSpeed().ConvertTo(Units.velocity.kms).val;
            body.relativeVel = radVec*radVel + tangVec*tangVel;
            Debug.LogFormat("body.relativeVel for {0} = {1} km/s", body._gameObject.name, body.relativeVel);
            if(body.orbit.orbitedBody != null)
                body.relativeVel += body.orbit.orbitedBody.relativeVel;
        }

        public void Init_State_Variables<T1>(T1 bodyToInit)
        where T1 : Dynamic_Obj_Common
        {
            bodyToInit.relativeAcc = KeplerGrav.GetGravAcc(bodyToInit._gameObject.transform.position, bodyToInit.orbit.orbitedBody.transform.position, bodyToInit.orbit.orbitedBody.settings.planetaryParams.mu.val*UniCsts.µExponent)*Units.M2KM;
            bodyToInit.relativeAcc += bodyToInit.orbit.orbitedBody.relativeAcc;
            Debug.LogFormat("State Variables for {0}: acc = {1}; vel = {2}; pos = {3}",bodyToInit._gameObject.name,bodyToInit.relativeAcc,bodyToInit.relativeVel,bodyToInit.realPosition-bodyToInit.orbit.orbitedBody.realPosition);
        }

        public void GravitationnalStep()
        {
            // First computing the acceleration updates for each Star, Planet or Spaceship 
            foreach(Transform obj in universeRunner.physicsObjArray)
                Compute_NewPositions(obj, obj.tag);

            // Once everything has been computed, apply the new accelerations at every objects
            foreach(Transform obj in universeRunner.physicsObjArray)
                Apply_NewPositions(obj, obj.tag);
        }

        public void Compute_NewPositions(Transform obj, string objTag)
        {
            // Compute acceleration, velocity and the new position of either a Planet or a Spaceship, due to gravitational pull
            CelestialBody orbitedBody;
            switch(ObjHd.Str_2_GoTags(objTag))
            {
                case UniverseRunner.goTags.Planet:
                    CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                    if(celestBody.orbitedBody != null) {
                        orbitedBody = celestBody.orbitedBody.GetComponent<CelestialBody>();
                        //GravitationalUpdate<CelestialBody, CelestialBodySettings>(orbitedBody, celestBody);
                        GravitationalUpdate<CelestialBody>(orbitedBody, celestBody);
                    }
                    break;

                case UniverseRunner.goTags.Spaceship:
                    Spaceship ship = obj.GetComponent<Spaceship>();
                    orbitedBody = ship.orbit.orbitedBody.GetComponent<CelestialBody>();
                    //GravitationalUpdate<Spaceship, SpaceshipSettings>(orbitedBody, ship);
                    GravitationalUpdate<Spaceship>(orbitedBody, ship);
                    break;
            }
        }

        public void GravitationalUpdate<T1/*,T2*/>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common //where T2: Dynamic_Obj_Settings
        {
            if(orbitingBody is T1 && orbitedBody != null)
            {
                //T2 settings = GetObjectSettings<T1, T2>(orbitingBody);
                if(universeRunner.simEnv.useNBodySimulation.value)
                    Compute_NBody_Grav_Acc<T1/*, T2*/>(orbitedBody, orbitingBody);
                else
                    Compute_Leapfrog_Values<T1>(orbitedBody, orbitingBody);
                    //Compute_SingleBody_Grav_Acc<T1/*,T2*/>(orbitedBody, orbitingBody);
            }
        }
        public void Compute_SingleBody_Grav_Acc<T1/*,T2*/>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common //where T2: Dynamic_Obj_Settings
        {
            orbitingBody.relativeAcc = KeplerGrav.GetGravAcc(orbitingBody._gameObject.transform.position, orbitedBody.transform.position, orbitedBody.settings.planetaryParams.mu.val*UniCsts.µExponent) * Units.M2KM;
        }
        public void Compute_Leapfrog_Values<T1>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common
        {
            // Leapfrog integration method here
            // 1/2 kick
            orbitingBody.relativeVel +=  orbitingBody.relativeAcc * Time.fixedDeltaTime/2d;
            // Drift
            orbitingBody.realPosition += orbitingBody.relativeVel*Time.fixedDeltaTime;
            //Debug.LogFormat("State Variables 1 for {0}: acc = {1}; vel = {2}; pos = {3}",orbitingBody._gameObject.name,orbitingBody.relativeAcc,orbitingBody.relativeVel,orbitingBody.realPosition-orbitedBody.realPosition);
        }

        public void Compute_Second_Part_Leapfrog<T1>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common
        {
            orbitingBody.relativeAcc = KeplerGrav.GetGravAcc(orbitingBody._gameObject.transform.position, orbitedBody._gameObject.transform.position, orbitedBody.settings.planetaryParams.mu.val*UniCsts.µExponent) * Units.M2KM;
            orbitingBody.relativeAcc += orbitedBody.relativeAcc;
            // 1/2 kick
            orbitingBody.relativeVel +=  orbitingBody.relativeAcc * Time.fixedDeltaTime/2d;
            //Debug.LogFormat("State Variables 2 for {0}: acc = {1}; vel = {2}; pos = {3}",orbitingBody._gameObject.name,orbitingBody.relativeAcc,orbitingBody.relativeVel,orbitingBody.realPosition-orbitedBody.realPosition);
        }


        public void Compute_NBody_Grav_Acc<T1/*,T2*/>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common //where T2: Dynamic_Obj_Settings
        {
            // TO IMPLEMENT HERE
        }

        public void Apply_NewPositions(Transform obj, string objTag)
        {
            switch(ObjHd.Str_2_GoTags(objTag))
            {
                case UniverseRunner.goTags.Planet:
                    CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                    celestBody._rigidbody.MovePosition((Vector3)celestBody.realPosition);
                    Compute_Second_Part_Leapfrog<CelestialBody>(celestBody.orbit.orbitedBody, celestBody);
                    break;

                case UniverseRunner.goTags.Spaceship:
                    Spaceship ship = obj.GetComponent<Spaceship>();
                    ship._rigidbody.MovePosition((Vector3)ship.realPosition);
                    Compute_Second_Part_Leapfrog<Spaceship>(ship.orbit.orbitedBody, ship);
                    break;
            }
        }
    }
}