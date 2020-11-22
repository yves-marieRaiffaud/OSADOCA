using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CalendarDate
{
    public enum DateFormat {ddMMYYYY, YYYYMMdd, MMddYYYY};

    private int _year;
    public int year
    {
        get {
            return _year;
        }
    }

    private int _month;
    public int month
    {
        get {
            return _month;
        }
    }

    private int _day;
    public int day
    {
        get {
            return _day;
        }
    }

    private int _hour;
    public int hour
    {
        get {
            return _hour;
        }
    }

    private int _minute;
    public int minute
    {
        get {
            return _minute;
        }
    }

    private int _second;
    public int second
    {
        get {
            return _second;
        }
    }

    private float _julianDate;
    public float julianDate
    {
        get {
            return _julianDate;
        }
    }

    //public string dateTimeUnityEditorStr;

    public CalendarDate(int __year, int __month, int __day, int __hour, int __minute, int __second)
    {
        _year = __year;
        _month = __month;
        _day = __day;
        _hour = __hour;
        _minute = __minute;
        _second = __second;
        ComputeJulianNumberDate();
    }

    public CalendarDate(string dateStr, DateFormat format)
    {
        // Searching for a 'space' in the string date & time. If there is no space, searching for a 'T' to determine time
        char splitCharacter = ' ';
        bool hasTimeIncluded = dateStr.Split(' ').Length > 1 ? true : false;
        if(!hasTimeIncluded) {
            hasTimeIncluded = dateStr.Split('T').Length > 1 ? true : false;
            splitCharacter = 'T';
        }

        if(hasTimeIncluded) {
            string[] dateTimeStr = dateStr.Split(splitCharacter);
            string date = dateTimeStr[0];
            string time = dateTimeStr[1];
            Parse_Year_Month_Day(date, format);
            Parse_Time(time);
        }

        else {
            Parse_Year_Month_Day(dateStr, format);
            // Initialize default time at "00:00:00"
            _hour = 0;
            _minute = 0;
            _second = 0;
        }
    }

    private void Parse_Time(string time)
    {
        // "12:05:01" == "HH:mm:ss"
        string[] splitedTime = time.Split(':');
        _hour = int.Parse(splitedTime[0]);
        _minute = int.Parse(splitedTime[1]);
        _second = int.Parse(splitedTime[2]);
    }

    private void Parse_Year_Month_Day(string date, DateFormat format)
    {
        string[] splitedDate = date.Split('-');
        if(splitedDate.Length != 3)
            throw new Exception("Splitted 'dateString' does not have a length of 3. Can't determine day, month and year.");
        switch(format)
        {
            case DateFormat.ddMMYYYY:
                _day = int.Parse(splitedDate[0]);
                _month = int.Parse(splitedDate[1]);
                _year = int.Parse(splitedDate[2]);
                break;
            case DateFormat.MMddYYYY:
                _month = int.Parse(splitedDate[0]);
                _day = int.Parse(splitedDate[1]);
                _year = int.Parse(splitedDate[2]);
                break;
            case DateFormat.YYYYMMdd:
                _year = int.Parse(splitedDate[0]);
                _month = int.Parse(splitedDate[1]);
                _day = int.Parse(splitedDate[2]);
                break;
        }
    }

    private void ComputeJulianNumberDate()
    {
        int jdn = (int)(Mathf.Floor((1461f * (_year+4800f+Mathf.Floor((_month-14f)/12f)))/4f) + Mathf.Floor((367f*(_month-2f-12f*Mathf.Floor(((_month-14f)/12f))))/12f) - Mathf.Floor((3f*((_year+4900f+Mathf.Floor((_month-14f)/12f))/100f))/4f) + _day - 32075f - 1f);
        _julianDate = (float)jdn + (_hour-12f)/24f + _minute/1440f + _second/86400f;
    }

    public int[] GetTime()
    {
        int[] time = new int[3];
        time[0] = _hour;
        time[0] = _minute;
        time[0] = _second;
        return time;
    }

    public void AddTime(int secondsToAdd)
    {
        // hour, minute, seconds
        int hourToAdd = (int)Mathf.Floor((float)secondsToAdd/3600f);
        int minuteToAdd = (int)Mathf.Floor((secondsToAdd - 3600f*hourToAdd)/60f);
        int secToAdd = secondsToAdd - 3600*hourToAdd - 60*minuteToAdd;
        _hour += hourToAdd;
        _minute += minuteToAdd;
        _second += secToAdd;
    }

    public override string ToString()
    {
        return string.Format("{0}-{1}-{2} {3}:{4}:{5}", _day, _month, _year, _hour, _minute, _second);
    }

    public string ToString(DateFormat format)
    {
        switch(format)
        {
            case DateFormat.ddMMYYYY:
                return string.Format("{0}-{1}-{2} {3}:{4}:{5}", _day, _month, _year, _hour, _minute, _second);
            case DateFormat.MMddYYYY:
                return string.Format("{0}-{1}-{2} {3}:{4}:{5}", _month, _day, _year, _hour, _minute, _second);
            case DateFormat.YYYYMMdd:
                return string.Format("{0}-{1}-{2} {3}:{4}:{5}", _year, _month, _day, _hour, _minute, _second);
            default:
                return ToString();
        }
    }

}