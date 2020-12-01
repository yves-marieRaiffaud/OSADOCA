using UnityEngine;
using Mathd_Lib;
using System;
using System.IO;

[Serializable]
public class EphemReader
{
    string filepath; // filepath of the epheremide file
    bool fileHasHeader; // True if the file has a one-line header (with the units for instance)
    char fileDelimiter; // ";"

    double startJD; // First Julian Date read at line 1 (doc starting at line 0 but containing the header)
    double currJD; // Current Julian Date that has been last read
    StreamReader reader; // StreamReader of the ephemeride file
    int readerRowNb; // Curent line number that is being read

    /// <summary>
    /// Time between two positions in the ephemerides file
    /// </summary>
    public int ephemDeltaTime
    {
        get {
            return _ephemDeltaTime;
        }
    }
    int _ephemDeltaTime;

    UniverseClock universeClock;

    public EphemReader(UniverseClock _universeClock, string _filepath, bool _ephemFileHasHeader, char _fileDelimiter)
    {
        universeClock = _universeClock;
        filepath = _filepath;
        startJD = currJD = universeClock.julianDate;
        fileHasHeader = _ephemFileHasHeader;
        fileDelimiter = _fileDelimiter;

        OpenEphemFile(); // Opening file
        Get_Ephem_DeltaTime(); // Retrieving the current delta time between two lines in the ephem file
        JD_to_Ephem_StartRow(); // Retrieving the julian date of the first line of the ephem file
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

    private void PointerToStart()
    {
        // Making sure we start at the first line
        reader.DiscardBufferedData();
        reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
        readerRowNb = 0;
    }

    private void Get_Ephem_DeltaTime()
    {
        PointerToStart();
        double jd_t1 = double.Parse(reader.ReadLine().Split(fileDelimiter)[0]);
        double jd_t2 = double.Parse(reader.ReadLine().Split(fileDelimiter)[0]);
        _ephemDeltaTime = Mathd.FloorToInt((jd_t2 - jd_t1) * 86400d);
    }

    private void JD_to_Ephem_StartRow()
    {
        // Finding the row containing the julian date closest to the 'startJD' variable, and while being before this variable
        if(reader == null)
            throw new System.Exception("You are trying to access the Ephemerides StreamReader while the file is closed.");

        PointerToStart();

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

    public Vector3d ReadNext_XYZ_Position()
    {
        string[] lineItems = reader.ReadLine().Split(fileDelimiter);
        readerRowNb++; // Incrementing row line number to keep track
        double x = double.Parse(lineItems[1]);
        double y = double.Parse(lineItems[2]);
        double z = double.Parse(lineItems[3]);
        return new Vector3d(x, y, z);
    }
}