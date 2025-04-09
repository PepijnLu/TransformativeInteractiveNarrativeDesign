using UnityEngine;
using TMPro;
using System.Collections;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;

    public TextMeshProUGUI narrationText;
    public AudioSource audioSource;
    public float fadeDuration = 1f;
    public float displayDuration = 3f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowText(string text, AudioClip audioClip = null)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayTextRoutine(text, audioClip));
    }

    private IEnumerator DisplayTextRoutine(string text, AudioClip audioClip)
    {
        narrationText.text = text;

        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }

        float elapsedTime = 0f;
        Color color = narrationText.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            narrationText.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(displayDuration);

        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            narrationText.color = color;
            yield return null;
        }

        narrationText.text = "";

        if (audioClip != null)
        {
            audioSource.Stop();
        }
    }
}
