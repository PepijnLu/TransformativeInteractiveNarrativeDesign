using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

struct ObjectData
{
    public GameObject objectRef;
    public Vector3 objectPos;
    public Quaternion objectRot;
}

public class GameManager : MonoBehaviour
{
    [SerializeField] public ScreenRecorder screenRecorder;
    public static GameManager instance;
    public bool playerCanMove;
    public int savedFrames;
    public int loopCount;
    public bool fireExtinguisherEquipped;
    public bool interactedThisLoop;
    public List<GameObject> objectsToReset = new();
    [SerializeField] List<ObjectData> objectDatas = new();
    [SerializeField] GameObject ending;
    [SerializeField] Image endingBG, endingQuote;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;   
    }
    void Start()
    {
        playerCanMove = true;
        StartCoroutine(InitializeObjectsWithDelay());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            //if(screenRecorder.recording) screenRecorder.stopRecording = true;
        }
    }

    IEnumerator InitializeObjectsWithDelay()
    {
        yield return new WaitForSeconds(1f);
        InitliazeObjects();
    }

    public void InitliazeObjects()
    {
        objectDatas.Clear();

        foreach(GameObject obj in objectsToReset)
        {
            ObjectData dataToAdd = new()
            {
                objectRef = obj,
                objectPos = obj.transform.position,
                objectRot = obj.transform.rotation
            };
            objectDatas.Add(dataToAdd);
        }
    }

    public void ResetNonPlayerObjects()
    {
        for(int i = 0; i < objectsToReset.Count; i++)
        {
            GameObject objectToReset = objectsToReset[i];
            objectToReset.SetActive(true);
            objectToReset.transform.position = objectDatas[i].objectPos;
            objectToReset.transform.rotation = objectDatas[i].objectRot;

            if (objectToReset.TryGetComponent<IInteractable>(out var interactable) && interactable is Door door)
            {
                door.isOpen = false;
            }
        }
        fireExtinguisherEquipped = false;
        interactedThisLoop = false;
    }

    public IEnumerator TriggerEnding(float duration)
    {
        playerCanMove = false;
        ending.SetActive(true);

        //Lerp the background in
        Color color = endingBG.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            endingBG.color = new Color(color.r, color.g, color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it's fully opaque at the end
        endingBG.color = new Color(color.r, color.g, color.b, 1f);

        //Lerp the quote in
        color = endingQuote.color;
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            endingQuote.color = new Color(color.r, color.g, color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it's fully opaque at the end
        endingQuote.color = new Color(color.r, color.g, color.b, 1f);

        yield return new WaitForSeconds(10);
        Application.Quit();
    }
}
