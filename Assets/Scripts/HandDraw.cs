using System.Runtime.InteropServices;
using System;
using UnityEngine;

public class HandDraw : MonoBehaviour
{
    private WhiteboardForHand whiteboard;
    private RaycastHit touch;

    private OVRHand m_hand;
    private OVRSkeleton m_skeleton;

    private Transform indexTip;
    private Transform indexDistal;

    [SerializeField]
    private Vector3 originPoint;
    private Vector3 targetPoint;
    private Vector3 direction;
    private float distance;
    [SerializeField, Range(0f, 0.5f)]
    private float distanceOffset;
    public bool isTouching;
    private const int WHITEBOARD_LAYER = 10;
    private const int ERROR_LAYER = 8;
    private const string ERROR_TAG = "Error";

    private int PortAddress;
    private int Data;

    [DllImport("inpoutx64.dll")]
    private static extern void Out32(int PortAddress, int Data);

    [DllImport("inpoutx64.dll")]
    private static extern UInt32 IsInpOutDriverOpen();

    // Counter variables
    private int errorHitCount = 0;
    public TimeDisplay timeDisplay;  // Reference to the TimeDisplay script

    public void Awake()
    {
        // Get the scripts that hold information about hand tracking
        m_hand = GetComponent<OVRHand>();
        m_skeleton = GetComponent<OVRSkeleton>();
        timeDisplay = FindAnyObjectByType<TimeDisplay>();  // Find the TimeDisplay script in the scene
    }

    // Update is called once per frame
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

        originPoint = indexDistal.position;
        targetPoint = indexTip.position;

        direction = Vector3.Normalize(targetPoint - originPoint);
        distance = Vector3.Distance(originPoint, targetPoint);
        distanceOffset = 0.0015f;

        // Cast a ray starting from the second index finger joint to the tip of the index finger.
        // Only check for objects that are in the whiteboard layer.
        if (Physics.Raycast(originPoint, direction, out touch, distance + distanceOffset, 1 << WHITEBOARD_LAYER) ||
            Physics.Raycast(targetPoint, -direction, out touch, distance + distanceOffset, 1 << WHITEBOARD_LAYER))
        {
            if (m_hand.GetFingerConfidence(OVRHand.HandFinger.Index) != OVRHand.TrackingConfidence.High)
                return;

            // indexTip.position = touch.point;
            // Get the Whiteboard component of the whiteboard we obtain from the raycast.
            whiteboard = touch.collider.GetComponent<WhiteboardForHand>();

            // touch.textureCoord gives us the texture coordinates at which our raycast
            // intersected the whiteboard. We can use this to tell the whiteboard where to
            // render the next.
            whiteboard.SetTouchPosition(touch.textureCoord.x, touch.textureCoord.y);

            // If the raycast intersects the board, it means we are touching the board
            whiteboard.ToggleTouch(true);
            isTouching = true;
            timeDisplay.UpdateTime();

            if (touch.collider.CompareTag(ERROR_TAG))
            {
                errorHitCount++;

                // Update the UI or send a signal as needed
                UpdateCountAndSignal();
            }
        }
        else
        {
            if (whiteboard != null)
            {
                // If the raycast no longer intersects, stop drawing on the board.
                whiteboard.ToggleTouch(false);
                isTouching = false;

                // Stop the stopwatch when not touching the whiteboard
                timeDisplay.StopTime();
            }
        }
    }
    void UpdateCountAndSignal()
    {
        // Update the UI text or send a signal using the specified function
        // Replace this with your actual UI update or signal-sending logic
        Debug.Log("Error Hit Count: " + errorHitCount);

        // Send the signal
        SendSignal();
    }

    void SendSignal()
    {
        // Send the signal using the specified function (Out32 in this case)
        // Replace this with your actual signal-sending logic
        Out32(20220, 2);

        // Reset the signal
        Out32(20220, 0);
    }
}
