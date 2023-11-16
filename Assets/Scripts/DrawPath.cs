using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawPath : MonoBehaviour
{
    private OVRHand m_hand;
    private OVRCustomSkeleton m_skeleton;

    private Transform indexTip;
  
    private LayerMask layerMask;
    private LineRenderer lineRenderer;
    [SerializeField]
    private List<Vector3>points = new();

    private Camera cam;
    private RaycastHit touch;
    private float distance;
    [SerializeField, Range(0f, 0.5f)]
    private float distanceOffset = 0.01f;
    [SerializeField]
    private Material lineMaterial;
    [SerializeField]
    private TimeDisplay time;
    
    public void Awake()
    {
        //Get the scripts that hold information about hand tracking
        m_hand = GetComponent<OVRHand>();
        m_skeleton = GetComponent<OVRCustomSkeleton>();

    }
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        cam = Camera.main;
        lineRenderer.enabled = false;        
    }
    void Update()
    {
        if (m_hand.GetFingerConfidence(OVRHand.HandFinger.Index) != OVRHand.TrackingConfidence.High)
            return;

        indexTip = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_IndexTip].Transform;

        if (!indexTip) return;
    }

    void FixedUpdate()
    {
        if (time.isStopwatchActive)
        {
            lineRenderer.enabled = true;
            Debug.DrawLine(cam.ScreenToWorldPoint(indexTip.position),touch.point,Color.red);
            points.Clear();
            Draw();
        }
    }
    private void Draw()
    {
        Ray ray = cam.ScreenPointToRay(indexTip.position);

        if (Physics.Raycast(ray, out touch, distance + distanceOffset, 1 << layerMask))
        {
            Vector3 touchPos = touch.point;
            touchPos.y = 0.9f;
            points.Add(touchPos);
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
            Debug.Log(touchPos);

        }
    }
}
