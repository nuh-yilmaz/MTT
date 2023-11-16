using UnityEngine;
using UnityEngine.XR;

public class Mirror : MonoBehaviour
{
    public GameObject leftHand, rightHand, rightHandMirror;
    private OVRHand m_hand;
    private OVRCustomSkeleton m_skeleton;
    private Transform indexTip, indexDistal, left, right;
    private Vector3 originPoint, targetPoint, direction;
    private RaycastHit touch;
    [SerializeField] LayerMask layerMask;
    private float distance, distanceOffset;
    public bool mirroring;
    private Vector3 currentPosition;

    public void Awake()
    {
        m_hand = GetComponent<OVRHand>();
        m_skeleton = GetComponent<OVRCustomSkeleton>();
    }

    void FixedUpdate()
    {
        if (indexTip == null && m_skeleton.IsInitialized)
        {
            Debug.Log("Skeleton initialized");
            indexTip = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_IndexTip].Transform;
            indexDistal = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Index3].Transform;
        }

        if (!indexTip) return;

        originPoint = indexDistal.position;
        targetPoint = indexTip.position;

        direction = Vector3.Normalize(targetPoint - originPoint);
        distance = Vector3.Distance(originPoint, targetPoint) + 0.00175f;
        distanceOffset = 0f;

        if (Physics.Raycast(originPoint, direction, out touch, distance + distanceOffset, layerMask) || Physics.Raycast(targetPoint, -direction, out touch, distance + distanceOffset, layerMask))
        {
            Vector3 newDirection = Vector3.Reflect(direction, touch.normal);
            Debug.DrawLine(originPoint, newDirection, Color.red);
            Debug.Log(newDirection);
        }
        if (mirroring)
        {
            
            // Determinate the position
            Vector3 playerToSourceHand = rightHand.transform.position - touch.normal;
            Vector3 playerToDestHand = Vector3.Reflect(playerToSourceHand,touch.normal);
            rightHandMirror.transform.position = touch.normal + playerToDestHand;

            
        }
    }
    
}