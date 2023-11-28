using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

public class ProjectManager : MonoBehaviour
{

    private OVRHand m_hand;
    private OVRSkeleton m_skeleton;
    private Transform indexTip;
    private Transform indexDistal;
    [SerializeField, Range(0f, 0.5f)]
    private float distanceOffset;
    [SerializeField] LayerMask layerMask;
    private LineRenderer lineRenderer;
    private bool isDrawing = false;

    private int errorCount = 0;  // Added errorCount variable
    public Text errorCountText;
    public GameObject SquareIn;  // Reference to SquareIn object
    public GameObject SquareOut; // Reference to SquareOut object
    public GameObject Paper; 

    private float startTime;
    private float totalElapsedTime = 0f;
    public Text elapsedTimeText;

    private int PortAddress;
    private int Data;
    [DllImport("inpoutx64.dll")]
    private static extern void Out32(int PortAddress, int Data);
    [DllImport("inpoutx64.dll")]
    private static extern UInt32 IsInpOutDriverOpen();

    void Start()
    {
        // Create an empty GameObject for LineRenderer
        GameObject lineObject = new("LineRendererObject");
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0; // Start with an empty line
        lineRenderer.startWidth = 0.01f; // Adjust the width of the line
        lineRenderer.endWidth = 0.01f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.Simplify(10);
        lineRenderer.material = new Material(Shader.Find("Standard"))
        {
            color = Color.black // Set the line color to black
        };

        // Assign your parallel port address (LPT3)
        PortAddress = 0x278; // LPT3 address

        // Ensure SquareIn, SquareOut, and Paper are assigned in the Unity Editor
        if (SquareIn == null || SquareOut == null || Paper == null)
        {
            Debug.LogError("Please assign SquareIn, SquareOut, and Paper in the Unity Editor.");
            return;
        }
    }

    public void Awake()
    {
        // Get the scripts that hold information about hand tracking
        m_hand = GetComponent<OVRHand>();
        m_skeleton = GetComponent<OVRSkeleton>();
    }

    void FixedUpdate()
    {
        if (indexTip == null && m_skeleton.IsInitialized)
        {
            Debug.Log("Skeleton initialized");
            indexTip = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_IndexTip].Transform;
            indexDistal = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Index3].Transform;
        }

        // If hands aren't initialized yet, don't execute the rest of the script.
        if (!indexTip) return;

        Vector3 originPoint = indexDistal.position;
        Vector3 targetPoint = indexTip.position;

        Vector3 direction = Vector3.Normalize(targetPoint - originPoint);
        float distance = Vector3.Distance(originPoint, targetPoint);

        // Cast a ray starting from the second index finger joint to the tip of the index finger.
        // Only check for objects that are in the whiteboard layer.
        if (Physics.Raycast(originPoint, direction, out RaycastHit touch, distance + distanceOffset, layerMask) ||
            Physics.Raycast(targetPoint, -direction, out touch, distance + distanceOffset, layerMask))
        {
            if (touch.collider != null)
            {
                if (!isDrawing)
                {
                    // Start drawing when touched
                    isDrawing = true;
                    startTime = Time.time;

                    SendTriggerToParallelPort(1);

                    lineRenderer.positionCount = 1;
                    lineRenderer.SetPosition(0, touch.point);
                }
                else
                {
                    // Update LineRenderer positions when drawing
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, touch.point);
                }
               


                // Display elapsed time in the scene (optional)
                if (elapsedTimeText != null)
                {
                    totalElapsedTime += Time.deltaTime;
                    elapsedTimeText.text = "Elapsed Time: " + totalElapsedTime.ToString("F2") + " seconds";
                }

                if(startTime != 0)
                {
                    // Check if the touch is within the borders of SquareOut
                    if (!IsWithinBorders(touch.point, SquareOut) || IsWithinBorders(touch.point, SquareIn))
                    {
                        // Increment error count
                        errorCount++;
                        SendTriggerToParallelPort(2);

                        // Display error count on UI Text
                        if (errorCountText != null)
                        {
                            errorCountText.text = "Hata: " + errorCount.ToString();
                        }
                    }
                }
            }
        }
        else
        {
            // Stop drawing when not touching
            isDrawing = false;
        }
    }
    private void SendTriggerToParallelPort(int triggerValue)
    {
        // Set the Data value to the unique trigger value for each error
        Data = triggerValue;
        Out32(PortAddress, Data);

        // Reset the trigger immediately
        Data = 0;
        Out32(PortAddress, Data);
    }

    private bool IsWithinBorders(Vector3 point, GameObject square)
    {
        // Check if the point is within the borders of the square
        Renderer squareRenderer = square.GetComponent<Renderer>();
        Bounds squareBounds = squareRenderer.bounds;

        return squareBounds.Contains(point);
    }
    private void CenterSquareOnPaper(GameObject square)
    {
        // Ensure the square and paper are not null
        if (square == null || Paper == null)
        {
            return;
        }

        // Set the position of the square to the center of the paper
        square.transform.position = Paper.transform.position;
    }
}
