using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchInteraction : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<Switch>(out var target))
            {
                target.Activate();
            }
        }
    }
}
