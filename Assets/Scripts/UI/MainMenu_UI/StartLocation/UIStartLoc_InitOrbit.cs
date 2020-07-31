using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mathd_Lib;

public class UIStartLoc_InitOrbit : MonoBehaviour
{
    [Header("Orbit.ing.ed body and Spacecraft")]
    public GameObject orbitingSpacecraft;
    //==================================================================
    [Header("Orbit Definition Panel")]
    public TMPro.TMP_Dropdown unitsDropdown;
    public TMPro.TMP_Dropdown orbitDefType;

    public TMPro.TMP_Text orbitShape_p1_txt;
    public TMPro.TMP_InputField orbitShape_p1_field;
    public TMPro.TMP_Text orbitShape_p2_txt;
    public TMPro.TMP_InputField orbitShape_p2_field;

    public TMPro.TMP_InputField inclination_field;
    public TMPro.TMP_InputField lAscN_field;

    public TMPro.TMP_InputField periapsisArg_field;

    public TMPro.TMP_Dropdown bodyPosType;
    public TMPro.TMP_Text bodyPos_txt;
    public TMPro.TMP_InputField bodyPos_field;
    //======================
    [HideInInspector] public CelestialBody orbitedBody; // Current selected CelestialBody
    private string surnameOrbitShape_p1;
    private string surnameOrbitShape_p2;
    //==================================================================
    [Header("Orbit Info Panel")]
    public TMPro.TMP_Text info_orbitShape_p1_txt;
    public TMPro.TMP_Text info_orbitShape_p1_val;
    public TMPro.TMP_Text info_orbitShape_p1_unit;
    public TMPro.TMP_Text info_orbitShape_p2_txt;
    public TMPro.TMP_Text info_orbitShape_p2_val;
    public TMPro.TMP_Text info_orbitShape_p2_unit;

    public TMPro.TMP_Text info_orbit_a_val;
    public TMPro.TMP_Text info_orbit_a_unit;
    public TMPro.TMP_Text info_orbit_b_val;
    public TMPro.TMP_Text info_orbit_b_unit;
    public TMPro.TMP_Text info_orbit_c_val;
    public TMPro.TMP_Text info_orbit_c_unit;

    public TMPro.TMP_Text info_bodyPos_p1_txt;
    public TMPro.TMP_Text info_bodyPos_p1_val;
    public TMPro.TMP_Text info_bodyPos_p1_unit;
    public TMPro.TMP_Text info_bodyPos_p2_txt;
    public TMPro.TMP_Text info_bodyPos_p2_val;
    public TMPro.TMP_Text info_bodyPos_p2_unit;
    public TMPro.TMP_Text info_bodyPos_p3_txt;
    public TMPro.TMP_Text info_bodyPos_p3_val;
    public TMPro.TMP_Text info_bodyPos_p3_unit;

    public TMPro.TMP_Text info_meanMotion_val;
    public TMPro.TMP_Text info_orbitalPeriod_val;
    //==================================================================
    [Header("Orbit Preview UI Elements")]
    public RectTransform perihelionPinpoint;
    public RectTransform aphelionPinpoint;
    //==================================================================
    private bool ORBITAL_PARAMS_VALID = true;
    public Orbit previewedOrbit;

    void OnEnable()
    {
        InitDropdowns();

        orbitDefType.onValueChanged.AddListener(delegate { OnOrbitDefTypeDropdownValueChanged(); });
        unitsDropdown.onValueChanged.AddListener(delegate { OnUnitsDropdownValueChanged(); });
        bodyPosType.onValueChanged.AddListener(delegate { OnBodyPosDropdownValueChanged(); });

        ClearOrbitInfoValues();
    }

    private void InitDropdowns()
    {
        UsefulFunctions.SetUpDropdown(unitsDropdown, new List<string>(System.Enum.GetNames(typeof(OrbitalTypes.orbitalParamsUnits))));
        UsefulFunctions.SetUpDropdown(orbitDefType, new List<string>(System.Enum.GetNames(typeof(OrbitalTypes.orbitDefinitionType))));
        UsefulFunctions.SetUpDropdown(bodyPosType, new List<string>(System.Enum.GetNames(typeof(OrbitalTypes.bodyPositionType))));

        // Calling the 'on-values changed' functions to init the fields
        OnOrbitDefTypeDropdownValueChanged();
        OnUnitsDropdownValueChanged();
        OnBodyPosDropdownValueChanged();
    }

    private void OnOrbitDefTypeDropdownValueChanged()
    {
        // Updating text depending on the 'orbitDefinitionType'
        switch(orbitDefType.value)
        {
            case 0:
                // 'rarp' enum
                orbitShape_p1_txt.text = "Aphelion";
                surnameOrbitShape_p1 = "ra";
                orbitShape_p2_txt.text = "Perihelion";
                surnameOrbitShape_p2 = "rp";
                break;
            case 1:
                // 'rpe' enum
                orbitShape_p1_txt.text = "Perihelion";
                surnameOrbitShape_p1 = "rp";
                orbitShape_p2_txt.text = "Excentricity";
                surnameOrbitShape_p2 = "e";
                break;
            case 2:
                // 'pe' enum
                orbitShape_p1_txt.text = "Semi-latus rectum";
                surnameOrbitShape_p1 = "p";
                orbitShape_p2_txt.text = "Excentricity"; 
                surnameOrbitShape_p2 = "e";
                break;
        }
        // Every time the orbitDefType as changed, the placeholder shall be updated
        OnUnitsDropdownValueChanged();
    }

    private void OnUnitsDropdownValueChanged()
    {
        // Updating placeholders of the input fields depending on the selected 'orbitalParamUnits' enum
        string p2Dimension;
        switch(unitsDropdown.value)
        {
            case 0:
                // 'km_degree' enum
                // p1 is a distance in every case: either 'ra', 'rp', or 'p'
                orbitShape_p1_field.placeholder.GetComponent<TMPro.TMP_Text>().text = surnameOrbitShape_p1 + " in km";

                // p2 can be either 'rp' or 'e'
                p2Dimension = " in km";
                if(surnameOrbitShape_p2.Equals("e")) { p2Dimension = " no dimension"; }
                orbitShape_p2_field.placeholder.GetComponent<TMPro.TMP_Text>().text = surnameOrbitShape_p2 + p2Dimension;
                break;
            
            case 1:
                // 'AU_degree' enum
                // p1 is a distance in every case: either 'ra', 'rp', or 'p'
                orbitShape_p1_field.placeholder.GetComponent<TMPro.TMP_Text>().text = surnameOrbitShape_p1 + " in AU";

                // p2 can be either 'rp' or 'e'
                p2Dimension = " in AU";
                if(surnameOrbitShape_p2.Equals("e")) { p2Dimension = " no dimension"; }
                orbitShape_p2_field.placeholder.GetComponent<TMPro.TMP_Text>().text = surnameOrbitShape_p2 + p2Dimension;
                break;
        }
    }

    private void OnBodyPosDropdownValueChanged()
    {
        switch(bodyPosType.value)
        {
            case 0:
                // nu, true anomaly
                bodyPos_txt.text = "True anomaly";
                bodyPos_field.placeholder.GetComponent<TMPro.TMP_Text>().text = "nu in °";
                break;
            case 1:
                // nu, true anomaly
                bodyPos_txt.text = "Mean anomaly";
                bodyPos_field.placeholder.GetComponent<TMPro.TMP_Text>().text = "M0 in °";
                break;
            case 2:
                // nu, true anomaly
                bodyPos_txt.text = "Mean longitude";
                bodyPos_field.placeholder.GetComponent<TMPro.TMP_Text>().text = "L0 in °";
                break;
            case 3:
                // nu, true anomaly
                bodyPos_txt.text = "Time of passage";
                bodyPos_field.placeholder.GetComponent<TMPro.TMP_Text>().text = "T0 in hh:mm:ss";
                break;
        }
    }

    public void OnUpdateOrbit_BtnClick()
    {
        ORBITAL_PARAMS_VALID = true;
        CheckForEmptyFields();
        OrbitalParams orbitalParams = Create_OrbitalParams_File();
        if(ORBITAL_PARAMS_VALID)
        {
            previewedOrbit = new Orbit(orbitalParams, orbitedBody, orbitingSpacecraft);
            previewedOrbit.UpdateLineRendererPos();
            UpdateSpacecraftPosition();
            UpdatePinpointsPosition();
            UpdateOrbitInfoValues();
            UpdateOrbitInfoUnits();
        }
        else {
            Debug.LogWarning("ORBITAL_PARAMS_VALID has been set to false. Check the entered orbital parameters.");
        }
    }

    private void UpdateSpacecraftPosition()
    {
        OrbitalTypes.bodyPositionType inputPosType = UsefulFunctions.String2_bodyPosTypeEnum(bodyPosType.options[bodyPosType.value].text);
        Vector3d shipWorldPos = Orbit.GetWorldPositionFromOrbit(previewedOrbit, inputPosType);
        orbitingSpacecraft.transform.position = (Vector3) shipWorldPos + orbitedBody.gameObject.transform.position;
    }

    private void UpdatePinpointsPosition()
    {
        // Positioning the perihelion and aphelion pinpoints in world space
        // Can only do so IF the excentricity e > 0 (if e=0, there is no aphelion nor perihelion as orbit is circular)
        if(previewedOrbit.param.e > 0d)
        {
            double bodyRadius = previewedOrbit.orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
            if((previewedOrbit.param.rp - bodyRadius) > 0d)
            {
                ShowPinpoint(perihelionPinpoint);
                perihelionPinpoint.transform.position = orbitedBody.gameObject.transform.position + previewedOrbit.lineRenderer.GetPosition(0);
            }
            else {
                HidePinpoint(perihelionPinpoint);
            }

            if((previewedOrbit.param.ra - bodyRadius) > 0d)
            {
                ShowPinpoint(aphelionPinpoint);
                aphelionPinpoint.transform.position = orbitedBody.gameObject.transform.position + previewedOrbit.lineRenderer.GetPosition((int)(previewedOrbit.lineRenderer.positionCount/2));
            }
            else {
                HidePinpoint(aphelionPinpoint);
            }
        }
        else {
            // Hiding the pinpoints
            HidePinpoint(perihelionPinpoint);
            HidePinpoint(aphelionPinpoint);
        }
    }

    private void ShowPinpoint(RectTransform pinPoint)
    {
        pinPoint.gameObject.SetActive(true);
    }

    private void HidePinpoint(RectTransform pinPoint)
    {
        pinPoint.gameObject.SetActive(false);
    }

    public bool InputFieldsAreAllEmpty()
    {
        if(orbitShape_p1_field.text != "") {
            return false;
        }
        if(orbitShape_p2_field.text != "") {
            return false;
        }
        if(inclination_field.text != "") {
            return false;
        }
        if(lAscN_field.text != "") {
            return false;
        }
        if(periapsisArg_field.text != "") {
            return false;
        }
        if(bodyPos_field.text != "") {
            return false;
        }
        return true;
    }

    private void CheckForEmptyFields()
    {
        // Inserting a 0 as the default value for each empty field when the UpdateOrbit button is pressed

        if(orbitShape_p1_field.text.Equals("")) {
            orbitShape_p1_field.text = "0";
        }
        if(orbitShape_p2_field.text.Equals("")) {
            orbitShape_p2_field.text = "0";
        }
        if(inclination_field.text.Equals("")) {
            inclination_field.text = "0";
        }
        if(lAscN_field.text.Equals("")) {
            lAscN_field.text = "0";
        }
        if(periapsisArg_field.text.Equals("")) {
            periapsisArg_field.text = "0";
        }
        if(bodyPos_field.text.Equals("")) {
            bodyPos_field.text = "0";
        }
    }

    private OrbitalParams Create_OrbitalParams_File()
    {
        //OrbitalParams orbitalParams = (OrbitalParams)OrbitalParams.CreateInstance("OrbitalParams");
        OrbitalParams orbitalParams = new OrbitalParams();
        orbitalParams.drawOrbit = true;
        orbitalParams.drawDirections = false;
        orbitalParams.orbitDrawingResolution = 500;
        orbitalParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.realOrbit;

        orbitalParams.orbitDefType = UsefulFunctions.String2_orbitDefinitionTypeEnum(orbitDefType.options[orbitDefType.value].text);
        orbitalParams.orbParamsUnits = UsefulFunctions.String2_OrbitalParamsUnitsEnum(unitsDropdown.options[unitsDropdown.value].text);

        double scaleFactor = 1d; // Default for 'km_degree' units
        if(unitsDropdown.value == 1) {
            scaleFactor = UniCsts.km2au;
        }
        switch(orbitDefType.value)
        {
            case 0:
                // rarp
                UsefulFunctions.ParseStringToDouble(orbitShape_p1_field.text, out orbitalParams.ra);
                UsefulFunctions.ParseStringToDouble(orbitShape_p2_field.text, out orbitalParams.rp);

                // Converting entered altitude to a proper distance with respect to the planet centre
                orbitalParams.ra += scaleFactor*orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
                orbitalParams.rp += scaleFactor*orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
                if(orbitalParams.ra < orbitalParams.rp || orbitalParams.ra < Mathd.Epsilon) {
                    ORBITAL_PARAMS_VALID = false;
                }
                break;
            case 1:
                // rpe
                UsefulFunctions.ParseStringToDouble(orbitShape_p1_field.text, out orbitalParams.rp);
                orbitalParams.rp += scaleFactor*orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
                UsefulFunctions.ParseStringToDouble(orbitShape_p2_field.text, out orbitalParams.e);
                if(orbitalParams.e < 0d) {
                    ORBITAL_PARAMS_VALID = false;
                }

                break;
            case 2:
                // pe
                UsefulFunctions.ParseStringToDouble(orbitShape_p1_field.text, out orbitalParams.p);
                UsefulFunctions.ParseStringToDouble(orbitShape_p2_field.text, out orbitalParams.e);
                if(orbitalParams.e < 0d) {
                    ORBITAL_PARAMS_VALID = false;
                }
                break;
        }

        UsefulFunctions.ParseStringToDouble(inclination_field.text, out orbitalParams.i);
        UsefulFunctions.ParseStringToDouble(lAscN_field.text, out orbitalParams.lAscN);
        UsefulFunctions.ParseStringToDouble(periapsisArg_field.text, out orbitalParams.omega);

        switch(bodyPosType.value)
        {
            case 0:
                // nu, true anomaly
                UsefulFunctions.ParseStringToDouble(bodyPos_field.text, out orbitalParams.nu);
                break;
            case 1:
                // m0, mean anomaly
                UsefulFunctions.ParseStringToDouble(bodyPos_field.text, out orbitalParams.m0);
                break;
            case 2:
                // L0, mean longitude
                UsefulFunctions.ParseStringToDouble(bodyPos_field.text, out orbitalParams.l0);
                break;
            case 3:
                // T0, time of passage at the perihelion
                UsefulFunctions.ParseStringToDouble(bodyPos_field.text, out orbitalParams.t0);
                break;
        }

        return orbitalParams;
    }

    private void ClearOrbitInfoValues()
    {
        info_orbitShape_p1_txt.text = "";
        info_orbitShape_p1_unit.text = "";
        info_orbitShape_p1_val.text = "";

        info_orbitShape_p2_txt.text = "";
        info_orbitShape_p2_unit.text = "";
        info_orbitShape_p2_val.text = "";

        info_orbit_a_unit.text = "";
        info_orbit_a_val.text = "";

        info_orbit_b_unit.text = "";
        info_orbit_b_val.text = "";

        info_orbit_c_unit.text = "";
        info_orbit_c_val.text = "";

        info_bodyPos_p1_txt.text = "";
        info_bodyPos_p1_unit.text = "";
        info_bodyPos_p1_val.text = "";

        info_bodyPos_p2_txt.text = "";
        info_bodyPos_p2_unit.text = "";
        info_bodyPos_p2_val.text = "";

        info_bodyPos_p3_txt.text = "";
        info_bodyPos_p3_unit.text = "";
        info_bodyPos_p3_val.text = "";

        info_meanMotion_val.text = "";
        info_orbitalPeriod_val.text = "";
    }

    private void UpdateOrbitInfoValues()
    {
        switch(orbitDefType.value)
        {
            case 0:
                // rarp, thus displaying p and e
                info_orbitShape_p1_txt.text = "p";
                info_orbitShape_p1_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.p, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_orbitShape_p2_txt.text = "e";
                info_orbitShape_p2_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.e, UniCsts.UI_SIGNIFICANT_DIGITS);
                break;
            case 1:
                // rpe, thus displaying ra and p
                info_orbitShape_p1_txt.text = "ra";
                info_orbitShape_p1_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.ra, UniCsts.UI_SIGNIFICANT_DIGITS);;
                info_orbitShape_p2_txt.text = "p";
                info_orbitShape_p2_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.p, UniCsts.UI_SIGNIFICANT_DIGITS);
                break;
            case 2:
                // pe, thus displaying ra and rp
                info_orbitShape_p1_txt.text = "ra";
                info_orbitShape_p1_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.ra, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_orbitShape_p2_txt.text = "rp";
                info_orbitShape_p2_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.rp, UniCsts.UI_SIGNIFICANT_DIGITS);
                break;
        }
        
        info_orbit_a_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.a, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_orbit_b_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.b, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_orbit_c_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.c, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_meanMotion_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.n*180d/Mathd.PI, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_orbitalPeriod_val.text = UsefulFunctions.DoubleToSignificantDigits(previewedOrbit.param.period, UniCsts.UI_SIGNIFICANT_DIGITS);
    }

    private void UpdateOrbitInfoUnits()
    {
        string distanceUnit = "";
        switch(unitsDropdown.value)
        {
            case 0:
                // 'km_degree' selected
                distanceUnit = " km";
                break;
            case 1:
                // 'AU_degree' selected
                distanceUnit = " AU";
                break;
        }

        switch(orbitDefType.value)
        {
            case 0:
                // 'rarp'
                info_orbitShape_p1_unit.text = distanceUnit;
                info_orbitShape_p2_unit.text = "";
                break;
            case 1:
                // 'rpe'
                info_orbitShape_p1_unit.text = distanceUnit;
                info_orbitShape_p2_unit.text = distanceUnit;
                break;
            case 2:
                // 'pe'
                info_orbitShape_p1_unit.text = distanceUnit;
                info_orbitShape_p2_unit.text = distanceUnit;
                break;
        }
        info_orbit_a_unit.text = distanceUnit;
        info_orbit_b_unit.text = distanceUnit;
        info_orbit_c_unit.text = distanceUnit;
    }

}