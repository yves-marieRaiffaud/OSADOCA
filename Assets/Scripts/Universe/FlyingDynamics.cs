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
        //-------------------------------------------
        //-------------------------------------------
        public void Init_FlyingObject<T1>(T1 body)
        where T1: Dynamic_Obj_Common
        {
            // INIT ORBIT
            body.orbit = new Orbit(body.orbitalParams, body.orbitalParams.orbitedBody, body._gameObject);

            // INIT POSITION
            Vector3d worldPos = Orbit.GetWorldPositionFromOrbit(body.orbit);
            body.realPosition = worldPos + body.orbit.orbitedBody.realPosition;
            body._gameObject.transform.position = (Vector3)body.realPosition;

            // INIT ORBITAL SPEED
            Vector3d radVec = body.orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.radialVec);
            double radVel = body.orbit.GetRadialSpeed().ConvertTo(Units.velocity.kms).val;
            Vector3d tangVec = body.orbit.ComputeDirectionVector(OrbitalTypes.typeOfVectorDir.tangentialVec);
            double tangVel = body.orbit.GetTangentialSpeed().ConvertTo(Units.velocity.kms).val;
            body.relativeVel = radVec * radVel + tangVec * tangVel;
            if (body.orbit.orbitedBody == null)
                return;
            body.relativeVel += body.orbit.orbitedBody.relativeVel;

            Debug.LogFormat("Init Variables for {0}:\nbody.relativeVel = {1}\nbody.realPosition = {2}\nbody.transform.position = {3}\nrelativePosition = {4}\nposition norm = {5} = ",body._gameObject.name,body.relativeVel,body.realPosition,body._gameObject.transform.position,(body.realPosition-body.orbit.orbitedBody.realPosition),(body.realPosition-body.orbit.orbitedBody.realPosition).magnitude);
        }
        public void Init_State_Variables<T1>(T1 bodyToInit)
        where T1 : Dynamic_Obj_Common
        {
            bodyToInit.relativeAcc = KeplerGrav.GetGravAcc(bodyToInit.realPosition, bodyToInit.orbit.orbitedBody.realPosition, bodyToInit.orbit.orbitedBody.settings.planetaryParams.mu.val*UniCsts.µExponent)*Units.M2KM;
            bodyToInit.relativeAcc += bodyToInit.orbit.orbitedBody.relativeAcc;
            Debug.LogFormat("Init State Variables for {0}:\nbody.relativeVel = {1}\nbody.realPosition = {2}\nbody.transform.position = {3}\nbody.relativeAcc = {4}\nrelativePosition = {5}\nposition norm = {6} = ",bodyToInit._gameObject.name,bodyToInit.relativeVel,bodyToInit.realPosition,bodyToInit._gameObject.transform.position,bodyToInit.relativeAcc,(bodyToInit.realPosition-bodyToInit.orbit.orbitedBody.realPosition),(bodyToInit.realPosition-bodyToInit.orbit.orbitedBody.realPosition).magnitude);
        }
        //-------------------------------------------
        //-------------------------------------------
        public void GravitationnalStep()
        {
            // First computing the acceleration updates for each Star, Planet or Spaceship 
            foreach(Dynamic_Obj_Common obj in universeRunner.physicsObjArray) {
                if(obj._gameObject.transform.tag != UniverseRunner.goTags.Star.ToString())
                    Compute_NewPositions(obj);
            }

            // Once everything has been computed, apply the new accelerations at every objects
            foreach(Dynamic_Obj_Common obj in universeRunner.physicsObjArray) {
                if(obj._gameObject.transform.tag != UniverseRunner.goTags.Star.ToString())
                    Apply_NewPositions(obj);
            }
        }
        //-------------------------------------------
        //-------------------------------------------
        public void Compute_NewPositions(Dynamic_Obj_Common obj)
        {
            // Compute acceleration, velocity and the new position of either a Planet or a Spaceship, due to gravitational pull
            if(obj.orbit.orbitedBody != null)
                GravitationalUpdate<Dynamic_Obj_Common>(obj.orbit.orbitedBody, obj);
            else
                Debug.LogWarningFormat("WARNING ! 'orbit.orbitedBody' of {0} is null. Cannot continue with the 'NewPositions' method.",obj._gameObject.name);
        }
        public void GravitationalUpdate<T1>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common
        {
            if(orbitingBody is T1 && orbitedBody != null) {
                if(universeRunner.simEnv.useNBodySimulation.value)
                    Compute_NBody_Grav_Acc<T1>(orbitedBody, orbitingBody);
                else
                    Compute_Leapfrog_Values<T1>(orbitedBody, orbitingBody);
            }
        }
        public void Compute_NBody_Grav_Acc<T1>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common
        {
            // TO IMPLEMENT HERE
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
        //-------------------------------------------
        //-------------------------------------------
        public void Apply_NewPositions(Dynamic_Obj_Common obj)
        {
            obj._rigidbody.MovePosition((Vector3)obj.realPosition);
            if(obj.orbit.orbitedBody != null)
                Compute_Second_Part_Leapfrog<Dynamic_Obj_Common>(obj.orbit.orbitedBody, obj);
            else
                Debug.LogWarningFormat("WARNING ! 'orbit.orbitedBody' of {0} is null. Cannot continue with the 'ApplyNewPositions' method and run 'Compute_Second_Part_Leapfrog()'.", obj._gameObject.name);

        }
        public void Compute_Second_Part_Leapfrog<T1>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common
        {
            // Update acceleration
            orbitingBody.relativeAcc = KeplerGrav.GetGravAcc(orbitingBody.realPosition, orbitedBody.realPosition, orbitedBody.settings.planetaryParams.mu.val*UniCsts.µExponent) * Units.M2KM;
            orbitingBody.relativeAcc += orbitedBody.relativeAcc;
            // 1/2 kick
            orbitingBody.relativeVel +=  orbitingBody.relativeAcc * Time.fixedDeltaTime/2d;
            //Debug.LogFormat("State Variables 2 for {0}: acc = {1}; vel = {2}; pos = {3}",orbitingBody._gameObject.name,orbitingBody.relativeAcc,orbitingBody.relativeVel,orbitingBody.realPosition-orbitedBody.realPosition);
        }
        //-------------------------------------------
        //-------------------------------------------
    }
}