using UnityEngine;
using System;
using TMPro;
using System.Runtime.InteropServices;
using System.Collections;

public class TimeDisplay : MonoBehaviour
{
    public bool isStopwatchActive;
    public float currentTime;
    public TextMeshProUGUI currrentTimeText;
    public GameObject Enable;
    public GameObject Disable;

    private int PortAddress;
    private int Data;

    [DllImport("inpoutx64.dll")]
    private static extern void Out32(int PortAddress, int Data);

    [DllImport("inpoutx64.dll")]
    private static extern UInt32 IsInpOutDriverOpen();

    // Start is called before the first frame update
    public void Start()
    {
        isStopwatchActive = false;
        currentTime = 0;

        Enable = GameObject.Find("EnableStopwatch");
        Disable = GameObject.Find("DisableStopwatch");

        StartCoroutine(TimeDelay());
    }
   IEnumerator TimeDelay()
    {
        Disable.SetActive(false);
        yield return new WaitUntil(() => currentTime >= 3);
        Disable.SetActive(true);
        yield return new WaitUntil(() => isStopwatchActive == false);
        Disable.SetActive(false);
    }
   

    // Update is called once per frame
    public void Update()
    {
       
        if (isStopwatchActive == true)
        {
            currentTime += Time.deltaTime;
            Out32(20220, 0);
        }

        TimeSpan time = TimeSpan.FromSeconds(currentTime);
        currrentTimeText.text = "Süre: " + time.ToString(@"mm\:ss\:fff");

    }
}
