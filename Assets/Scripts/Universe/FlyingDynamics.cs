using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mathd_Lib;
//using System.IO;
//using System.Linq;

using Universe;

namespace FlyingObjects
{
    public class FlyingDynamics
    {
        UniverseRunner universeRunner;

        public FlyingDynamics(UniverseRunner _universeRunner)
        {
            universeRunner = _universeRunner;
        }

        /*/// <summary>
        /// Applies the Leapfrog method and move the 'spaceship' to its updated position. Updates the spaceship acceleration, velocity and realPosition members 
        /// </summary>
        public void Leapfrog_Method(Spaceship spaceship)
        {
            // Leapfrog integration method here
            // 1/2 kick
            spaceship.relativeVel +=  spaceship.relativeAcc * Time.fixedDeltaTime/2d;
            // Drift
            spaceship.transform.position += (Vector3) (spaceship.relativeVel*Time.fixedDeltaTime/1000d);
            spaceship.relativeAcc = Kepler.GetGravAcc(spaceship.transform.position, spaceship.orbitedBody.transform.position, spaceship.orbitedBody.mu);
            // 1/2 kick
            spaceship.relativeVel +=  spaceship.relativeAcc * Time.fixedDeltaTime/2d;
        }*/

        public void Compute_NewPositions()
        {
            
        }

        public void Gravity_Method(Spaceship spaceship)
        {
            spaceship.relativeAcc = Kepler.GetGravAcc(spaceship.transform.position, spaceship.orbitedBody.transform.position, spaceship.orbitedBody.mu)/1000d;
            spaceship._rigidbody.AddForce((Vector3) (spaceship.relativeAcc), ForceMode.Acceleration);
        }

    }
}