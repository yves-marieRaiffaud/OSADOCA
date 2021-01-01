using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ComOps = CommonMethods.CommunicationOps;
using Communication;
using TMPro;
using MSDropdownNamespace;
using System;
using UnityEngine.Playables;

public class UI_ComPanelHandler : MonoBehaviour
{
    public AnimationClip unfoldClip;
    public AnimationClip foldClip;
    Animator panelAnimator;
    PlayableGraph _playableGraph;

    ToggleSwitch mainToggleSwitch; // Toggle switch to enable/disable the whole connection panel
    TMP_Dropdown protDropdown; // Dropdown for the the selection of the com protocol to use
    TMP_Dropdown dataTypeDropdown; // Dropdown for the the selection of the data type to send/receive

    MSDropdown dataToSend_MSDrop; // Multi-select dropdown for the selection of the data fields to send (only for UDP/TCP SENDERS for now), else will be null

    TMP_InputField ipField;
    TMP_InputField portField;

    Button defaultBtn;
    Button testBtn;

    void Awake()
    {
        Assign_Variables();
        Init_Variables();
    }
    void Assign_Variables()
    {
        panelAnimator = GetComponent<Animator>();

        // The hereunder members must be in every com panels
        mainToggleSwitch = transform.Find("Title_Toggle_Panel/Panel_OverallToggle/Toggle").GetComponent<ToggleSwitch>();
        mainToggleSwitch.onValueChanged.AddListener(delegate {OnToggleSwitch_Clicked();});

        protDropdown = transform.Find("Protocol_Panel/Protocol_Dropdown").GetComponent<TMP_Dropdown>();
        protDropdown.onValueChanged.AddListener(delegate{OnProtocol_ValueChanged();});

        dataTypeDropdown = transform.Find("DataType_Panel/DataType_Dropdown").GetComponent<TMP_Dropdown>();
        protDropdown.onValueChanged.AddListener(delegate{OnDataType_ValueChanged();});

        ipField = transform.Find("IP_Panel/IP_input").GetComponent<TMP_InputField>();
        portField = transform.Find("Port_Panel/Port_input").GetComponent<TMP_InputField>();

        defaultBtn = transform.Find("Btns_Panel/Default_btn").GetComponent<Button>();
        defaultBtn.onClick.AddListener(delegate{OnDefaultBtn_Clicked();});

        testBtn = transform.Find("Btns_Panel/Test_btn").GetComponent<Button>();
        testBtn.onClick.AddListener(delegate{OnTestBtn_Clicked();});

        // The hereunder member is optional and available only for 'UDP Sender' or 'TCP/IP Sender'
        if(transform.Find("DataToSend_Panel/DataToSend_MSDropdown") != null) {
            dataToSend_MSDrop = transform.Find("DataToSend_Panel/DataToSend_MSDropdown").GetComponent<MSDropdown>();
            dataToSend_MSDrop.OnValueChanged.AddListener(delegate{OnDataToSend_ValueChanged();});
        }
    }
    void Init_Variables()
    {
        protDropdown.ClearOptions();
        List<string> protocolOptions = Enum.GetValues(typeof(ComProtocol)).Cast<ComProtocol>().Select(v => v.ToString()).ToList();
        protDropdown.AddOptions(protocolOptions);

        OnProtocol_ValueChanged();
    }

    void OnToggleSwitch_Clicked()
    {
        if(mainToggleSwitch.isOn) {
            // Unfolding panel
            UnfoldPanel();
            for(int idx=0; idx<transform.childCount; idx++) {
                if(transform.GetChild(idx).name.Equals("Title_Toggle_Panel"))
                    continue;
                transform.GetChild(idx).gameObject.SetActive(true);
            }
        }
        else {
            // Folding panel
            FoldPanel();
            for(int idx=0; idx<transform.childCount; idx++) {
                if(transform.GetChild(idx).name.Equals("Title_Toggle_Panel"))
                    continue;
                transform.GetChild(idx).gameObject.SetActive(false);
            }
        }
    }
    void FoldPanel()
    {
        if(panelAnimator != null && foldClip != null)
            AnimationPlayableUtilities.PlayClip(panelAnimator, foldClip, out _playableGraph);
    }
    void UnfoldPanel()
    {
        if(panelAnimator != null && unfoldClip != null)
            AnimationPlayableUtilities.PlayClip(panelAnimator, unfoldClip, out _playableGraph);
    }

    void OnProtocol_ValueChanged()
    {
        dataTypeDropdown.ClearOptions();
        ComProtocol chosenProt = ComOps.Str_2_ComProtocol(protDropdown.options[protDropdown.value].text);
        ComConectionType possibleCos = ComChannel.comEnumsCombinations[chosenProt];
        List<string> possibleCoTypes = Enum.GetValues(typeof(ComConectionType)).Cast<ComConectionType>()
                                        .Where(val => (possibleCos & val) == val)
                                        .Select(v => v.ToString())
                                        .ToList();
        dataTypeDropdown.AddOptions(possibleCoTypes);

        OnDataType_ValueChanged();
    }
    void OnDataType_ValueChanged()
    {
        // SOMETHING HERE
        if(dataToSend_MSDrop != null)
            OnDataToSend_ValueChanged();
    }

    void OnDataToSend_ValueChanged()
    {

    }

    void OnDefaultBtn_Clicked()
    {
        Debug.Log("Clicked on the default button !");
    }
    void OnTestBtn_Clicked()
    {
        Debug.Log("Clicked on the test button !");
    }
}
