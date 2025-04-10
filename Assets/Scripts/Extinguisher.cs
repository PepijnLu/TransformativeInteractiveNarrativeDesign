using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extinguisher : MonoBehaviour
{
    private float extinguishDistance = 5f;
    private float lookThreshold = 0.4f;
    private float extinguishTimer = 0f;
    private float timeToExtinguish = 2f;
    [SerializeField] private GameObject currentFireTarget = null;
    public Transform playerCamera, playerTransform;

    public AudioSource extinguishSound, objectiveSound;

    private void Update()
    {

        if (GameManager.instance.fireExtinguisherEquipped)
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
                        GameManager.instance.interactedThisLoop = true;
                        GameObject fire = GameObject.FindGameObjectWithTag("Fire");
                        if(fire == null) objectiveSound.Play();
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

    public void SetCorrectParent()
    {
        if(!GameManager.instance.fireExtinguisherEquipped)
        {
            Debug.Log("FireExt equip");
            transform.SetParent(playerTransform);
            transform.position = playerTransform.position + (playerTransform.transform.forward * 1) + (playerTransform.transform.right * 1f) + (-playerTransform.transform.up * .5f);
            GameManager.instance.fireExtinguisherEquipped = true;
        }
        else
        {
            Debug.Log("FireExt deequip");
            transform.SetParent(null);
            GameManager.instance.fireExtinguisherEquipped = false;
        }
    }
}