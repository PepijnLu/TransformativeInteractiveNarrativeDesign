using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public float rotationDuration = 2f; // Time it takes to rotate
    private bool isRotating = false;
    [SerializeField] bool exitDoor;
    [SerializeField] AudioSource doorSfx;
    public bool isOpen;
    void IInteractable.OnInteract()
    {
        if (!isRotating) StartCoroutine(OpenDoor());   
    }

    IEnumerator OpenDoor()
    {
        Debug.Log("Start opening door");
        isRotating = true;

        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation;
        if(isOpen) targetRotation = Quaternion.Euler(-90, 0, 0);
        else targetRotation = Quaternion.Euler(-90, 0, -90f);

        isOpen = !isOpen;

        float elapsedTime = 0f;
        doorSfx.Play();
        while (elapsedTime < rotationDuration)
        {
            Quaternion desiredRotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationDuration);
            transform.localRotation = desiredRotation;
            elapsedTime += .02f;
            yield return new WaitForFixedUpdate();
        }

        transform.localRotation = targetRotation; // Ensure exact final rotation
        isRotating = false;
        Debug.Log("Finish opening door");

        if(exitDoor) 
        {
            if(GameManager.instance.interactedThisLoop) StartCoroutine(StartRewind());
            else StartCoroutine(GameManager.instance.TriggerEnding(4f));
        }
    }

    IEnumerator StartRewind()
    {
        GameManager.instance.playerCanMove = false;
        yield return new WaitForSeconds(1);
        ScreenRecorder screenRecorder = FindFirstObjectByType<ScreenRecorder>();
    
        if(screenRecorder.recording) screenRecorder.stopRecording = true;
        else throw new System.Exception("Camera wasnt recording");
        GameManager.instance.loopCount++;
    }
}

