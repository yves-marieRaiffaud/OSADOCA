using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavBall : MonoBehaviour
{
    public UniverseRunner universeRunner;
    public GameObject navBallGO;
    Quaternion navBallBaseRot;
    public Vector3 navBallSphereNorthPoleAxis; // == (0;1;0), north axis along the world +Y-Axis

    CelestialBody orbitedBody;
    Spaceship ship;

    void Start()
    {
        if(universeRunner != null)
            universeRunner.startIsDone.AddListener(InitNavBall);
    }

    void InitNavBall()
    {
        // Method called ONLY once the 'Start()' method of universeRunner has finished
        if(universeRunner.activeSpaceship != null)
            ship = universeRunner.activeSpaceship;
        else {
            Debug.LogWarning("'universeRunner.activeSpaceship' is null. Navball won't be updated");
            return;
        }

        if(ship.orbit != null)
            orbitedBody = universeRunner.activeSpaceship.orbit.orbitedBody;
        navBallBaseRot = navBallGO.transform.rotation;
    }

    void LateUpdate()
    {
        if(ship==null || orbitedBody==null)
            return;
        
        Quaternion quatRot = ship.transform.rotation * Quaternion.Inverse(orbitedBody.transform.rotation) * navBallBaseRot;
        
        navBallGO.transform.rotation = quatRot;
    }
}
