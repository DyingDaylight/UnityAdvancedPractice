using System.Collections.Generic;
using RTS.Units;
using UnityEngine;
using UnityEngine.AI;

namespace RTS.Systems
{
    public class MovementController : MonoBehaviour
    {
        [SerializeField, Min(0.01f)]
        private float formationSpacing = 1.5f;

        [SerializeField, Min(0.01f)]
        private float navMeshSearchRadius = 0.5f;
        
        private NavMeshPath path;
        
        private void Awake()
        {
            path = new NavMeshPath();
        }

        public bool TryMoveUnits(IReadOnlyCollection<ControllableUnit> units,
            Vector3 destination, bool append)
        {
            List<ControllableUnit> orderedUnits = new(units);
            
            if (!TryCalculateDestinations(orderedUnits, destination, out List<Vector3> destinations))
                return false;
            
            for (int i = 0; i < orderedUnits.Count; i++)
            {
                UnitMovement movement = orderedUnits[i].Movement;

                if (append)
                    movement.AddWaypoint(destinations[i]);
                else
                    movement.SendTo(destinations[i]);
            }
            
            return true;
        }
        
        public bool TryCalculateDestinations(IReadOnlyList<ControllableUnit> units,
            Vector3 target, out List<Vector3> destinations)
        {
            destinations = new List<Vector3>(units.Count);
            
            if (units.Count == 0)
                return false;
            
            Vector3? center = GetValidNavMeshPoint(target);
            
            if (!center.HasValue)
                return false;
            
            List<Vector3> formationPositions = FormationCalculator.CalculatePositions(center.Value,
                units.Count, formationSpacing);

            for (int i = 0; i < units.Count; i++)
            {
                Vector3? validPosition = GetValidNavMeshPoint(formationPositions[i]);
                
                if (!validPosition.HasValue)
                    return false;
                
                if (!units[i].Movement.TryCalculatePath(validPosition.Value, path))
                    return false;
                
                destinations.Add(validPosition.Value);
            }
            
            return true;
        }

        public bool TryCalculatePreviewPath(ControllableUnit unit, Vector3 target, NavMeshPath previewPath)
        {
            Vector3? validPosition = GetValidNavMeshPoint(target);
            
            if (!validPosition.HasValue)
                return false;
            
            return unit.Movement.TryCalculatePath(validPosition.Value, previewPath);
        }
        
        private Vector3? GetValidNavMeshPoint(Vector3 rawPoint)
        {
            if (NavMesh.SamplePosition(rawPoint, out NavMeshHit hit, navMeshSearchRadius,
                    NavMesh.AllAreas))
            {
                return hit.position; 
            }

            return null;
        }
    }
}