using UnityEngine;
using System.Collections;

public class TimedNarration : MonoBehaviour
{
    public string[] messages;
    public AudioClip[] audioClips;
    public float initialDelay = 2f;

    private int currentMessageIndex = 0;

    private void Start()
    {
        StartCoroutine(DisplayMessages());
    }

    private IEnumerator DisplayMessages()
    {
        yield return new WaitForSeconds(initialDelay);

        foreach (string message in messages)
        {
            AudioClip audioClip = (audioClips != null && currentMessageIndex < audioClips.Length)
                ? audioClips[currentMessageIndex]
                : null;

            NarrationManager.Instance.ShowText(message, audioClip);

            yield return new WaitForSeconds(NarrationManager.Instance.fadeDuration * 2 + NarrationManager.Instance.displayDuration);

            currentMessageIndex++;
        }
    }
}
