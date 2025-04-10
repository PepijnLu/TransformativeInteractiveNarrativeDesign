using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    [SerializeField] AudioSource pausePlay;
    public void StartGame()
    {
        StartCoroutine(StartGameCR());
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameCR());
    }

    IEnumerator StartGameCR()
    {
        pausePlay.Play();
        while(pausePlay.isPlaying) yield return null;
        SceneManager.LoadScene("House");
    }

    IEnumerator QuitGameCR()
    {
        pausePlay.Play();
        while(pausePlay.isPlaying) yield return null;
        Application.Quit();
    }
}
