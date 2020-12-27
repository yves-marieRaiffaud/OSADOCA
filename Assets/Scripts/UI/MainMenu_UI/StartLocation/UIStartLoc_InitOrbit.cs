using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mathd_Lib;
using MathOps = CommonMethods.MathsOps;
using ObjHand = CommonMethods.ObjectsHandling;
using UI_Fncs = CommonMethods.UI_Methods;
using UniCsts = UniverseConstants;
using UnityEngine.Events;

public class UIStartLoc_InitOrbit : MonoBehaviour
{
    [Header("Orbiting spacecraft")]
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
    public TMPro.TMP_Text info_bodyPos_p4_txt;
    public TMPro.TMP_Text info_bodyPos_p4_val;
    public TMPro.TMP_Text info_bodyPos_p4_unit;

    public TMPro.TMP_Text info_meanMotion_val;
    public TMPro.TMP_Text info_orbitalPeriod_val;
    //==================================================================
    [Header("Orbit Preview UI Elements")]
    public RectTransform perihelionPinpoint;
    public RectTransform aphelionPinpoint;
    //==================================================================
    private bool ORBITAL_PARAMS_VALID = true;
    public Orbit previewedOrbit;
    public StartLocPanelIsSetUp panelIsFullySetUp;

    void OnEnable()
    {
        InitDropdowns();

        orbitDefType.onValueChanged.AddListener(delegate { OnOrbitDefTypeDropdownValueChanged(); });
        unitsDropdown.onValueChanged.AddListener(delegate { OnUnitsDropdownValueChanged(); });
        bodyPosType.onValueChanged.AddListener(delegate { OnBodyPosDropdownValueChanged(); });

        ClearOrbitInfoValues();
    }
    void InitDropdowns()
    {
        UI_Fncs.SetUpDropdown(unitsDropdown, new List<string>(System.Enum.GetNames(typeof(OrbitalTypes.orbitalParamsUnits))));
        UI_Fncs.SetUpDropdown(orbitDefType, new List<string>(System.Enum.GetNames(typeof(OrbitalTypes.orbitDefinitionType))));
        UI_Fncs.SetUpDropdown(bodyPosType, new List<string>(System.Enum.GetNames(typeof(OrbitalTypes.bodyPositionType))));
        // Calling the 'on-values changed' functions to init the fields
        OnOrbitDefTypeDropdownValueChanged();
        OnUnitsDropdownValueChanged();
        OnBodyPosDropdownValueChanged();
    }
    void OnOrbitDefTypeDropdownValueChanged()
    {
        // Updating text depending on the 'orbitDefinitionType'
        switch(orbitDefType.value) {
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
    void OnUnitsDropdownValueChanged()
    {
        // Updating placeholders of the input fields depending on the selected 'orbitalParamUnits' enum
        string p2Dimension;
        switch(unitsDropdown.value) {
            case 0:
                // 'km' enum
                // p1 is a distance in every case: either 'ra', 'rp', or 'p'
                orbitShape_p1_field.placeholder.GetComponent<TMPro.TMP_Text>().text = surnameOrbitShape_p1 + " in km";
                // p2 can be either 'rp' or 'e'
                p2Dimension = " in km";
                if(surnameOrbitShape_p2.Equals("e"))
                    p2Dimension = " no dimension";
                orbitShape_p2_field.placeholder.GetComponent<TMPro.TMP_Text>().text = surnameOrbitShape_p2 + p2Dimension;
                break;
            
            case 1:
                // 'AU' enum
                // p1 is a distance in every case: either 'ra', 'rp', or 'p'
                orbitShape_p1_field.placeholder.GetComponent<TMPro.TMP_Text>().text = surnameOrbitShape_p1 + " in AU";
                // p2 can be either 'rp' or 'e'
                p2Dimension = " in AU";
                if(surnameOrbitShape_p2.Equals("e"))
                    p2Dimension = " no dimension";
                orbitShape_p2_field.placeholder.GetComponent<TMPro.TMP_Text>().text = surnameOrbitShape_p2 + p2Dimension;
                break;
        }
    }
    void OnBodyPosDropdownValueChanged()
    {
        switch(bodyPosType.value) {
            case 0:
                // nu, true anomaly
                bodyPos_txt.text = "True anomaly";
                bodyPos_field.placeholder.GetComponent<TMPro.TMP_Text>().text = "nu in °";
                break;
            case 1:
                // nu, true anomaly
                bodyPos_txt.text = "Mean anomaly";
                bodyPos_field.placeholder.GetComponent<TMPro.TMP_Text>().text = "M in °";
                break;
            case 2:
                // E, mean eccentricity
                bodyPos_txt.text = "Mean eccentricity";
                bodyPos_field.placeholder.GetComponent<TMPro.TMP_Text>().text = "E in °";
                break;
            case 3:
                // nu, true anomaly
                bodyPos_txt.text = "Mean longitude";
                bodyPos_field.placeholder.GetComponent<TMPro.TMP_Text>().text = "L in °";
                break;
            case 4:
                // nu, true anomaly
                bodyPos_txt.text = "Time of passage";
                bodyPos_field.placeholder.GetComponent<TMPro.TMP_Text>().text = "T in seconds";
                break;
        }
    }

    //------------------------------------
    //------------------------------------
    void Start()
    {
        if(panelIsFullySetUp == null)
            panelIsFullySetUp = new StartLocPanelIsSetUp();
    }
    //------------------------------------
    //------------------------------------
    // METHODS CALLED FROM THE 'UIStartLoc_Panel' script
    public bool TriggerPanelIsSetBoolEvent()
    {
        CheckForEmptyFields();
        if(panelIsFullySetUp != null)
            panelIsFullySetUp.Invoke(0, System.Convert.ToInt32(ORBITAL_PARAMS_VALID));
        return ORBITAL_PARAMS_VALID;
    }
    public void OnUpdateOrbit_BtnClick()
    {
        ORBITAL_PARAMS_VALID = true;
        CheckForEmptyFields();
        OrbitalParams orbitalParams = Create_OrbitalParams_File();
        if(ORBITAL_PARAMS_VALID)
        {
            if(previewedOrbit != null)
                previewedOrbit = new Orbit(orbitalParams, orbitedBody, orbitingSpacecraft, previewedOrbit.lineRenderer);
            else
                previewedOrbit = new Orbit(orbitalParams, orbitedBody, orbitingSpacecraft);

            UpdateSpacecraftPosition();
            UpdatePinpointsPosition();
            UpdateOrbitInfoValues();
            UpdateOrbitInfoUnits();

            // Sending 0 as the 'inOrbitInit' identifier and 1 as the panel IS set-up
            if(panelIsFullySetUp != null)
                panelIsFullySetUp.Invoke(0, 1);
        }
        else {
            Debug.LogWarning("ORBITAL_PARAMS_VALID has been set to false. Check the entered orbital parameters.");
            // Sending 0 as the 'inOrbitInit' identifier and 0 as the panel IS NOT set-up
            if(panelIsFullySetUp != null)
                panelIsFullySetUp.Invoke(0, 0);
        }
    }
    void CheckForEmptyFields()
    {
        // Inserting a 0 as the default value for each empty field when the UpdateOrbit button is pressed
        float tmpRes=0f;
        MathOps.ParseStringToFloat(orbitShape_p1_field.text,out tmpRes);
        float aphelion = tmpRes;
        if(orbitShape_p1_field.text.Equals("") || MathOps.FloatsAreEqual(tmpRes, 0f, 0.1f))
            // Cannot have an aphelion value of 0
            ORBITAL_PARAMS_VALID = false;

        MathOps.ParseStringToFloat(orbitShape_p2_field.text,out tmpRes);
        if(orbitShape_p2_field.text.Equals("") || MathOps.FloatsAreEqual(tmpRes, 0f, 0.1f))
            // Cannot have a perihelion value of 0
            ORBITAL_PARAMS_VALID = false;

        /*if(aphelion-tmpRes < Mathf.Epsilon)
            ORBITAL_PARAMS_VALID = false;*/

        // Allowing to have not specified the other fields (i, lAscN, omega & bodyPosition): they are thus all set to 0
        if(inclination_field.text.Equals(""))
            inclination_field.text = "0";
        if(lAscN_field.text.Equals(""))
            lAscN_field.text = "0";
        if(periapsisArg_field.text.Equals(""))
            periapsisArg_field.text = "0";
        if(bodyPos_field.text.Equals(""))
            bodyPos_field.text = "0";
    }
    public bool InputFieldsAreAllEmpty()
    {
        if(orbitShape_p1_field.text != "")
            return false;
        if(orbitShape_p2_field.text != "")
            return false;
        if(inclination_field.text != "")
            return false;
        if(lAscN_field.text != "")
            return false;
        if(periapsisArg_field.text != "")
            return false;
        if(bodyPos_field.text != "")
            return false;
        return true;
    }
    //------------------------------------
    //------------------------------------
    void UpdateSpacecraftPosition()
    {
        Vector3d shipWorldPos = Orbit.GetWorldPositionFromOrbit(previewedOrbit);
        shipWorldPos /= 2d*previewedOrbit.orbitedBody.settings.planetaryParams.radius.val;
        shipWorldPos *= previewedOrbit.orbitedBody.transform.localScale.x;
        orbitingSpacecraft.transform.position = (Vector3) shipWorldPos + orbitedBody.gameObject.transform.position;
    }
    void UpdatePinpointsPosition()
    {
        // Positioning the perihelion and aphelion pinpoints in world space
        // Can only do so IF the excentricity e > 0 (if e=0, there is no aphelion nor perihelion as orbit is circular)
        if(previewedOrbit.param.e > 0d)
        {
            double bodyRadius = previewedOrbit.orbitedBody.settings.planetaryParams.radius.val;
            if((previewedOrbit.param.rp - bodyRadius) > 0d) {
                ShowPinpoint(perihelionPinpoint);
                Vector3 uiPos = previewedOrbit.lineRenderer.points3[0];
                uiPos *= previewedOrbit.orbitedBody.transform.localScale.x;
                perihelionPinpoint.transform.position = orbitedBody.gameObject.transform.position + uiPos;
            }
            else
                HidePinpoint(perihelionPinpoint);

            if((previewedOrbit.param.ra - bodyRadius) > 0d) {
                ShowPinpoint(aphelionPinpoint);
                Vector3 uiPos = previewedOrbit.lineRenderer.points3[(int)(previewedOrbit.lineRenderer.points3.Count/2)];
                uiPos *= previewedOrbit.orbitedBody.transform.localScale.x;
                aphelionPinpoint.transform.position = orbitedBody.gameObject.transform.position + uiPos;
            }
            else
                HidePinpoint(aphelionPinpoint);
        }
        else {
            // Hiding the pinpoints
            HidePinpoint(perihelionPinpoint);
            HidePinpoint(aphelionPinpoint);
        }
    }
    void ShowPinpoint(RectTransform pinPoint) {
        pinPoint.gameObject.SetActive(true);
    }
    void HidePinpoint(RectTransform pinPoint) {
        pinPoint.gameObject.SetActive(false);
    }
    //------------------------------------
    //------------------------------------
    OrbitalParams Create_OrbitalParams_File()
    {
        OrbitalParams orbitalParams = ScriptableObject.CreateInstance<OrbitalParams>();
        orbitalParams.drawOrbit = true;
        orbitalParams.drawDirections = false;
        orbitalParams.orbitDrawingResolution = 500;
        orbitalParams.orbitRealPredType = OrbitalTypes.typeOfOrbit.realOrbit;

        orbitalParams.orbitDefType = ObjHand.Str_2_OrbitDefinitionType(orbitDefType.options[orbitDefType.value].text);
        orbitalParams.orbParamsUnits = ObjHand.Str_2_OrbitalParamsUnit(unitsDropdown.options[unitsDropdown.value].text);

        double scaleFactor = 1d; // Default for 'km_degree' units
        if(unitsDropdown.value == 1)
            scaleFactor = Units.KM2AU;
        switch(orbitDefType.value)
        {
            case 0:
                // rarp
                MathOps.ParseStringToDouble(orbitShape_p1_field.text, out orbitalParams.ra);
                MathOps.ParseStringToDouble(orbitShape_p2_field.text, out orbitalParams.rp);

                // Converting entered altitude to a proper distance with respect to the planet centre
                orbitalParams.ra += scaleFactor*orbitedBody.settings.planetaryParams.radius.val;
                orbitalParams.rp += scaleFactor*orbitedBody.settings.planetaryParams.radius.val;
                if(orbitalParams.ra < Mathd.Epsilon)
                    ORBITAL_PARAMS_VALID = false;
                break;
            case 1:
                // rpe
                MathOps.ParseStringToDouble(orbitShape_p1_field.text, out orbitalParams.rp);
                orbitalParams.rp += scaleFactor*orbitedBody.settings.planetaryParams.radius.val;
                MathOps.ParseStringToDouble(orbitShape_p2_field.text, out orbitalParams.e);
                if(orbitalParams.e < 0d)
                    ORBITAL_PARAMS_VALID = false;

                break;
            case 2:
                // pe
                MathOps.ParseStringToDouble(orbitShape_p1_field.text, out orbitalParams.p);
                MathOps.ParseStringToDouble(orbitShape_p2_field.text, out orbitalParams.e);
                if(orbitalParams.e < 0d)
                    ORBITAL_PARAMS_VALID = false;
                break;
        }

        MathOps.ParseStringToDouble(inclination_field.text, out orbitalParams.i);
        orbitalParams.i *= UniCsts.deg2rad;
        MathOps.ParseStringToDouble(lAscN_field.text, out orbitalParams.lAscN);
        orbitalParams.lAscN *= UniCsts.deg2rad;
        MathOps.ParseStringToDouble(periapsisArg_field.text, out orbitalParams.omega);
        orbitalParams.omega *= UniCsts.deg2rad;

        orbitalParams.bodyPosType = ObjHand.Str_2_BodyPosType(bodyPosType.options[bodyPosType.value].text);

        switch(bodyPosType.value)
        {
            case 0:
                // nu, true anomaly
                MathOps.ParseStringToDouble(bodyPos_field.text, out orbitalParams.nu);
                orbitalParams.nu *= UniCsts.deg2rad;
                break;
            case 1:
                // M, mean anomaly
                MathOps.ParseStringToDouble(bodyPos_field.text, out orbitalParams.M);
                orbitalParams.M *= UniCsts.deg2rad;
                break;
            case 2:
                // E, eccentric anomaly
                MathOps.ParseStringToDouble(bodyPos_field.text, out orbitalParams.E);
                orbitalParams.E *= UniCsts.deg2rad;
                break;
            case 3:
                // L, mean longitude
                MathOps.ParseStringToDouble(bodyPos_field.text, out orbitalParams.L);
                orbitalParams.L *= UniCsts.deg2rad;
                break;
            case 4:
                // T, time of passage at the perihelion
                MathOps.ParseStringToDouble(bodyPos_field.text, out orbitalParams.t);
                break;
        }
        return orbitalParams;
    }
    void UpdateOrbitInfoValues()
    {
        // Updating the OrbitShape fields
        switch(orbitDefType.value)
        {
            case 0:
                // rarp, thus displaying p and e
                info_orbitShape_p1_txt.text = "p";
                info_orbitShape_p1_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.p, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_orbitShape_p2_txt.text = "e";
                info_orbitShape_p2_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.e, UniCsts.UI_SIGNIFICANT_DIGITS);
                break;
            case 1:
                // rpe, thus displaying ra and p
                info_orbitShape_p1_txt.text = "ra";
                info_orbitShape_p1_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.ra, UniCsts.UI_SIGNIFICANT_DIGITS);;
                info_orbitShape_p2_txt.text = "p";
                info_orbitShape_p2_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.p, UniCsts.UI_SIGNIFICANT_DIGITS);
                break;
            case 2:
                // pe, thus displaying ra and rp
                info_orbitShape_p1_txt.text = "ra";
                info_orbitShape_p1_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.ra, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_orbitShape_p2_txt.text = "rp";
                info_orbitShape_p2_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.rp, UniCsts.UI_SIGNIFICANT_DIGITS);
                break;
        }
        // Updating the OrbitShape 'a'; 'b' & 'c' fields
        info_orbit_a_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.a, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_orbit_b_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.b, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_orbit_c_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.c, UniCsts.UI_SIGNIFICANT_DIGITS);
        // Updating the body position type fields
        switch(bodyPosType.value)
        {
            case 0:
                // 'nu' == true anomaly, thus displaying info M;E;L;t
                info_bodyPos_p1_txt.text = "M";
                info_bodyPos_p1_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.M, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p1_unit.text = "deg";

                info_bodyPos_p2_txt.text = "E";
                info_bodyPos_p2_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.E, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p2_unit.text = "deg";

                info_bodyPos_p3_txt.text = "L";
                info_bodyPos_p3_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.L, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p3_unit.text = "deg";

                info_bodyPos_p4_txt.text = "t";
                info_bodyPos_p4_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.t, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p4_unit.text = "s";
                break;
            case 1:
                // 'M' == mean anomaly, thus displaying info nu;E;L;t
                info_bodyPos_p1_txt.text = "nu";
                info_bodyPos_p1_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.nu, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p1_unit.text = "deg";

                info_bodyPos_p2_txt.text = "E";
                info_bodyPos_p2_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.E, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p2_unit.text = "deg";

                info_bodyPos_p3_txt.text = "L";
                info_bodyPos_p3_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.L, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p3_unit.text = "deg";

                info_bodyPos_p4_txt.text = "t";
                info_bodyPos_p4_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.t, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p4_unit.text = "s";
                break;
            case 2:
                // 'E' == eccentric anomaly, thus displaying info nu;M;L;t
                info_bodyPos_p1_txt.text = "nu";
                info_bodyPos_p1_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.nu, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p1_unit.text = "deg";

                info_bodyPos_p2_txt.text = "M";
                info_bodyPos_p2_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.M, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p2_unit.text = "deg";

                info_bodyPos_p3_txt.text = "L";
                info_bodyPos_p3_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.L, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p3_unit.text = "deg";

                info_bodyPos_p4_txt.text = "t";
                info_bodyPos_p4_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.t, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p4_unit.text = "s";
                break;
            case 3:
                // 'L' == mean longitude, thus displaying info nu;M;E;t
                info_bodyPos_p1_txt.text = "nu";
                info_bodyPos_p1_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.nu, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p1_unit.text = "deg";

                info_bodyPos_p2_txt.text = "M";
                info_bodyPos_p2_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.M, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p2_unit.text = "deg";

                info_bodyPos_p3_txt.text = "E";
                info_bodyPos_p3_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.E, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p3_unit.text = "deg";

                info_bodyPos_p4_txt.text = "t";
                info_bodyPos_p4_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.t, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p4_unit.text = "s";
                break;
            case 4:
                // 't' == time of passage at perigee, thus displaying info nu;M;E;L
                info_bodyPos_p1_txt.text = "nu";
                info_bodyPos_p1_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.nu, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p1_unit.text = "deg";

                info_bodyPos_p2_txt.text = "M";
                info_bodyPos_p2_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.M, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p2_unit.text = "deg";

                info_bodyPos_p3_txt.text = "E";
                info_bodyPos_p3_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.E, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p3_unit.text = "deg";

                info_bodyPos_p4_txt.text = "L";
                info_bodyPos_p4_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.L, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_bodyPos_p4_unit.text = "deg";
                break;
        }
        // Updating the last two remaining fields: mean motion & orbital period
        info_meanMotion_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.n*UniCsts.rad2deg, UniCsts.UI_SIGNIFICANT_DIGITS); // to °/s
        info_orbitalPeriod_val.text = MathOps.DoubleToSignificantDigits(previewedOrbit.param.period, UniCsts.UI_SIGNIFICANT_DIGITS);
    }
    void UpdateOrbitInfoUnits()
    {
        // Updating the unit for each field in the 'ABOUT THE ORBIT' panel
        string distanceUnit = "";
        switch(unitsDropdown.value) {
            case 0:
                // 'km_degree' selected
                distanceUnit = " km";
                break;
            case 1:
                // 'AU_degree' selected
                distanceUnit = " AU";
                break;
        }

        switch(orbitDefType.value) {
            case 0:
                // 'rarp'
                info_orbitShape_p1_unit.text = distanceUnit;
                info_orbitShape_p2_unit.text = "-";
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
    void ClearOrbitInfoValues()
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

        info_bodyPos_p4_txt.text = "";
        info_bodyPos_p4_unit.text = "";
        info_bodyPos_p4_val.text = "";

        info_meanMotion_val.text = "";
        info_orbitalPeriod_val.text = "";
    }
}