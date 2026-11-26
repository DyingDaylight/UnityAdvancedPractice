using System;
using System.Collections.Generic;
using RTS.Units;
using UnityEngine;

namespace RTS.Systems
{
    public class UnitRegistry : MonoBehaviour
    {
        [SerializeField] private List<ControllableUnit> initialUnits = new();
    
        private readonly HashSet<ControllableUnit> units = new();

        public IReadOnlyCollection<ControllableUnit> Units => units;
    
        public event Action<ControllableUnit> UnitRegistered;
        public event Action<ControllableUnit> UnitUnregistered;

        private void Awake()
        {
            foreach (ControllableUnit unit in initialUnits)
                Register(unit);
        }
    
        public bool IsRegistered(ControllableUnit unit)
        {
            return units.Contains(unit);
        }

        public void Register(ControllableUnit unit)
        {
            if (unit == null)
                return;

            bool wasAdded = units.Add(unit);

            if (!wasAdded)
                return;
        
            UnitRegistered?.Invoke(unit);
        }

        public void Unregister(ControllableUnit unit)
        {
            bool wasRemoved = units.Remove(unit);
        
            if (!wasRemoved)
                return;
        
            UnitUnregistered?.Invoke(unit);
        }
    }
}
