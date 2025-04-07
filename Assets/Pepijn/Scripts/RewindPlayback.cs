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
    bool rewinding;
    Vector3 startPosition;
    Quaternion startRotation;
    public bool savedStartPos;

    void Start()
    {
        StartCoroutine(RewindFlicker());
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            StartCoroutine(LoadImages());
        }
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
        rewinding = true;
        StartCoroutine(RewindFlicker());
        imageToChange.gameObject.SetActive(true);
        string path = Path.Combine(Application.streamingAssetsPath, folderName);
        
        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path, "*.png");
            int currentIndex = files.Length;
            float timeStep = 6.15f / files.Length;

            while(currentIndex > 0)
            {
                LoadImage(files[currentIndex - 1]);
                currentIndex--;
                yield return new WaitForSeconds(timeStep);
            }
        }
        else
        {
            Debug.LogError("Folder does not exist: " + path);
        }

        player.transform.position = startPosition;
        player.transform.rotation = startRotation;  
        savedStartPos = false;

        imageToChange.gameObject.SetActive(false);
        rewinding = false;
        GameManager.instance.playerCanMove = true;
    }

    IEnumerator RewindFlicker()
    {
        //rewindObject.SetActive(true);
        while(true)
        {
            yield return new WaitForSeconds(rewindFlickerSpeed);
            rewindObject.SetActive(!rewindObject.activeSelf);
        }
        //rewindObject.SetActive(false);
    }

    void LoadImage(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(1920, 1080); // create a texture
        texture.LoadImage(fileData); // load the PNG data into the texture
        imageToChange.texture = texture;
    }
}
