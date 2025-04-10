using UnityEngine;

public class Collectable : MonoBehaviour, IInteractable
{
    [SerializeField] AudioSource collectibleSfx;
    void IInteractable.OnInteract()
    {
        collectibleSfx.Play();
        gameObject.SetActive(false);
        GameManager.instance.interactedThisLoop = true;
    }
}
