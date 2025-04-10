using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public float rotationDuration = 2f; // Time it takes to rotate
    private bool isRotating = false;
    void IInteractable.OnInteract()
    {
        if (!isRotating) StartCoroutine(OpenDoor());   
    }

    IEnumerator OpenDoor()
    {
        Debug.Log("Start opening door");
        isRotating = true;

        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(-90, 0, -90f); // Rotate to 90 degrees on Y axis

        float elapsedTime = 0f;

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
    }
}

