using System;
using System.Collections.Generic;
using RTS.Units;
using UnityEngine;

namespace RTS.Systems
{
    public class SelectionManager : MonoBehaviour
    {
        [SerializeField] private UnitRegistry unitRegistry;
    
        private readonly HashSet<ControllableUnit> selectedUnits = new();
    
        public IReadOnlyCollection<ControllableUnit> SelectedUnits => selectedUnits;
    
        public event Action SelectionChanged;

        private void OnEnable()
        {
            unitRegistry.UnitUnregistered += HandleUnitUnregistered;
        }

        private void OnDisable()
        {
            unitRegistry.UnitUnregistered -= HandleUnitUnregistered;
        }
    
        public void ToggleSelection(ControllableUnit unit)
        {
            bool changed;

            if (IsSelected(unit))
                changed = DeselectUnit(unit);
            else
                changed = SelectUnit(unit);

            if (changed)
                SelectionChanged?.Invoke();
        }
    
        public void AddToSelection(IEnumerable<ControllableUnit> units)
        {
            bool changed = false;

            foreach (ControllableUnit unit in units)
            {
                if (SelectUnit(unit))
                    changed = true;
            }

            if (changed)
                SelectionChanged?.Invoke();
        }

        public void ClearSelection()
        {
            if (selectedUnits.Count == 0)
                return;

            foreach (ControllableUnit unit in selectedUnits)
            {
                if (unit != null)
                    unit.SetSelected(false);
            }

            selectedUnits.Clear();

            SelectionChanged?.Invoke();
        }
    
        public void ReplaceSelection(ControllableUnit unit)
        {
            ReplaceSelection(new[] { unit });
        }
    
        public void ReplaceSelection(IEnumerable<ControllableUnit> units)
        {
            HashSet<ControllableUnit> newSelection = new(units);
            List<ControllableUnit> unitsToDeselect = new();

            foreach (ControllableUnit unit in selectedUnits)
            {
                if (!newSelection.Contains(unit))
                    unitsToDeselect.Add(unit);
            }

            bool changed = false;

            foreach (ControllableUnit unit in unitsToDeselect)
            {
                if (DeselectUnit(unit))
                    changed = true;
            }

            foreach (ControllableUnit unit in newSelection)
            {
                if (SelectUnit(unit))
                    changed = true;
            }

            if (changed)
                SelectionChanged?.Invoke();
        }
    
        private bool IsSelected(ControllableUnit unit)
        {
            return selectedUnits.Contains(unit);
        }
    
        private void HandleUnitUnregistered(ControllableUnit unit)
        {
            RemoveFromSelection(unit);
        }

        private bool SelectUnit(ControllableUnit unit)
        {
            if (unit == null)
                return false;
        
            if (!unitRegistry.IsRegistered(unit))
                return false;
        
            bool added = selectedUnits.Add(unit);

            if (!added)
                return false;

            unit.SetSelected(true);

            return true;
        }

        private bool DeselectUnit(ControllableUnit unit)
        {
            if (unit == null)
                return false;
        
            bool removed = selectedUnits.Remove(unit);

            if (!removed)
                return false;

            unit.SetSelected(false);

            return true;
        }
    
        private void RemoveFromSelection(ControllableUnit unit)
        {
            if (DeselectUnit(unit))
                SelectionChanged?.Invoke();
        }
    }
}
