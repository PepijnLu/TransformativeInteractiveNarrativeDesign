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
    [SerializeField] ScreenRecorder screenRecorder;
    public bool savedStartPos, rewinding;


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
        rewinding = true;
        GameManager.instance.playerCanMove = false;
        rewindSound.Play();
        yield return new WaitForSeconds(1.26f);
        imageToChange.gameObject.SetActive(true);

        int totalFrames = GameManager.instance.screenRecorder.framesToSave.Count;
        float timeStep = 6.15f / totalFrames;
        int imagesLoaded = 0;

        Debug.Log($"Rewind Time: Goal: {6.15}, TimeStep: {timeStep}, Estimated Total Time: {timeStep * totalFrames}");
        float elapsedTime = 0;

        while(imagesLoaded < totalFrames)
        {
            imageToChange.texture = GameManager.instance.screenRecorder.framesToSave.Pop();
            imagesLoaded++;
            yield return new WaitForSeconds(timeStep);
            elapsedTime += timeStep;
        }

        Debug.Log($"Rewind Time: Elapsed Time: {elapsedTime}");

        GameManager.instance.screenRecorder.framesToSave.Clear();
        player.transform.position = startPosition;
        player.transform.rotation = startRotation;  
        savedStartPos = false;
        GameManager.instance.ResetNonPlayerObjects();
        imageToChange.gameObject.SetActive(false);
        GameManager.instance.playerCanMove = true;
        screenRecorder.stopRecording = false;
        StartCoroutine(screenRecorder.SaveFrames());
        rewinding = false;
    }

    IEnumerator RewindFlicker()
    {
        while(true)
        {
            if(rewinding)
            {
                yield return new WaitForSeconds(rewindFlickerSpeed);
                rewindObject.SetActive(!rewindObject.activeSelf);
            }
            else
            {
                if(rewindObject.activeSelf) rewindObject.SetActive(false);
            }

            yield return null;
        }
    }
}
