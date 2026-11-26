using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace RTS
{
    public class ControlManager : MonoBehaviour
    {
        [SerializeField] private RectTransform selectionBox;
        [SerializeField] private float dragThreshold = 10f;
        [SerializeField] private List<ControllableUnit> allUnits = new List<ControllableUnit>();
    
        [SerializeField, Min(0.01f)] private float formationSpacing = 1.5f;
        [SerializeField] private LayerMask groundLayer;
        private LineRenderer lineRenderer;
    
        private List<ControllableUnit> units = new List<ControllableUnit>();
        private NavMeshPath path;
    
        private Vector2 startMousePos;
        private bool isDragging = false;
    
        private void Start()
        {
            path = new NavMeshPath();
        
            lineRenderer  = GetComponent<LineRenderer>();
            selectionBox.gameObject.SetActive(false);
        }

        void Update()
        {
            if (Mouse.current == null)
                return;
        
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                startMousePos = Mouse.current.position.ReadValue();
                isDragging = false;
                return;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                SelectDestination();
                return;
            }

            if (Mouse.current.leftButton.IsPressed())
            {
                if (!isDragging && Vector2.Distance(startMousePos, Mouse.current.position.ReadValue()) > dragThreshold)
                {
                    isDragging = true;
                    selectionBox.gameObject.SetActive(true);
                }
            
                if (isDragging)
                {
                    UpdateSelectionBox(Mouse.current.position.ReadValue());
                    return;
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (isDragging)
                {
                    selectionBox.gameObject.SetActive(false);
                    SelectUnitsInBox();
                }
                else
                {
                    SelectUnits();
                }

                isDragging = false;
            }

            TryShowPath();
        }

        private void SelectUnitsInBox()
        {
            if (Keyboard.current != null && !Keyboard.current.leftShiftKey.isPressed)
                ClearSelection();
        
            Vector2 endMousePos = Mouse.current.position.ReadValue();

            Vector2 min = Vector2.Min(startMousePos, endMousePos);
            Vector2 max = Vector2.Max(startMousePos, endMousePos);

            Rect selectionRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            
            foreach (ControllableUnit unit in allUnits)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);
                
                if (screenPosition.z <= 0)
                    continue;
                
                if (selectionRect.Contains((Vector2)screenPosition))
                {
                    if (units.Contains(unit))
                        continue;

                    unit.SetSelected(true);
                    units.Add(unit);
                }
            }
        }

        private void TryShowPath()
        {
            if (units.Count == 0)
                return;
        
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Vector3? targetPosition = GetValidNavMeshPoint(hit.point);

                if (!targetPosition.HasValue)
                {
                    Debug.Log("No target position");
                    ClearLine();
                    return;
                }
            
                Vector3 startPosition = CalculateStartPosition();
            
                if (NavMesh.CalculatePath(startPosition, targetPosition.Value, NavMesh.AllAreas, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        lineRenderer.positionCount = path.corners.Length;
                        lineRenderer.SetPositions(path.corners);
                    }
                }
            }
        }

        private Vector3 CalculateStartPosition()
        {
            if (units.Count == 0)
                return Vector3.zero;
        
            return units[0].transform.position;
        }

        private void SelectDestination()
        {
            if (units.Count == 0)
                return;
            
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
                return;

            if (!TryCalculateDestinations(hit.point, out List<Vector3> destinations))
            {
                Debug.Log("Invalid formation destination");
                return;
            }
            
            bool append = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

            for (int i = 0; i < units.Count; i++)
            {
                if (append)
                    units[i].Movement.AddWaypoint(destinations[i]);
                else
                    units[i].Movement.SendTo(destinations[i]);
            }
        }

        private void SelectUnits()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out ControllableUnit unit))
                {
                    Debug.Log("Clicked on unit");
                    ClearSelection();
                    unit.SetSelected(true);
                    units.Add(unit);
                } else
                {
                    ClearSelection();
                    Debug.Log("Clicked outside unit");
                }
            }
        }

        private void ClearSelection()
        {
            foreach (ControllableUnit unit in units)
            {
                unit.SetSelected(false);
            }
            units.Clear();
            ClearLine();
        }
    
        private void ClearLine()
        {
            lineRenderer.positionCount = 0;
        }
    
        private Vector3? GetValidNavMeshPoint(Vector3 rawPoint)
        {
            NavMeshHit hit;
            float searchRadius = 0.5f;

            if (NavMesh.SamplePosition(rawPoint, out hit, searchRadius, NavMesh.AllAreas))
            {
                return hit.position; 
            }

            return null;
        }
    
        void UpdateSelectionBox(Vector2 currentMousePos)
        {
            float width = currentMousePos.x - startMousePos.x;
            float height = currentMousePos.y - startMousePos.y;

            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            selectionBox.anchoredPosition = startMousePos + new Vector2(width / 2f, height / 2f);
        }

        private bool TryCalculateDestinations(Vector3 target, out List<Vector3> destinations)
        {
            destinations = new List<Vector3>();
            
            Vector3? center = GetValidNavMeshPoint(target);
            
            if (!center.HasValue)
                return false;
            
            List<Vector3> position = FormationCalculator.CalculatePositions(center.Value,
                units.Count, formationSpacing);

            for (int i = 0; i < units.Count; i++)
            {
                Vector3? validPosition = GetValidNavMeshPoint(position[i]);
                
                if (!validPosition.HasValue)
                    return false;

                if (!units[i].Movement.TryCalculatePath(validPosition.Value, path))
                    return false;
                
                destinations.Add(validPosition.Value);
            }
            
            return true;
        }
    }
}
