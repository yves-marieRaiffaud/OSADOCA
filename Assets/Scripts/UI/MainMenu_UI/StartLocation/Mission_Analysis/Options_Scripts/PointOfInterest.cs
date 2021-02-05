using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class PointOfInterest : MonoBehaviour
{
    public bool isActive {get; set;}

    public UnityEvent OnClick;
    public UnityEvent OnEnter;
    public UnityEvent OnExit;
}