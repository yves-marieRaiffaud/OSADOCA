using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

[ExecuteInEditMode]
public class Resources_Manifest_Helper : MonoBehaviour
{
    [Tooltip("Name of the folder to generate a manifest for. Must NOT include the first / after the 'Resources' folder")]
    public string rscFolderToCheck;
    [Tooltip("Either the manifest should include all subdirectories files matching the specified extension.")]
    public bool checkSubDirectories=false;
    [Tooltip("File extension to add to the manifest file")]
    public string fileExtensionsToCheck;
    [Tooltip("Name of the Manifest file to be generated, located in the specified 'rscFolderToCheck'")]
    public string manifestFileName = "manifest.txt";

    public void On_Generate_File_Click()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/Resources/" + rscFolderToCheck);
        SearchOption searchOption = SearchOption.TopDirectoryOnly;
        if(checkSubDirectories)
            searchOption = SearchOption.AllDirectories;

        FileInfo[] infos = dir.GetFiles(fileExtensionsToCheck, searchOption);
        string[] assetsNames = infos.Select(f => f.Name).ToArray();
        string textToWrite = "";
        for(int idx=0; idx<assetsNames.Length; idx++) {
            textToWrite += assetsNames[idx];
            if(idx < assetsNames.Length-1)
                textToWrite += "\n";
        }

        File.WriteAllText(Application.dataPath + "/Resources/" + rscFolderToCheck + "/" + manifestFileName, textToWrite);
        Debug.Log("Filepath = " + Application.dataPath + "/Resources/" + rscFolderToCheck + "/" + manifestFileName);
    }
}