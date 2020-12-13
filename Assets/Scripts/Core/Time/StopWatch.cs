using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mathd_Lib;

public class StopWatch : MonoBehaviour
{
    private TMPro.TMP_Text ui_text;

    double _currentTime;
    public double currentTime
    {
        get {
            return _currentTime;
        }
    }
    
    TimeSpan _timeDuration;
    public TimeSpan timeDuration
    {
        get {
            return _timeDuration;
        }
    }
    
    bool isActive;
    //========================
    //========================
    //========================
    void OnEnable()
    {
        _currentTime = 0d; // in s
        isActive = false;
    }

    void Start()
    {
        _timeDuration = new TimeSpan(0, 0, 0);
        if(gameObject.GetComponent<TMPro.TMP_Text>() != null)
        {
            ui_text = gameObject.GetComponent<TMPro.TMP_Text>();
            ui_text.text = GetCurrentTimeString();
        }
        else {
            ui_text = null;
        }
    }

    void FixedUpdate()
    {
        if(!isActive)
            return;
        _currentTime += Time.fixedDeltaTime; // in s
        string timeString = CurrentStringTime();

        if(ui_text != null)
            ui_text.text = timeString;
    }

    private string CurrentStringTime()
    {
        double leftover = 0d;

        int day = (int)Mathd.Floor(_currentTime / 86400d);
        leftover = _currentTime % 86400d; // in s
        int hour = (int)Mathd.Floor(leftover / 3600d);
        leftover %= 3600d;
        int minute = (int)Mathd.Floor(leftover / 60d);
        leftover %= 60d;
        int second = (int)Mathd.Floor(leftover);
        if(second != 0)
            leftover %= second;
        int milli = (int)(leftover * 100d);

        _timeDuration = new TimeSpan(day, hour, minute, second, milli);
        return GetCurrentTimeString();
    }
    //================================
    //================================
    //================================
    public void ResetStopwatch()
    {
        isActive = false;
        _currentTime = 0d;
        _timeDuration = new TimeSpan(0, 0, 0);
        if(ui_text != null) {
            ui_text.text = GetCurrentTimeString();
        }
    }

    public string GetCurrentTimeString()
    {
        return _timeDuration.ToString("g");
    }

    public void Start_Stopwatch()
    {
        isActive = true;
    }

    public void Stop_Stopwatch()
    {
        isActive = false;
    }
}