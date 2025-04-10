using UnityEngine;

public class Collectable : MonoBehaviour, IInteractable
{
    void IInteractable.OnInteract()
    {
        gameObject.SetActive(false);
    }
}
