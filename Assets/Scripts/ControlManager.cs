using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ControlManager : MonoBehaviour
{
    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private float dragThreshold = 10f;
    [SerializeField] private List<ControllableUnit> allUnits = new List<ControllableUnit>();
    
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
            SelectUnits();
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

            isDragging = false;
        }

        TryShowPath();
    }

    private void SelectUnitsInBox()
    {
        if (Keyboard.current != null && !Keyboard.current.leftShiftKey.isPressed)
            ClearSelection();
        
        Vector2 min = selectionBox.rect.min;
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
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (ControllableUnit controllableUnit in units)
            {
                controllableUnit.SendTo(hit.point);
            }
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
                unit.SetControlled(true);
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
            unit.SetControlled(false);
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
}
