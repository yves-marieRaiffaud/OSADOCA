using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mathd_Lib;

public class StopWatch : MonoBehaviour
{
    private TMPro.TMP_Text ui_text;
    private double currentTime { get; set; }
    public TimeSpan timeDuration;
    private bool isActive;
    //========================
    //========================
    //========================
    void OnEnable()
    {
        currentTime = 0d; // in s
        isActive = false;
    }

    void Start()
    {
        timeDuration = new TimeSpan(0, 0, 0);
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
        if(!isActive) { return; }
        currentTime += Time.fixedDeltaTime; // in s
        string timeString = CurrentStringTime();
        
        if(ui_text != null)
        {
            ui_text.text = timeString;
        }
    }

    private string CurrentStringTime()
    {
        double leftover = 0d;

        int day = (int)Mathd.Floor(currentTime / 86400d);
        leftover = currentTime % 86400d; // in s
        int hour = (int)Mathd.Floor(leftover / 3600d);
        leftover %= 3600d;
        int minute = (int)Mathd.Floor(leftover / 60d);
        leftover %= 60d;
        int second = (int)Mathd.Floor(leftover);
        if(second != 0)
            leftover %= second;
        int milli = (int)(leftover * 100d);

        timeDuration = new TimeSpan(day, hour, minute, second, milli);
        return GetCurrentTimeString();
    }
    //================================
    //================================
    //================================
    public void ResetStopwatch()
    {
        isActive = false;
        currentTime = 0d;
        timeDuration = new TimeSpan(0, 0, 0);
        if(ui_text != null) {
            ui_text.text = GetCurrentTimeString();
        }
    }

    public string GetCurrentTimeString()
    {
        return timeDuration.ToString("g");
    }

    public void StartStopwatch()
    {
        isActive = true;
    }

    public void StopStopwatch()
    {
        isActive = false;
    }
}