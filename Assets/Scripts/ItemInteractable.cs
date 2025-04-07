using UnityEngine;
using UnityEngine.Events;

public class ItemInteractable : MonoBehaviour, IInteractable
{
    public UnityEvent OnInteractEvent = new UnityEvent();

    public void OnInteract()
    {
        OnInteractEvent.Invoke();
    }
}
