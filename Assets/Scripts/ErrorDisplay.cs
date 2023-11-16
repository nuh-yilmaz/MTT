using UnityEngine;
using TMPro;

public class ErrorDisplay : MonoBehaviour
{
    public TextMeshProUGUI errorText;
    public int errorCount = 0;
    public bool isErrorCounting = false;
    private readonly TimeDisplay timeDisplay;
    public void StopErrorCounting()
    {
        if (errorCount > 0 && timeDisplay.isStopwatchActive == false)
        {
            isErrorCounting = false;
            print("Hata:" + errorCount.ToString());
        }
    }
    // Update is called once per frame
    public void Update()
    {

        if (isErrorCounting == true)
        {
            errorText.text = "Hata: " + errorCount.ToString();
        }
    }
    
}