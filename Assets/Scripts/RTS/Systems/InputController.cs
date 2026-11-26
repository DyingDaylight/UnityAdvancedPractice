using System.Collections.Generic;
using System.Linq;
using RTS.Units;
using RTS.Visualization;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace RTS.Systems
{
    public class InputController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera inputCamera;
        [SerializeField] private UnitRegistry unitRegistry;
        [SerializeField] private SelectionBox selectionBox;
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private MovementController movementController;
        [SerializeField] private PathVisualizer pathVisualizer;
        
        [Header("Settings")]
        [SerializeField, Min(0f)] private float dragThreshold = 10f;
        [SerializeField] private LayerMask groundLayer;

        private Vector2 selectionStartPosition;
        private bool isSelecting;
        private bool isDragging;
        
        private NavMeshPath previewPath;
        
        private void OnEnable()
        {
            selectionManager.SelectionChanged += HandleSelectionChanged;
        }

        private void OnDisable()
        {
            selectionManager.SelectionChanged -= HandleSelectionChanged;
        }
        
        private void Start()
        {
            previewPath = new NavMeshPath();
            
            selectionBox.Hide();
        }

        private void Update()
        {
            if (Mouse.current == null)
                return;

            HandleSelectionInput();
            
            if (isSelecting)
                return;
            
            HandleMovementInput();
            UpdatePathPreview();
        }

        private void HandleSelectionInput()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                BeginSelection();
            
            if (!isSelecting)
                return;
            
            if (Mouse.current.leftButton.IsPressed())
                UpdateSelection();
            
            if (Mouse.current.leftButton.wasReleasedThisFrame)
                FinishSelection();
        }

        private void HandleMovementInput()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
                IssueMovementCommand();
        }

        private void BeginSelection()
        {
            selectionStartPosition = Mouse.current.position.ReadValue();
            
            isSelecting = true;
            isDragging = false;
            
            pathVisualizer.Clear();
        }

        private void UpdateSelection()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (!isDragging && HasExceededDragThreshold(mousePosition))
            {
                isDragging = true;
                selectionBox.Show();
            }
            
            if (isDragging)
                selectionBox.UpdateBounds(selectionStartPosition, mousePosition);
        }

        private void FinishSelection()
        {
            if (isDragging)
            {
                selectionBox.Hide();
                SelectUnitsInBox();
            }
            else
            {
                SelectUnit();
            }
            
            isSelecting = false;
            isDragging = false;
        }

        private bool HasExceededDragThreshold(Vector2 mousePosition)
        {
            return Vector2.Distance(selectionStartPosition, mousePosition) > dragThreshold;
        }
        
        private void UpdatePathPreview()
        {
            if (selectionManager.SelectedUnits.Count == 0)
            {
                pathVisualizer.Clear();
                return;
            }
        
            if (!TryGetGroundPoint(out Vector3 target))
            {
                pathVisualizer.Clear();
                return;
            }
            
            ControllableUnit unit = selectionManager.SelectedUnits.First();
            
            bool success = movementController.TryCalculatePreviewPath(unit, target, previewPath);

            if (success)
                pathVisualizer.ShowPath(previewPath.corners);
            else
                pathVisualizer.Clear();
        }
        
        private void SelectUnit()
        {
            if (!TryGetUnitUnderCursor(out ControllableUnit unit))
            {
                selectionManager.ClearSelection();
                return;
            }
            
            if (IsShiftPressed())
                selectionManager.ToggleSelection(unit);
            else
                selectionManager.ReplaceSelection(unit);
        }
        
        private void SelectUnitsInBox()
        {
            Rect selectionRect = CalculateSelectionRect();
            
            List<ControllableUnit> unitsInBox = GetUnitsInRect(selectionRect);
            
            if (IsShiftPressed())
                selectionManager.AddToSelection(unitsInBox);
            else
                selectionManager.ReplaceSelection(unitsInBox);
        }

        private void HandleSelectionChanged()
        {
            pathVisualizer.Clear();
        }

        private void IssueMovementCommand()
        {
            if (selectionManager.SelectedUnits.Count == 0)
                return;
            
            if (!TryGetGroundPoint(out Vector3 target))
                return;

            bool success = movementController.TryMoveUnits(
                selectionManager.SelectedUnits, target, IsShiftPressed());
            
            if (!success)
                Debug.Log("Invalid formation destination");
        }
        
        private bool IsShiftPressed()
        {
            return Keyboard.current != null &&
                   (Keyboard.current.leftShiftKey.isPressed ||
                    Keyboard.current.rightShiftKey.isPressed);
        }
        
        private Ray GetMouseRay()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            return inputCamera.ScreenPointToRay(mousePosition);
        }
        
        private bool TryGetGroundPoint(out Vector3 point)
        {
            Ray ray = GetMouseRay();

            if (Physics.Raycast(ray, out RaycastHit hit,Mathf.Infinity, groundLayer))
            {
                point = hit.point;
                return true;
            }

            point = default;
            return false;
        }
        
        private bool TryGetUnitUnderCursor(out ControllableUnit unit)
        {
            Ray ray = GetMouseRay();

            unit = null;

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return false;

            return hit.collider.TryGetComponent(out unit);
        }
        
        private Rect CalculateSelectionRect()
        {
            Vector2 endMousePos = Mouse.current.position.ReadValue();

            Vector2 min = Vector2.Min(selectionStartPosition, endMousePos);
            Vector2 max = Vector2.Max(selectionStartPosition, endMousePos);

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        
        private List<ControllableUnit> GetUnitsInRect(Rect selectionRect)
        {
            List<ControllableUnit> unitsInRect = new();
            
            foreach (ControllableUnit unit in unitRegistry.Units)
            {
                Vector3 screenPosition = inputCamera.WorldToScreenPoint(unit.transform.position);
                
                if (screenPosition.z <= 0)
                    continue;
                
                if (selectionRect.Contains((Vector2)screenPosition))
                    unitsInRect.Add(unit);
            }
            
            return unitsInRect;
        }
    }
}
