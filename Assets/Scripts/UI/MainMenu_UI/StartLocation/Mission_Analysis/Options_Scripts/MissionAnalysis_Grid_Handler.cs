using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

//[ExecuteAlways]
public class MissionAnalysis_Grid_Handler : MonoBehaviour
{
    public Image _gridImage;
    public TMP_Dropdown _gridDropdown;
    // List of string containing the fileNames of the grids, with their extension
    List<string> gridsNamesArr;

    void Awake()
    {
        if(_gridDropdown == null)
            Debug.LogError("Error while Awaking the MissionAnalysis_GridHandler: '_gridDropdown' is null");

        SetUp_Dropdown_Options();
        _gridDropdown.onValueChanged.AddListener(OnDropdown_Grid_ValueChanged);
        _gridDropdown.value = 1;
        _gridDropdown.value = 0;
    }

    public void SetUp_Dropdown_Options()
    {
        gridsNamesArr = Read_Grid_Manifest_File();
        List<string> optsDropd = new List<string>();
        optsDropd.Add("None"); // Adding None as the first option, index 0

        // Getting rid of the file extension ".png" and replacing the '_' with a space, while keeping the original array for when one item is clicked
        for(int idx=0; idx<gridsNamesArr.Count; idx++) {
            string gridName = gridsNamesArr[idx];
            gridName = Path.GetFileNameWithoutExtension(gridName);
            gridName = gridName.Replace('_', ' ');
            optsDropd.Add(gridName);
        }
        _gridDropdown.ClearOptions();
        _gridDropdown.AddOptions(optsDropd);
    }

    List<string> Read_Grid_Manifest_File()
    {
        string[] gridsNames = Resources.Load<TextAsset>(Filepaths.ma_Grids_ManifestPath).text.Split('\n');
        return new List<string>(gridsNames);
    }

    void OnDropdown_Grid_ValueChanged(int selectedValue)
    {   
        // Check if the 'None' options has been selected
        if(selectedValue == 0) {
            _gridImage.enabled = false;
            _gridImage.sprite = null;
            return;
        }
        
        // Else, loading the corresponding grid, adding 1 to the index to take into account the None option
        Sprite _img = Resources.Load<Sprite>(Filepaths.ma_Grids_FolderPath + Path.GetFileNameWithoutExtension(gridsNamesArr[selectedValue-1]));
        _gridImage.sprite = _img;
        _gridImage.enabled = true;
    }
}