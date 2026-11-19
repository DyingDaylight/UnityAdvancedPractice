using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class AgentMovement : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private NavMeshAgent agent;
    
    [SerializeField] private float mudCost = 1f;
    
    private bool destinationReached;

    private void Start()
    {
        agent.SetAreaCost(NavMesh.GetAreaFromName("Mud"), mudCost);
        agent.SetDestination(target.position);
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            Debug.Log("Mouse clicked");
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                agent.SetDestination(hit.point);
                destinationReached = false;
                Debug.Log("Hit " + hit.point);
            }
        }
        
        if (!destinationReached && !agent.pathPending &&
            agent.hasPath &&
            agent.pathStatus == NavMeshPathStatus.PathComplete &&
            agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.5f))
        {
            destinationReached = true;
            Debug.Log("Reached destination!");
        }
    }
}
