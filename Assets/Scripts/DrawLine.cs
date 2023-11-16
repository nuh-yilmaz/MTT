using System.Runtime.InteropServices;
using System;
using TMPro;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

public class DrawManager : MonoBehaviour
{
    private RaycastHit touch;

    private OVRHand m_hand;
    private OVRCustomSkeleton m_skeleton;

    private Transform indexTip;
    private Transform indexDistal;

    [SerializeField]
    private Vector3 originPoint, targetPoint, touchPos;

    [SerializeField]
    private bool touching;

    [SerializeField]
    private LineRenderer lineRenderer;
    
    [SerializeField]
    private List<Vector3> touchPosList;
    private const int WHITEBOARD_LAYER = 10;

    [SerializeField]
    private Material lineMaterial;
    private Vector3 firstPoint;
    private GameObject time;

    void Awake()
    {
        //Get the scripts that hold information about hand tracking
        m_hand = GetComponent<OVRHand>();
        m_skeleton = GetComponent<OVRCustomSkeleton>();
        time = GameObject.Find("Time");
        lineRenderer = GameObject.Find("Time").GetComponent<LineRenderer>();
    }

    void Start()
    {
        //Set Up Line Renderer, Add Start Point Info and Close
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = lineMaterial;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        touchPosList.Clear();
        lineRenderer.positionCount = touchPosList.Count;

        firstPoint = new(time.transform.position.x, 0.9f, time.transform.position.z);
        touchPosList.Add(firstPoint);
        lineRenderer.SetPosition(0, touchPosList[0]);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        /*if (m_hand.GetFingerConfidence(OVRHand.HandFinger.Index) != OVRHand.TrackingConfidence.High)
            return;*/

        // Hands are not initialized immediately, so we need to wait until they appear
        // and are initialized.
        if (indexTip == null && m_skeleton.IsInitialized)
        {
            Debug.Log("Skeleton initialized");
            indexTip = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_IndexTip].Transform;
            indexDistal = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Index3].Transform;
        }

        // If hands aren't initialized yet, don't execute the rest of the script.
        if (!indexTip) return;

        // Since we're going to be using our index finger as the pen
        // for this whiteboard, we need to cast a ray from the second joint
        // of our index finger to the tip of the finger.

        originPoint = indexDistal.position;
        targetPoint = indexTip.position;
        Vector3 direction = Vector3.Normalize(targetPoint - originPoint);
        float distance = Vector3.Distance(originPoint, targetPoint) + 0.001f;
        float distanceOffset = 0f;

        //Cast a ray starting from the second index finger joint to the tip of the index finger.
        //Only check for objects that are in the whiteboard layer.
        if (Physics.Raycast(originPoint, direction, out touch, distance+distanceOffset, 1 << WHITEBOARD_LAYER) ||
            Physics.Raycast(targetPoint, -direction, out touch, distance+distanceOffset, 1 << WHITEBOARD_LAYER))
        {
            indexTip.position = touch.collider.ClosestPointOnBounds(indexTip.position);
            touching = true;

            if (time.GetComponent<TimeDisplay>().isStopwatchActive)
            {
                if (touching)
                {
                    lineRenderer.positionCount++;
                    touchPos = new(touch.collider.ClosestPointOnBounds(indexTip.position).x, 0.9f, touch.collider.ClosestPointOnBounds(indexTip.position).z);

                    if (Vector3.Distance(touchPosList[lineRenderer.positionCount - 2], touchPos) > 0.001f)
                    {
                        touchPosList.Add(touchPos);
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, touchPosList[lineRenderer.positionCount - 1]);
                    }
                }
            }
        }
    }
}