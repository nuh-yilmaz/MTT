using UnityEngine;
using static OVRPlugin;

public class SetPivotPoint : MonoBehaviour
{
    public OVRHand r_hand;
    public OVRSkeleton r_skeleton;
    public GameObject rightHand;
    private Transform handIndexTipTransform;

    public void Awake()
    {
        r_hand = rightHand.GetComponent<OVRHand>();
        r_skeleton = rightHand.GetComponent<OVRSkeleton>();
    }

    void FixedUpdate()
    {
        // Check if the hand has been tracked
        if (r_hand.IsTracked)
        {
            handIndexTipTransform = r_skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;
            Debug.Log(handIndexTipTransform.position);

            // Set the pivot point of the hand to the right index position
            //transform.position = handIndexTipTransform.position;
        }
    }
}
