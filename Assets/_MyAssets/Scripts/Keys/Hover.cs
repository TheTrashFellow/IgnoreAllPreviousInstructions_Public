using UnityEngine;
using UnityEngine.AI;

public class Hover : MonoBehaviour
{
    [SerializeField] private float minHoverHeight = 0.5f;
    [SerializeField] private float maxHoverHeight = 1f;
    [SerializeField] private float hoverSpeed = 3f;
    [SerializeField] private float adjustmentSpeed = 5f;
    [SerializeField] private float navMeshSampleRadius = 2f; 

    private float hoverTimer;

    private void Update()
    {
        HoverAboveNavMesh();
    }

    private void HoverAboveNavMesh()
    {
        Vector3 position = transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(position, out hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            hoverTimer += Time.deltaTime * hoverSpeed;

            float hoverOffset = Mathf.Lerp(minHoverHeight, maxHoverHeight, (Mathf.Sin(hoverTimer) + 1) / 2);
            float targetHeight = hit.position.y + hoverOffset;

            Vector3 targetPosition = new Vector3(position.x, targetHeight, position.z);
            transform.position = Vector3.Lerp(position, targetPosition, Time.deltaTime * adjustmentSpeed);
        }
    }

    
    public void SetHoverRange(float minHeight, float maxHeight, float speed)
    {
        minHoverHeight = minHeight;
        maxHoverHeight = maxHeight;
        hoverSpeed = speed;
    }
}
