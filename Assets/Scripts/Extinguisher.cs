using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extinguisher : MonoBehaviour
{
    public bool isEquipped = false;

    private float extinguishDistance = 5f;
    private float lookThreshold = 0.4f;
    private float extinguishTimer = 0f;
    private float timeToExtinguish = 2f;
    private GameObject currentFireTarget = null;
    public Transform playerCamera = null;

    public AudioSource extinguishSound;

    private void Update()
    {

        if (isEquipped)
        {
            CheckForFireObjects();

            if (Input.GetMouseButton(0))
            {
                if (!extinguishSound.isPlaying)
                {
                    extinguishSound.Play();
                }
                if (currentFireTarget != null)
                {
                    extinguishTimer += Time.deltaTime;

                    if (extinguishTimer >= timeToExtinguish)
                    {
                        currentFireTarget.SetActive(false);
                        extinguishTimer = 0f;
                    }
                }   
            }
            else
            {
                extinguishTimer = 0f;
                extinguishSound.Stop();
            }
        }


    }

    private void CheckForFireObjects()
    {
        Collider[] nearby = Physics.OverlapSphere(playerCamera.position, extinguishDistance);

        float bestMatch = lookThreshold;
        GameObject bestFireTarget = null;

        foreach (var col in nearby)
        {
            if (col.gameObject.CompareTag("Fire"))
            {
                Vector3 dirToObject = (col.bounds.center - playerCamera.position).normalized;
                float lookAlignment = Vector3.Dot(playerCamera.forward, dirToObject);

                if (lookAlignment > bestMatch)
                {
                    bestMatch = lookAlignment;
                    bestFireTarget = col.gameObject;
                }
            }
        }

        currentFireTarget = bestFireTarget;
    }

    public void Equipper() {
        isEquipped = true;
    }
}