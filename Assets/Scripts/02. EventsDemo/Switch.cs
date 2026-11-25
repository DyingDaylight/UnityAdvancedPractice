using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour
{
    [SerializeField] private UnityEvent OnActivated = new();

    public void Activate()
    {
        OnActivated.Invoke();
    }
}
