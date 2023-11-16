using System.Runtime.InteropServices;
using System;
using UnityEngine;

public class EnableTime : MonoBehaviour
{
    public TimeDisplay timeDisplay;

    private int PortAddress;
    private int Data;

    [DllImport("inpoutx64.dll")]
    private static extern void Out32(int PortAddress, int Data);

    [DllImport("inpoutx64.dll")]
    private static extern UInt32 IsInpOutDriverOpen();

    // When we enter on a collider
   
    public void OnTriggerEnter(Collider enabler)
    {
        if (enabler.gameObject.layer == 9 && !timeDisplay.isStopwatchActive == true)
        {
            StartTime();
    
        }
    }

    public void Update()
    {
        Out32(20220, 0);
    }

    void StartTime()
    {
        timeDisplay.isStopwatchActive = true;

        Out32(20220, 1);

        print("Port Açýk:" + "S" + Data);
        print("Süre Baþladý.");
    }
}