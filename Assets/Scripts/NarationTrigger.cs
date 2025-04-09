using UnityEngine;

public class NarrationTrigger : MonoBehaviour
{
    [TextArea]
    public string narrationText;
    public AudioClip narrationAudio; // Audio clip to play with narration

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NarrationManager.Instance.ShowText(narrationText, narrationAudio);
            gameObject.SetActive(false); // Disable the trigger after use
        }
    }
}
