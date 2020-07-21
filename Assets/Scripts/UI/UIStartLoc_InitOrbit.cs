using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mathd_Lib;

public class UIStartLoc_InitOrbit : MonoBehaviour
{
    [Header("Orbit.ing.ed body and Spacecraft")]
    public CelestialBody orbitedBody;
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
    private string surnameOrbitShape_p1;
    private string surnameOrbitShape_p2;
    //==================================================================
    [Header("Orbit Info Panel")]
    public TMPro.TMP_Text info_orbitShape_p1_txt;
    public TMPro.TMP_Text info_orbitShape_p1_val;
    public TMPro.TMP_Text info_orbitShape_p2_txt;
    public TMPro.TMP_Text info_orbitShape_p2_val;

    public TMPro.TMP_Text info_orbit_a_val;
    public TMPro.TMP_Text info_orbit_b_val;
    public TMPro.TMP_Text info_orbit_c_val;

    public TMPro.TMP_Text info_bodyPos_p1_txt;
    public TMPro.TMP_Text info_bodyPos_p1_val;
    public TMPro.TMP_Text info_bodyPos_p2_txt;
    public TMPro.TMP_Text info_bodyPos_p2_val;
    public TMPro.TMP_Text info_bodyPos_p3_txt;
    public TMPro.TMP_Text info_bodyPos_p3_val;

    public TMPro.TMP_Text info_meanMotion_val;
    public TMPro.TMP_Text info_orbitalPeriod_val;
    //==================================================================
    private bool ORBITAL_PARAMS_VALID = true;
    public Orbit previewedOrbit;

    void OnEnable()
    {
        orbitedBody.AwakeCelestialBody(UniCsts.planetsDict[orbitedBody.settings.chosenPredifinedPlanet]);

        InitDropdowns();

        orbitDefType.onValueChanged.AddListener(delegate { OnOrbitDefTypeDropdownValueChanged(); });
        unitsDropdown.onValueChanged.AddListener(delegate { OnUnitsDropdownValueChanged(); });
        bodyPosType.onValueChanged.AddListener(delegate { OnBodyPosDropdownValueChanged(); });
    }

    private void InitDropdowns()
    {
        UsefulFunctions.SetUpDropdown(unitsDropdown, new List<string>(System.Enum.GetNames(typeof(OrbitalParams.orbitalParamsUnits))));
        UsefulFunctions.SetUpDropdown(orbitDefType, new List<string>(System.Enum.GetNames(typeof(OrbitalParams.orbitDefinitionType))));
        UsefulFunctions.SetUpDropdown(bodyPosType, new List<string>(System.Enum.GetNames(typeof(OrbitalParams.bodyPositionType))));

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
        OrbitalParams orbitalParams = ComputeNewOrbit();
        if(ORBITAL_PARAMS_VALID)
        {
            previewedOrbit = new Orbit(orbitalParams, orbitedBody, orbitingSpacecraft);
            UpdateOrbitInfoValues();
        }
        else {
            Debug.LogWarning("ORBITAL_PARAMS_VALID has been set to false. Check the entered orbital parameters.");
        }
    }

    private OrbitalParams ComputeNewOrbit()
    {
        OrbitalParams orbitalParams = (OrbitalParams)OrbitalParams.CreateInstance("OrbitalParams");
        orbitalParams.drawOrbit = true;
        orbitalParams.drawDirections = false;
        orbitalParams.orbitDrawingResolution = 500;
        orbitalParams.orbitRealPredType = OrbitalParams.typeOfOrbit.realOrbit;

        orbitalParams.orbitDefType = UsefulFunctions.String2_orbitDefinitionTypeEnum(orbitDefType.options[orbitDefType.value].text);
        orbitalParams.orbParamsUnits = UsefulFunctions.String2_OrbitalParamsUnitsEnum(unitsDropdown.options[unitsDropdown.value].text);

        switch(orbitDefType.value)
        {
            case 0:
                // rarp
                UsefulFunctions.ParseStringToDouble(orbitShape_p1_field.text, out orbitalParams.ra);
                UsefulFunctions.ParseStringToDouble(orbitShape_p2_field.text, out orbitalParams.rp);
                // Converting entered altitude to a proper distance with respect to the planet centre
                orbitalParams.ra += orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
                orbitalParams.rp += orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
                if(orbitalParams.ra < orbitalParams.rp || orbitalParams.ra < Mathd.Epsilon) {
                    ORBITAL_PARAMS_VALID = false;
                }
                break;
            case 1:
                // rpe
                UsefulFunctions.ParseStringToDouble(orbitShape_p1_field.text, out orbitalParams.rp);
                orbitalParams.rp += orbitedBody.settings.planetBaseParamsDict[CelestialBodyParamsBase.planetaryParams.radius.ToString()];
                UsefulFunctions.ParseStringToDouble(orbitShape_p2_field.text, out orbitalParams.e);
                if(orbitalParams.e - Mathd.Epsilon < Mathd.Epsilon) {
                    ORBITAL_PARAMS_VALID = false;
                }

                break;
            case 2:
                // pe
                UsefulFunctions.ParseStringToDouble(orbitShape_p1_field.text, out orbitalParams.p);
                UsefulFunctions.ParseStringToDouble(orbitShape_p2_field.text, out orbitalParams.e);
                if(orbitalParams.e - Mathd.Epsilon < Mathd.Epsilon) {
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

    private void UpdateOrbitInfoValues()
    {
        switch(orbitDefType.value)
        {
            case 0:
                // rarp, thus displaying p and e
                info_orbitShape_p1_txt.text = "p";
                info_orbitShape_p1_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.p, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_orbitShape_p2_txt.text = "e";
                info_orbitShape_p2_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.e, UniCsts.UI_SIGNIFICANT_DIGITS);
                break;
            case 1:
                // rpe, thus displaying ra and p
                info_orbitShape_p1_txt.text = "ra";
                info_orbitShape_p1_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.ra, UniCsts.UI_SIGNIFICANT_DIGITS);;
                info_orbitShape_p2_txt.text = "p";
                info_orbitShape_p2_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.p, UniCsts.UI_SIGNIFICANT_DIGITS);
                break;
            case 2:
                // pe, thus displaying ra and rp
                info_orbitShape_p1_txt.text = "ra";
                info_orbitShape_p1_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.ra, UniCsts.UI_SIGNIFICANT_DIGITS);
                info_orbitShape_p2_txt.text = "rp";
                info_orbitShape_p2_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.rp, UniCsts.UI_SIGNIFICANT_DIGITS);
                break;
        }
        
        info_orbit_a_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.a, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_orbit_b_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.b, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_orbit_c_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.c, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_meanMotion_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.n, UniCsts.UI_SIGNIFICANT_DIGITS);
        info_orbitalPeriod_val.text = UsefulFunctions.ToSignificantDigits(previewedOrbit.param.period, UniCsts.UI_SIGNIFICANT_DIGITS);
    }

    void OnDisable()
    {
        // Do something
    }
}