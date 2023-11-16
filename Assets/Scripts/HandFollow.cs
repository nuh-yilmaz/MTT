using UnityEngine;

public class HandFollow : MonoBehaviour
{
    public OVRHand l_hand, r_hand;
    public OVRSkeleton l_skeleton, r_skeleton;
    public Transform rightHand;
    private bool flipped;

    public void Awake()
    {
        l_hand = GetComponent<OVRHand>();
        l_skeleton = GetComponent<OVRSkeleton>();

        r_hand = rightHand.gameObject.GetComponent<OVRHand>();
        r_skeleton = rightHand.gameObject.GetComponent<OVRSkeleton>();
    }

    void Update()
    {
        // Check if the rightHand is assigned and the hand is tracking
        if (rightHand != null && r_hand.IsTracked)
        {
            // If not flipped, flip the leftHand at the start
            if (!flipped)
            {
                FlipHand();
            }

            // Get the direction from rightHand to rightIndexTip
            Vector3 direction = r_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position - rightHand.position;
            Vector3 reversedirection = l_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position - transform.position;

            // Set the leftHand position to match the rightHand
            transform.position = rightHand.position + direction - reversedirection;

            // Rotate the leftHand to face the right hand's index tip
            transform.LookAt(rightHand.position - direction, rightHand.up);
        }
    }

    void FlipHand()
    {
        GameObject bones = GameObject.Find("[BuildingBlock] Hand Tracking left/Bones");
        if (bones != null)
        {
            Vector3 scale = bones.transform.localScale;
            scale.z = -Mathf.Abs(scale.z);
            bones.transform.localScale = scale;
            flipped = true; // Set the flag to true so that this block is not executed in subsequent frames
        }
        else
        {
            Debug.LogError("Bones GameObject not found.");
        }
    }
}
