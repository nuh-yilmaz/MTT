using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using static OVRPlugin;
using Unity.VisualScripting;

public class ProjectManager : MonoBehaviour
{
    public enum DrawingMode
    {
        normalMode,
        mirrorMode
    }

    private DrawingMode currentDrawingMode = DrawingMode.normalMode;
    private int totalDrawings = 0;
    private int maxDrawings = 20;

    public OVRHand m_hand, r_hand, l_hand;
    public OVRSkeleton m_skeleton, r_skeleton, l_skeleton;
    public Transform rightHand, leftHand, mirHand;
    private Transform indexTip;
    private Transform indexDistal;
    [SerializeField, Range(0f, 0.5f)]
    private float distanceOffset;

    private bool isAdjustingPaper = false;
    private bool isPaperPlaced = false;
    private bool handsMovedAway = false;
    private Vector3 paperPlacementPosition;
    [SerializeField] LayerMask layerMask;
    private LineRenderer lineRenderer;
    private bool isDrawing = false;
    private Vector3 firstTouchPoint;

    private int errorCount = 0;
    public Text errorCountText;
    private bool isErrorCounted;
    public GameObject SquareIn;
    public GameObject SquareOut;
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

    public void Awake()
    {
        r_hand = rightHand.gameObject.GetComponent<OVRHand>();
        r_skeleton = rightHand.gameObject.GetComponent<OVRSkeleton>();

        l_hand = leftHand.gameObject.GetComponent<OVRHand>();
        l_skeleton = leftHand.gameObject.GetComponent<OVRSkeleton>();

        m_hand = mirHand.gameObject.GetComponent<OVRHand>();
        m_skeleton = mirHand.gameObject.GetComponent<OVRSkeleton>();
    }

    void Start()
    {
        InitializeLineRenderer();
        InitializeParallelPort();
        if (currentDrawingMode == DrawingMode.mirrorMode) 
        {
            Mirroring();
            rightHand.gameObject.SetActive(false);
            leftHand.gameObject.SetActive(false);
        }
        else
        {
            rightHand.gameObject.SetActive(false);
            mirHand.gameObject.SetActive(false);
        }
        if (SquareIn == null || SquareOut == null || Paper == null)
        {
            Debug.LogError("Please assign SquareIn, SquareOut, and Paper in the Unity Editor.");
            return;
        }
    }
    void InitializeLineRenderer()
    {
        GameObject lineObject = new GameObject("Line");
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.Simplify(20);
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.material = new Material(Shader.Find("Standard")) { color = Color.black };
    }

    void InitializeParallelPort()
    {
        PortAddress = 0x278; // LPT3 address
    }

    void FixedUpdate()
    {
        if (indexTip == null && r_skeleton.IsInitialized)
        {
            indexTip = r_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_IndexTip].Transform;
            indexDistal = r_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Index3].Transform;
        }

        if (!indexTip || !m_hand.IsDataHighConfidence)
        {
            // If hand tracking data is not available or not confident, return
            return;
        }

        Vector3 originPoint = indexDistal.position;
        Vector3 targetPoint = indexTip.position;

        Vector3 direction = Vector3.Normalize(targetPoint - originPoint);
        float distance = Vector3.Distance(originPoint, targetPoint);

        if (!isPaperPlaced)
        {
            // If Paper is not placed, it follows the index finger tip only when finger is pinching
            if (r_hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                Vector3 targetPos = targetPoint;
                targetPos.y = Paper.transform.position.y; // Maintain the Y-coordinate of the table
                Paper.transform.position = targetPos;
                paperPlacementPosition = Paper.transform.position; // Store the paper placement position
                isPaperPlaced = true; // Set isPaperPlaced to true when the paper is placed
            }
        }

        // Check if the hands have moved away at least 5 cm from the paper placement position
        if (isPaperPlaced && Vector3.Distance(paperPlacementPosition, targetPoint) > 0.05f)
        {
            handsMovedAway = true;
        }

        // Check if the distance between touch positions is greater than 1mm and Paper is placed
        if (isPaperPlaced && handsMovedAway && distance > 0.01f)
        {
            if (Physics.Raycast(originPoint, direction, out RaycastHit touch, distance + distanceOffset, layerMask) ||
                Physics.Raycast(targetPoint, -direction, out touch, distance + distanceOffset, layerMask))
            {
                if (touch.collider != null)
                {
                    Vector3 touchPoint = touch.point;
                    touchPoint.y = Paper.transform.position.y; // Set the Y-coordinate to match the table

                    if (!isDrawing)
                    {
                        isDrawing = true;
                        startTime = Time.time;

                        // Create a new empty GameObject to serve as the container for the line renderer
                        GameObject lineContainer = new GameObject("LineContainer");
                        lineContainer.transform.position = touchPoint;
                        lineContainer.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                        // Attach the line renderer to the line container
                        lineRenderer.transform.parent = lineContainer.transform;

                        SendTriggerToParallelPort(1);

                        lineRenderer.positionCount = 1;
                        lineRenderer.SetPosition(0, touch.point);

                        // Store the first touch point for comparison
                        firstTouchPoint = touch.point;
                    }
                    else
                    {
                        float lastDistance = Vector3.Distance(lineRenderer.GetPosition(lineRenderer.positionCount - 1), touch.point);
                        // Draw a line only if the distance between touch positions is greater than 1mm
                        if (lastDistance > 0.001f)
                        {
                            lineRenderer.positionCount++;
                            lineRenderer.SetPosition(lineRenderer.positionCount - 1, touch.point);
                        }
                    }

                    if (elapsedTimeText != null)
                    {
                        totalElapsedTime += Time.deltaTime;
                        elapsedTimeText.text = "Süre: " + totalElapsedTime.ToString("F2");
                    }

                    if (startTime != 0)
                    {
                        bool isOutsideSquareOut = !IsWithinBorders(touch.point, SquareOut);
                        bool isInsideSquareIn = IsWithinBorders(touch.point, SquareIn);

                        if (isOutsideSquareOut || isInsideSquareIn)
                        {
                            // Count the error only once for each entry into SquareIn or exit from SquareOut
                            if (!isErrorCounted)
                            {
                                errorCount++;
                                SendTriggerToParallelPort(2);

                                if (errorCountText != null)
                                {
                                    errorCountText.text = "Hata: " + errorCount.ToString();
                                }

                                isErrorCounted = true; // Set the flag to indicate that an error has been counted
                            }
                        }
                        else
                        {
                            isErrorCounted = false; // Reset the flag when within SquareOut and outside SquareIn
                        }
                    }
                }
            }
        }
        else
        {
            if (isDrawing)
            {
                // Check if the first touch point is the same as the last touch point to detect the end of drawing
                if (Vector3.Distance(firstTouchPoint, lineRenderer.GetPosition(lineRenderer.positionCount - 1)) < 0.001f)
                {
                    // Drawing is finished
                    isDrawing = false;
                    isErrorCounted = false; // Reset the flag when drawing is finished
                    SwitchDrawingMode();
                }
            }
        }
    }
    void Update()
    {
        // Detect input to start adjusting the paper's position
        if (m_hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            isAdjustingPaper = true;
        }

        // Detect input to stop adjusting the paper's position
        if (!m_hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            isAdjustingPaper = false;
        }
    }

    private void SendTriggerToParallelPort(int triggerValue)
    {
        Data = triggerValue;
        Out32(PortAddress, Data);

        Data = 0;
        Out32(PortAddress, Data);
    }

    private bool IsWithinBorders(Vector3 point, GameObject square)
    {
        Renderer squareRenderer = square.GetComponent<Renderer>();
        Bounds squareBounds = squareRenderer.bounds;

        return squareBounds.Contains(point);
    }
    void SwitchDrawingMode()
    {
        totalDrawings++;

        SetDrawingMode(UnityEngine.Random.Range(0, 2) == 0 ? DrawingMode.normalMode : DrawingMode.mirrorMode);
        handsMovedAway = false;
        lineRenderer.positionCount = 0;

        if (totalDrawings > maxDrawings)
        {
            //UnityEditor.EditorApplication.isPlaying = false;
        }
    }
    public void SetDrawingMode(DrawingMode mode)
    {
        currentDrawingMode = mode;
    }
    private void Mirroring()
    {
        // Get the direction from rightHand to rightIndexTip
        Vector3 directionStart = r_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform.position;
        Vector3 directionFinish = r_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
        Vector3 direction = directionStart - directionFinish;
        Vector3 reverseDirection = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position - m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform.position;

        // Set the leftHand position to match the rightHand
        transform.position = directionFinish - reverseDirection;
        transform.rotation = Quaternion.LookRotation(direction, rightHand.up);
    }
}