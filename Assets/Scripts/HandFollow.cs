using UnityEngine;

public class HandFollow : MonoBehaviour
{
    public OVRHand m_hand, r_hand;
    public OVRSkeleton m_skeleton, r_skeleton;
    public Transform rightHand;

    public void Awake()
    {
        m_hand = GetComponent<OVRHand>();
        m_skeleton = GetComponent<OVRSkeleton>();

        r_hand = rightHand.gameObject.GetComponent<OVRHand>();
        r_skeleton = rightHand.gameObject.GetComponent<OVRSkeleton>();
    }
    void Update()
    {
        // Check if the rightHand is assigned and the hand is tracking
        if (rightHand != null && r_hand.IsTracked)
        {
            rightHand.gameObject.SetActive(false);

            // Get the direction from rightHand to rightIndexTip
            Vector3 directionStart = r_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform.position;
            Vector3 directionFinish = r_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position;
            Vector3 direction = directionStart - directionFinish;
            Vector3 reverseDirection = m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position - m_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform.position;

            // Set the leftHand position to match the rightHand
            transform.position = directionFinish - reverseDirection;
            transform.rotation = Quaternion.LookRotation(direction,rightHand.up);
           
        }

    }
    
}
