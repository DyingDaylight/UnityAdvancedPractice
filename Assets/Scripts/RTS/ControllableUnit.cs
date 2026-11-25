using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTS
{
    public class ControllableUnit : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float jumpDuration = 0.6f;
        
        private bool isControlled = false;
        private Material material;
        private Color color;
    
        private bool hasDestination;
        
        private bool isJumping;
        
        private Queue<Vector3> waypoints = new();
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            material = GetComponent<Renderer>().material;
            color = material.color;
            
            agent.autoTraverseOffMeshLink = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (isJumping)
                return;

            if (agent.isOnOffMeshLink)
            {
                StartCoroutine(Jump());
                return;
            }
            
            if (!hasDestination || agent.pathPending)
                return;
            
            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
                return;
            
            if (agent.remainingDistance > agent.stoppingDistance)
                return;
            
            hasDestination = false;
            
            Debug.Log("Reached destination!");
            
            if (waypoints.Count > 0)
                MoveTo(waypoints.Dequeue());
        }

        public void SetControlled(bool controlled)
        {
            isControlled = controlled;
            material.color = controlled ? Color.paleVioletRed : color;
        }

        public void SendTo(Vector3 desiredPosition)
        {
            waypoints.Clear();
            MoveTo(desiredPosition);
        }

        public void AddWaypoint(Vector3 desiredPosition)
        {
            if (!hasDestination)
            {
                MoveTo(desiredPosition);
                return;
            }
            
            waypoints.Enqueue(desiredPosition);
        }

        public bool TryCalculatePath(Vector3 destination, NavMeshPath path)
        {
            return agent.CalculatePath(destination, path) &&
                   path.status == NavMeshPathStatus.PathComplete;
        }

        private void MoveTo(Vector3 destination)
        {
            hasDestination = agent.SetDestination(destination);
        }

        private IEnumerator Jump()
        {
            isJumping = true;
            
            OffMeshLinkData linkData = agent.currentOffMeshLinkData;
            
            Vector3 start = transform.position;
            Vector3 end = linkData.endPos + Vector3.up * agent.baseOffset;

            float elapsed = 0f;

            while (elapsed < jumpDuration)
            {
                float t = elapsed / jumpDuration;
                
                Vector3 position = Vector3.Lerp(start, end, t);
                
                position.y += 4f * jumpHeight * t * (1f - t);
                
                transform.position = position;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.position = end;
            
            agent.CompleteOffMeshLink();
            
            isJumping = false;
        }
    }
}
