using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mathd_Lib;

[DisallowMultipleComponent, Serializable]
public class UniverseClock : MonoBehaviour
{
    public CalendarDate calendarDate;

    void Awake()
    {
        if(calendarDate == null)
            calendarDate = new CalendarDate("18-09-1997 16:00:00", CalendarDate.DateFormat.ddMMYYYY);
        calendarDate.Init();

        StartCoroutine("UpdateUniverseTimeCoroutine");
    }

    private IEnumerator UpdateUniverseTimeCoroutine()
    {
        yield return new WaitForSeconds(1);
        calendarDate.AddTime(1);
    }
}