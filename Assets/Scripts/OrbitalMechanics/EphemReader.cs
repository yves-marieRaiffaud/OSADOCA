using UnityEngine;
using Mathd_Lib;
using UnityEngine.Rendering;
using System;
using System.IO;

[Serializable]
public class EphemReader
{
    string filepath;
    bool fileHasHeader;
    char fileDelimiter;

    double startJD;
    double currJD;
    StreamReader reader;
    int readerRowNb;

    UniverseClock universeClock;

    public EphemReader(UniverseClock _universeClock, string _filepath, bool _ephemFileHasHeader, char _fileDelimiter)
    {
        universeClock = _universeClock;
        filepath = _filepath;
        startJD = currJD = universeClock.julianDate;
        fileHasHeader = _ephemFileHasHeader;
        fileDelimiter = _fileDelimiter;

        OpenEphemFile();
        JD_to_Ephem_StartRow();
    }

    private void OpenEphemFile()
    {
        if(reader == null)
            reader = new StreamReader(filepath);
        else
            Debug.LogWarning("WARNING ! StreamReader 'reader' is already initialized.");
    }

    private void CloseEphemFile()
    {
        if(reader != null) {
            reader.Close();
            reader = null;
        }
        else
            Debug.LogWarning("WARNING! Trying to close the Ephem file that is not open.");
    }

    private void JD_to_Ephem_StartRow()
    {
        if(reader == null)
            throw new System.Exception("You are trying to access the Ephemerides StreamReader while the file is closed.");

        // Finding the row containing the julian date closest to the 'startJD' variable, and while being before this variable
        // Making sure we start at the first line
        reader.DiscardBufferedData();
        reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
        readerRowNb = 0;

        // Skipping first line if the file has a header
        if(fileHasHeader)
            reader.ReadLine();

        // Making sure that the desired startJD is greater than the first julian Date available in the ephem file
        double firstJD = double.Parse(reader.ReadLine().Split(fileDelimiter)[0]);
        readerRowNb++;
        if(firstJD - startJD > float.Epsilon) {
            string errorStr = "The desired start julian date is before the first julian date available in the ephemerides file.";
            Debug.LogError(errorStr);
            throw new System.Exception(errorStr);
        }

        double tmpJD = firstJD;
        while(tmpJD - startJD < float.Epsilon)
        {
            tmpJD = double.Parse(reader.ReadLine().Split(fileDelimiter)[0]);
            readerRowNb++;
        }
        readerRowNb--; // Removing 1 to get the line number just before reaching the startJD, starting from 0

    }
}