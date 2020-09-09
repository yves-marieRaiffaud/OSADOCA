using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RCSThrustersGroup : MonoBehaviour
{
    [Tooltip("Modifying this value will modify the thrust power of every RCSThruster in this folder")]
    public float commonThrustPower;

    void OnValidate()
    {
        for(int i=0; i<transform.childCount; i++) {
            RCSThruster th = transform.GetChild(i).GetComponent<RCSThruster>();
            th._RCSThrustPower = commonThrustPower;
        }
    }
}