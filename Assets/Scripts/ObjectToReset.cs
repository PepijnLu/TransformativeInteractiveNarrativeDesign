using UnityEngine;

public class ObjectToReset : MonoBehaviour
{
    void Start()
    {
        GameManager.instance.objectsToReset.Add(gameObject);
    }
}
