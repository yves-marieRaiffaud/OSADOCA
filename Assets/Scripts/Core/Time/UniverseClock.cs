using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mathd_Lib;

[DisallowMultipleComponent, Serializable]
public class UniverseClock : MonoBehaviour
{
    // Using ISO day of the week == Monday is the first day of the week
    public enum DayOfWeek {Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday};
    private DayOfWeek _currentDayOfWeek;
    public DayOfWeek currentDayOfWeek
    {
        get {
            return _currentDayOfWeek;
        }
    }

    [Tooltip("Separator for the date shall be '-', and ':' for the time.\nEx: '01-01-2030T12:00:00' or '01-01-2030 12:00:00'")]
    public string dateString_unityEditor;

    public enum DateFormat {ddMMYYYY, YYYYMMdd, MMddYYYY};
    public DateFormat dateFormat_unityEditor;

    private DateFormat _format;
    public DateFormat format
    {
        get {
            return _format;
        }
    }

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

    [SerializeField]
    private int _hour;
    public int hour
    {
        get {
            return _hour;
        }
    }

    [SerializeField]
    private int _minute;
    public int minute
    {
        get {
            return _minute;
        }
    }

    [SerializeField]
    private int _second;
    public int second
    {
        get {
            return _second;
        }
    }

    [SerializeField]
    private double _julianDate;
    public double julianDate
    {
        get {
            return _julianDate;
        }
    }

    // Ellapsed time in seconds since the start of the Universe Clock
    private long _ellapsedTime;
    public long ellapsedTime
    {
        get {
            return _ellapsedTime;
        }
    }

    private IEnumerator jdCoroutine;
    private IEnumerator posReaderUpdateCoroutine;

    struct TimerAlarm
    {
        long _alarmEllapsedtime;
        public long alarmEllapsedtime
        {
            get {
                return _alarmEllapsedtime;
            }
        }

        Action _callBackFunction;
        public Action callBackFunction
        {
            get {
                return _callBackFunction;
            }
        }
        public TimerAlarm(long _targetEllapsedTime, Action _callbackFcn) {
            _alarmEllapsedtime = _targetEllapsedTime;
            _callBackFunction = _callbackFcn;
        }
    };
    List<TimerAlarm> timerCallbacks;

    /// <summary>
    /// Initializing the UniverseClock by setting up the specified date in the Unity inspector via the field 'dateString_unityEditor'. Else assigns a default date
    /// </summary>
    public void Init()
    {
        // If the string in the Unity Inpector is not null, we assign the value
        if(dateString_unityEditor != null && dateString_unityEditor.Length >= 10)
            AssignNewDateFromString(dateString_unityEditor, dateFormat_unityEditor);
        else
            // Else we assign a default date and time
            AssignNewDateFromString("18-09-1997 16:00:00", DateFormat.ddMMYYYY);
        timerCallbacks = new List<TimerAlarm>();
    }

    // Launching coroutines
    void Awake()
    {
        Init();
        ComputeJulianNumberDate();
        JD_2_Day_of_Week();
        jdCoroutine = UpdateUniverseTimeCoroutine();
        StartCoroutine(jdCoroutine);
    }

    // Main coroutine incrementing the universe clock
    private IEnumerator UpdateUniverseTimeCoroutine()
    {
        while(true)
        {
            AddTime(1);
            yield return new WaitForSeconds(1f);
        }
    }
    /// <summary>
    /// Called during the main coroutine to increment the clock. Checking if any TimerAlarm has reached its time
    /// </summary>
    public void AddTime(int secondsToAdd)
    {
        _julianDate += secondsToAdd/86400d;
        _ellapsedTime += secondsToAdd;

        // Checking for callbacks at specified timers
        if(timerCallbacks.Count.Equals(0))
            return;
        else if(_ellapsedTime == timerCallbacks[0].alarmEllapsedtime) {
            Action action = timerCallbacks[0].callBackFunction;
            timerCallbacks.RemoveAt(0);
            action.Invoke();
        }

        // Update Hour, Minute & Second properties
        //Get_Time_From_JulianDate();
    }

    /// <summary>
    /// Extracting information from the string date. Updating members of the universeClock class
    /// </summary>
    public void AssignNewDateFromString(string dateStr, DateFormat format)
    {
        _format = format;
        ParseString_2_Date(dateStr);
    }

    private void ParseString_2_Date(string dateStr)
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
            Parse_Year_Month_Day(date);
            Parse_Time(time);
        }

        else {
            Parse_Year_Month_Day(dateStr);
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
    private void Parse_Year_Month_Day(string date)
    {
        string[] splitedDate = date.Split('-');
        if(splitedDate.Length != 3)
            throw new Exception("Splitted 'dateString' does not have a length of 3. Can't determine day, month and year.");
        switch(_format)
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

    /// <summary>
    /// From the 'year', 'month' and 'day' value, computing the corresponding Julian Date Number (JDN).
    /// </summary>
    private void ComputeJulianNumberDate()
    {
        double jdn = (int)(Mathf.Floor((1461f * (_year+4800f+Mathf.Floor((_month-14f)/12f)))/4f) + Mathf.Floor((367f*(_month-2f-12f*Mathf.Floor(((_month-14f)/12f))))/12f) - Mathf.Floor((3f*((_year+4900f+Mathf.Floor((_month-14f)/12f))/100f))/4f) + _day - 32075f - 1f);
        _julianDate = (double)jdn + (_hour-12d)/24d + _minute/1440d + _second/86400d;
    }

    /// <summary>
    /// From the Julian Date Number (the truncated integer of the Julian Date), retrieving the corresponding day of the week. First day of the week is Monday.
    /// </summary>
    private void JD_2_Day_of_Week()
    {
        int jdn = (int)_julianDate; // Retrieving only the Julian Date Number (JDN)
        int w0 = (jdn % 7) + 1;
        // Using ISO day of the week : first day is Monday
        switch(w0)
        {
            case 1:
                _currentDayOfWeek = DayOfWeek.Monday;
                break;
            case 2:
                _currentDayOfWeek = DayOfWeek.Tuesday;
                break;
            case 3:
                _currentDayOfWeek = DayOfWeek.Wednesday;
                break;
            case 4:
                _currentDayOfWeek = DayOfWeek.Thursday;
                break;
            case 5:
                _currentDayOfWeek = DayOfWeek.Friday;
                break;
            case 6:
                _currentDayOfWeek = DayOfWeek.Saturday;
                break;
            case 7:
                _currentDayOfWeek = DayOfWeek.Sunday;
                break;
        }
    }

    /// <summary>
    /// Allow to add a new timerAlarm and to specify a callback (Action) that will be called once the target alarm time is reached 
    /// </summary>
    public void Add_Timer_Callback(long ellapsedTime, Action callbackAction)
    {
        // 'ellapsedTime' is the time in seconds since the beggining of the UniverseClock
        // 'callbackAction' is the action/function to call when the ellapsedTime is reached
        long timeToReach = _ellapsedTime + ellapsedTime;
        int idx = 0;
        foreach(TimerAlarm item in timerCallbacks)
        {
            if(item.alarmEllapsedtime < timeToReach) {
                idx++; continue;
            }
            else {
                timerCallbacks.Insert(idx, new TimerAlarm(ellapsedTime, callbackAction));
                return;
            }
        }
        // Else, we append the TimerAlarm at the end of the list
        timerCallbacks.Add(new TimerAlarm(ellapsedTime, callbackAction));
    }

    /// <summary>
    /// Retrieving the hour, minute and second from the julian date.
    /// </summary>
    private void Get_Time_From_JulianDate()
    {
        double decimalPart = _julianDate % 1;
        double plainHour = decimalPart*24d;
        _hour = 12 + Mathd.FloorToInt(plainHour);
        if(_hour > 23)
            _hour -= 24;
        double plainMinute = plainHour % 1 * 60;
        _minute = Mathd.FloorToInt(plainMinute);
        _second = Mathd.FloorToInt(plainMinute % 1 * 60);
    }
    public int[] GetTime()
    {
        Get_Time_From_JulianDate();
        int[] time = new int[3];
        time[0] = _hour;
        time[1] = _minute;
        time[2] = _second;
        return time;
    }

    /// Date and Time to string
    public string Time_ToString()
    {
        return string.Format("{0}:{1}:{2}", _hour, _minute, _second);
    }
    public string Date_ToString(DateFormat format)
    {
        switch(format)
        {
            case DateFormat.ddMMYYYY:
                return string.Format("{0}-{1}-{2}", _day, _month, _year);
            case DateFormat.MMddYYYY:
                return string.Format("{0}-{1}-{2}", _month, _day, _year);
            case DateFormat.YYYYMMdd:
                return string.Format("{0}-{1}-{2}", _year, _month, _day);
            default:
                return ToString();
        }
    }
    public string Date_ToString()
    {
        return Date_ToString(_format);
    }
    public override string ToString()
    {
        return ToString(_format) + " " + Time_ToString();
    }
    public string ToString(DateFormat format)
    {
        return Date_ToString(format) + " " + Time_ToString();
    }
}