using System.Runtime.InteropServices;
using System;
using UnityEngine;

public class DisableTime : MonoBehaviour
{
    public TimeDisplay timeDisplay;

    private int PortAddress;
    private int Data;

    [DllImport("inpoutx64.dll")]
    private static extern void Out32(int PortAddress, int Data);

    [DllImport("inpoutx64.dll")]
    private static extern UInt32 IsInpOutDriverOpen();

    // When we enter on a collider
    public void OnTriggerEnter(Collider disabler)
    {
        // in base of the tag of the collider gameObject
        if (disabler.gameObject.layer == 9 && timeDisplay.isStopwatchActive == true)
        {
            print("Süre Durdu.");
            timeDisplay.isStopwatchActive = false;
            print("Süre:" + timeDisplay.currentTime.ToString());
        }
    }
    public void FixedUpdate()
    {
        Out32(20220, 0);
    }
}