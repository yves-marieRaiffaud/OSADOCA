using UnityEngine;
using System.IO;
using Mathd_Lib;
using System.Collections;
using System.Collections.Generic;

public class simUI : MonoBehaviour
{
    public UniverseRunner universeRunner;
    public TMPro.TMP_Text velocityVal;
    public TMPro.TMP_Text altitudeVal;
    //=================
    public bool saveOrbitalDataToText=false;
    string fileName = "dataOrbit.txt";
    StreamWriter sr;
    //=================
    private Distance altitude_tMinus;
    //=================
    private float uiUpdateValues_period = 0.2f; // in s
    private bool delayCoroutine;
    //=================
    private MainNozzle_Control shipMainControlNozzleScript;
    private SimRocketCam_MouseOrbit rocketCamScript;
    //=================
    void Start()
    {
        delayCoroutine = true;
        if(universeRunner == null) { Debug.LogError("The UniverseRunner instance has not been set in the editor."); return; }
        //=====
        if(saveOrbitalDataToText)
        {
            if (File.Exists(fileName))
                Debug.Log(fileName+" already exists.");
            sr = File.CreateText(fileName);
        }
        //=====
        shipMainControlNozzleScript = universeRunner.activeSpaceship.transform.GetComponentInChildren<MainNozzle_Control>(false);
        rocketCamScript = universeRunner.activeSpaceship.transform.GetComponentInChildren<SimRocketCam_MouseOrbit>(false);
        //=====
        StartCoroutine(UpdateUICoroutine());
    }

    private IEnumerator UpdateUICoroutine()
    {
        while (true)
        {
            if(delayCoroutine)
            {
                delayCoroutine = false;
                // Waiting 2 seconds for the Spaceship to correctly initialized and for the first FixedUpdate to be done
                // Then, not having any issue when computing the spaceship 'GetShipAltitude()'
                yield return new WaitForSeconds(1);
                altitude_tMinus = new Distance(universeRunner.activeSpaceship.GetShipAltitude(), Units.distance.km);
            }
            UpdateUIValues();
            yield return new WaitForSeconds(uiUpdateValues_period);
        }
    }

    private void UpdateUIValues()
    {
        if(altitude_tMinus == null)
            altitude_tMinus = new Distance(universeRunner.activeSpaceship.GetShipAltitude(), Units.distance.km);

        Distance currAltitude = new Distance(universeRunner.activeSpaceship.GetShipAltitude(), Units.distance.km);
        Velocity currVelocity = new Velocity((Mathd.Abs(currAltitude.value-altitude_tMinus.value) / uiUpdateValues_period), Units.velocity.km_s);
        
        string velocity = UsefulFunctions.DoubleToSignificantDigits(currVelocity.ConvertTo(Units.velocity.m_s).value, UniCsts.UI_SIGNIFICANT_DIGITS);
        string altitude = UsefulFunctions.DoubleToSignificantDigits(currAltitude.value, UniCsts.UI_SIGNIFICANT_DIGITS);

        velocityVal.text = velocity + " m/s";
        altitudeVal.text = altitude + " " + currAltitude.unit.ToString();

        altitude_tMinus = currAltitude;

        if(!saveOrbitalDataToText) { return; }
        sr.WriteLine (velocity + ";" + altitude);
    }

    public void ToggleThrustVectorDisplay()
    {
        shipMainControlNozzleScript.showThrustVector = !shipMainControlNozzleScript.showThrustVector;
        if(shipMainControlNozzleScript.thrustLR_GO != null)
            shipMainControlNozzleScript.thrustLR_GO.SetActive(shipMainControlNozzleScript.showThrustVector);
        //universeRunner.activeSpaceship.propulsionManager.ToggleRCSVectorsDisplay();
    }

    public void ToggleRocketCameraRotationLock()
    {
        rocketCamScript.freezeCamRotation = !rocketCamScript.freezeCamRotation;
    }



    void OnApplicationQuit()
    {
        if(!saveOrbitalDataToText) { return; }
        sr.Close();
    }
}