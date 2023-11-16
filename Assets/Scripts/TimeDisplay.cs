using System.Runtime.InteropServices;
using System;
using TMPro;
using UnityEngine;

public class TimeDisplay : MonoBehaviour
{
    public bool isStopwatchActive;
    public float currentTime;
    public TextMeshProUGUI currrentTimeText;

    public HandDraw handDraw;

    [SerializeField] private int PortAddress, Data;

    [DllImport("inpoutx64.dll")]
    private static extern void Out32(int PortAddress, int Data);

    [DllImport("inpoutx64.dll")]
    private static extern UInt32 IsInpOutDriverOpen();

    // Start is called before the first frame update
    public void Start()
    {
        isStopwatchActive = false;
        currentTime = 0;
        handDraw = FindAnyObjectByType<HandDraw>();
    }

    // Update is called once per frame
    public void Update()
    {
        UpdateTime();
        StopTime();
    }

    public void UpdateTime()
    {
        if (handDraw.isTouching && !isStopwatchActive)
        {
            isStopwatchActive = true;
            Out32(20220, 1);
            print("Port Açýk:" + "S" + Data);
            print("Süre Baþladý.");
        }

        if (isStopwatchActive)
        {
            currentTime += Time.deltaTime;
            Out32(20220, 0);
            TimeSpan time = TimeSpan.FromSeconds(currentTime);
            currrentTimeText.text = "Süre: " + time.ToString(@"mm\:ss\:fff");
        }
    }

    public void StopTime()
    {
        if (!handDraw.isTouching && currentTime > 0.1f)
        {
            isStopwatchActive = false;
        }
    }
}