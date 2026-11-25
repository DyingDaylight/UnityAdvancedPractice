using UnityEngine;
using UnityEngine.AI;

public class ControllableUnit : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;

    private bool isControlled = false;
    private Material material;
    private Color color;
    
    private bool destinationReached;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        material = GetComponent<Renderer>().material;
        color = material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isControlled)
            return;
        
        if (!destinationReached && !agent.pathPending &&
            agent.hasPath &&
            agent.pathStatus == NavMeshPathStatus.PathComplete &&
            agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.5f))
        {
            destinationReached = true;
            Debug.Log("Reached destination!");
        }
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;
        material.color = controlled ? Color.paleVioletRed : color;
    }

    public void SendTo(Vector3 desiredPosition)
    {
        Debug.Log("Starting for destination!");
        agent.SetDestination(desiredPosition);
        destinationReached = false;
    }
}
