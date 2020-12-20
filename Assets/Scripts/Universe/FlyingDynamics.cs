using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
//using System.IO;
//using System.Linq;

using KeplerGrav = Kepler.Gravitational;
using Universe;
using ObjHd = CommonMethods.ObjectsHandling;

namespace FlyingObjects
{
    public class FlyingDynamics
    {
        UniverseRunner universeRunner;
        public FlyingDynamics(UniverseRunner _universeRunner)
        {
            universeRunner = _universeRunner;
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
                    Compute_SingleBody_Grav_Acc<T1/*,T2*/>(orbitedBody, orbitingBody);
            }
        }

        public void Apply_NewPositions(Transform obj, string objTag)
        {
            switch(ObjHd.Str_2_GoTags(objTag))
            {
                case UniverseRunner.goTags.Planet:
                    CelestialBody celestBody = obj.GetComponent<CelestialBody>();
                    celestBody._rigidbody.AddForce((Vector3) celestBody.relativeAcc, ForceMode.Acceleration);
                    break;

                case UniverseRunner.goTags.Spaceship:
                    Spaceship ship = obj.GetComponent<Spaceship>();
                    ship._rigidbody.AddForce((Vector3) ship.relativeAcc, ForceMode.Acceleration);
                    break;
            }
        }

        public void Compute_SingleBody_Grav_Acc<T1/*,T2*/>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common //where T2: Dynamic_Obj_Settings
        {
            orbitingBody.relativeAcc = KeplerGrav.GetGravAcc(orbitingBody._gameObject.transform.position, orbitedBody.transform.position, orbitedBody.settings.planetaryParams.mu.val) * Units.M2KM;
        }
        public void Compute_NBody_Grav_Acc<T1/*,T2*/>(CelestialBody orbitedBody, T1 orbitingBody)
        where T1: Dynamic_Obj_Common //where T2: Dynamic_Obj_Settings
        {
            // TO IMPLEMENT HERE
        }

    }
}