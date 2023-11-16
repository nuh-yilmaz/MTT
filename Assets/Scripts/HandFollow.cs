using UnityEngine;

public class HandFollow : MonoBehaviour
{
    public OVRHand l_hand;
    public OVRSkeleton l_skeleton;
    public Transform rightHand;
    private bool flipped;

    public void Awake()
    {
        l_hand = rightHand.GetComponent<OVRHand>();
        l_skeleton = rightHand.GetComponent<OVRSkeleton>();
    }

    void Update()
    {
        // Check if the rightHand is assigned and the hand is tracking
        if (rightHand != null && l_hand.IsTracked)
        {
            // If not flipped, flip the leftHand at the start
            if (!flipped)
            {
                FlipHand();
            }

            // Get the direction from rightHand to rightIndexTip
            Vector3 direction = l_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform.position - rightHand.position;

            // Set the leftHand position to match the rightHand
            transform.position = rightHand.position + new Vector3(-0.175f, 0, 0);

            // Rotate the leftHand to face the direction from rightHand to rightIndexTip
            transform.rotation = Quaternion.LookRotation(-direction, rightHand.up);
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
