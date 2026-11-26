using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RTS
{
    public class UnitMovement : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        
        [Header("Jump")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.5f;
        [SerializeField, Min(0.01f)] private float jumpDuration = 0.6f;
    
        private readonly Queue<Vector3> waypoints = new();

        private bool hasDestination;

        private bool isJumping;
        private bool replaceDestinationAfterJump;


        private void Start()
        {
            agent.autoTraverseOffMeshLink = false;
        }

        private void Update()
        {
            if (isJumping)
                return;

            if (ShouldStartJump())
            {
                StartCoroutine(Jump());
                return;
            }
            
            if (!IsReadyToCheckDestination())
                return;

            if (!HasCompletePath())
            {
                HandleUnreachableDestination();
                return;
            }

            if (HasReachedDestination())
                HandleDestinationReached();
        }

        public void SendTo(Vector3 desiredPosition)
        {
            waypoints.Clear();

            if (isJumping)
            {
                replaceDestinationAfterJump = true;
                waypoints.Enqueue(desiredPosition);
                return;
            }
            
            TryMoveTo(desiredPosition);
        }

        public void AddWaypoint(Vector3 desiredPosition)
        {
            if (hasDestination || isJumping)
            {
                waypoints.Enqueue(desiredPosition);
                return;
            }
            
            TryMoveTo(desiredPosition);
        }

        public bool TryCalculatePath(Vector3 destination, NavMeshPath path)
        {
            return agent.CalculatePath(destination, path) &&
                   path.status == NavMeshPathStatus.PathComplete;
        }

        private bool TryMoveTo(Vector3 destination)
        {
            hasDestination = agent.SetDestination(destination);

            if (!hasDestination)
            {
                agent.ResetPath();
                Debug.LogWarning("Failed to set destination.");
            }
            
            return hasDestination;
        }
        
        private void MoveToNextWaypoint()
        {
            hasDestination = false;
            
            while (waypoints.Count > 0)
            {
                if (TryMoveTo(waypoints.Dequeue()))
                    return;
            }
        }
        
        private bool IsReadyToCheckDestination()
        {
            return hasDestination && !agent.pathPending;
        }

        private bool HasCompletePath()
        {
            return agent.pathStatus == NavMeshPathStatus.PathComplete;
        }
        
        private bool HasReachedDestination()
        {
            return agent.remainingDistance <= agent.stoppingDistance;
        }
        
        private void HandleUnreachableDestination()
        {
            Debug.LogWarning("Destination unreachable. Skipping.");
                
            agent.ResetPath();
            MoveToNextWaypoint();    
        }

        private void HandleDestinationReached()
        {
            Debug.Log("Reached destination!");

            MoveToNextWaypoint();
        }
        
        private bool ShouldStartJump()
        {
            return agent.isOnOffMeshLink;
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
                
                transform.position = CalculateJumpPosition(start, end, t);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.position = end;
            agent.CompleteOffMeshLink();
            
            isJumping = false;

            ApplyDeferredDestination();
        }

        private Vector3 CalculateJumpPosition(Vector3 start, Vector3 end, float t)
        {
            Vector3 position = Vector3.Lerp(start, end, t);
            position.y += 4f * jumpHeight * t * (1f - t);
            return position;
        }
        
        private void ApplyDeferredDestination()
        {
            if (!replaceDestinationAfterJump)
                return;

            replaceDestinationAfterJump = false;

            agent.ResetPath();
            MoveToNextWaypoint();
        }
    }
}
