using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class OrbitalTypes
{
    [SerializeField] public enum orbitDefinitionType { rarp, rpe, pe }; // Specify the parameters to use to define the orbit
    [SerializeField] public enum bodyPositionType { nu, m0, l0, t0 }; // Specify the parameter to use to position the body on its orbit
    [SerializeField] public enum orbitalParamsUnits { km_degree, AU_degree };

    [System.Flags, SerializeField] public enum typeOfVectorDir { vernalPoint=2, vpAxisRight=4, vpAxisUp=8, apogeeLine=16, ascendingNodeLine=32, tangentialVec=64, radialVec=128, velocityVec=256, accelerationVec=512 };
    
    [SerializeField] public enum typeOfOrbit { realOrbit, predictedOrbit };
}