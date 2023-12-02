using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

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

    private int errorCount = 0;
    public Text errorCountText;

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

    private bool isAdjustingPaper = false;

    private bool isPaperPlaced = false;

    private bool handsMovedAway = false;
    private Vector3 paperPlacementPosition;

    public void Awake()
    {
        m_hand = GetComponent<OVRHand>();
        m_skeleton = GetComponent<OVRSkeleton>();
    }

    void Start()
    {
        InitializeLineRenderer();
        InitializeParallelPort();

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
        if (indexTip == null && m_skeleton.IsInitialized)
        {
            indexTip = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_IndexTip].Transform;
            indexDistal = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Index3].Transform;
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
            if (m_hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
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
                        if (!IsWithinBorders(touch.point, SquareOut) || IsWithinBorders(touch.point, SquareIn))
                        {
                            errorCount++;
                            SendTriggerToParallelPort(2);

                            if (errorCountText != null)
                            {
                                errorCountText.text = "Hata: " + errorCount.ToString();
                            }
                        }
                    }
                }
            }
        }
        else
        {
            isDrawing = false;
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
  }