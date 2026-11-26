using UnityEngine;

namespace RTS
{
    public class ControllableUnit : MonoBehaviour
    {
        
        [SerializeField] private UnitMovement movement;
        [SerializeField] private GameObject selectionIndicator;

        public UnitMovement Movement => movement;

        public void SetSelected(bool selected)
        {
            selectionIndicator.SetActive(selected);
        }
    }
}
