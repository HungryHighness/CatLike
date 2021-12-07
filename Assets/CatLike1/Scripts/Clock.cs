using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    [SerializeField] private Transform hoursPivot;
    [SerializeField] private Transform minutesPivot;
    [SerializeField] private Transform secondsPivot;
    private const float HoursToDegrees = -30f, MinutesToDegrees = -6f, SecondsToDegrees = -6f;
    
    private void Update()
    {
        var time = DateTime.Now.TimeOfDay;
        hoursPivot.localRotation = Quaternion.Euler(0, 0, (float)(HoursToDegrees * time.TotalHours));
        minutesPivot.localRotation = Quaternion.Euler(0f, 0f, (float)(MinutesToDegrees * time.TotalMinutes));
        secondsPivot.localRotation = Quaternion.Euler(0f, 0f, (float)(SecondsToDegrees * time.TotalSeconds));
    }
}