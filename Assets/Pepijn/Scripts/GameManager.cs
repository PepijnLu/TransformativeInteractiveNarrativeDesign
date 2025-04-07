using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public ScreenRecorder screenRecorder;
    public static GameManager instance;
    public bool playerCanMove;
    public int savedFrames;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;   
    }
    void Start()
    {
        GameManager.instance.playerCanMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Backspace))
        {
            if(screenRecorder.recording) screenRecorder.stopRecording = true;
        }
    }
}
