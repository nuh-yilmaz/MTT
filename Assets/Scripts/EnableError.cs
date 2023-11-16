using System.Runtime.InteropServices;
using System;
using UnityEngine;

public class EnableErrorCount : MonoBehaviour
{
    public ErrorDisplay errorDisplay;
    public TimeDisplay timeDisplay;

    private int PortAddress;
    private int Data;

    [DllImport("inpoutx64.dll")]
    private static extern void Out32(int PortAddress, int Data);

    [DllImport("inpoutx64.dll")]
    private static extern UInt32 IsInpOutDriverOpen();
    public void OnTriggerEnter(Collider enableError)
    {
        if (!timeDisplay.isStopwatchActive == false)
        {
            if (enableError.gameObject.layer == 9)
            {
                errorDisplay.isErrorCounting = true;
                Out32(20220, 2);
                errorDisplay.errorCount++;
            }
        }
    }

    public void Update()
    {
        Out32(20220, 0);
    }
}