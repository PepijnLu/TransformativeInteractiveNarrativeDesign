using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

public class RewindPlayback : MonoBehaviour
{
    public string folderName = "ScreenRecorder"; // Folder inside StreamingAssets
    public RawImage imageToChange;
    public AudioSource rewindSound;
    [SerializeField] float rewindFlickerSpeed;
    [SerializeField] GameObject rewindObject, player;
    Vector3 startPosition;
    Quaternion startRotation;
    public bool savedStartPos;
    int imagesLoaded;


    void Start()
    {
        StartCoroutine(RewindFlicker());
    }

    void Update()
    {

    }

    public void GetStartPosition()
    {
        startPosition = player.transform.position;
        startRotation = player.transform.rotation;
        savedStartPos = true;
    }

    public IEnumerator LoadImages()
    {
        GameManager.instance.playerCanMove = false;
        rewindSound.Play();
        yield return new WaitForSeconds(1.26f);
        imageToChange.gameObject.SetActive(true);

        int totalFrames = GameManager.instance.screenRecorder.framesToSave.Count;
        float timeStep = 6.15f / totalFrames;

        while(imagesLoaded < totalFrames)
        {
            imageToChange.texture = GameManager.instance.screenRecorder.framesToSave.Pop();
            imagesLoaded++;
            yield return new WaitForSeconds(timeStep);
        }

        player.transform.position = startPosition;
        player.transform.rotation = startRotation;  
        savedStartPos = false;

        imageToChange.gameObject.SetActive(false);
        GameManager.instance.playerCanMove = true;
    }

    IEnumerator RewindFlicker()
    {
        while(true)
        {
            yield return new WaitForSeconds(rewindFlickerSpeed);
            rewindObject.SetActive(!rewindObject.activeSelf);
        }
    }
}
