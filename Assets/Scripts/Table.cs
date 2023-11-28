using UnityEngine;
using static OVRPlugin;

public class Paper : MonoBehaviour
{
    [SerializeField] private OVRHand m_hand; // Reference to the OVRHand component
    [SerializeField] private OVRSkeleton m_skeleton; // Reference to the OVRSkeleton component
    [SerializeField] private LayerMask interactableLayer; // Layer for interactable objects
    [SerializeField] private GameObject paperPrefab; // Prefab for the paper object
    [SerializeField] private float distanceOffset = 0.01f; // Offset for raycasting distance

    public void Awake()
    {
        // Get the scripts that hold information about hand tracking
        m_hand = GetComponent<OVRHand>();
        m_skeleton = GetComponent<OVRSkeleton>();
    }

    private void Start()
    {
        InitializeHandSkeleton();
    }

    private void Update()
    {
        if (m_hand.IsInitialized)
        {
            PerformHandRaycast();
        }
    }

    private void InitializeHandSkeleton()
    {
        if (m_hand == null || !m_hand.IsInitialized)
        {
            Debug.LogError("Hand not initialized");
            return;
        }

        Debug.Log("Hand initialized");
    }

    private void PerformHandRaycast()
    {
        Transform indexTip = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_IndexTip].Transform;
        Transform indexDistal = m_skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Index3].Transform;

        if (indexTip == null || indexDistal == null)
        {
            Debug.LogError("Index finger not properly set up");
            return;
        }

        Vector3 originPoint = indexDistal.position;
        Vector3 targetPoint = indexTip.position;

        Vector3 direction = Vector3.Normalize(targetPoint - originPoint);
        float distance = Vector3.Distance(originPoint, targetPoint);

        // Cast a ray starting from the second index finger joint to the tip of the index finger.
        // Only check for objects that are in the interactable layer.
        if (Physics.Raycast(originPoint, direction, out RaycastHit hit, distance + distanceOffset, interactableLayer) ||
            Physics.Raycast(targetPoint, -direction, out hit, distance + distanceOffset, interactableLayer))
        {
            // Check if the hit object is the table
            if (hit.collider.CompareTag("Table"))
            {
                // Perform your action (e.g., spawn paper on the table)
                SpawnPaper(hit.point);
                Debug.Log(hit.point);
            }
        }
    }

    private void SpawnPaper(Vector3 spawnPosition)
    {
        // Instantiate the paper object at the specified position
        GameObject paperInstance = Instantiate(paperPrefab, spawnPosition, Quaternion.identity);
    }
}
