using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool playerCanMove;
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
        
    }
}
