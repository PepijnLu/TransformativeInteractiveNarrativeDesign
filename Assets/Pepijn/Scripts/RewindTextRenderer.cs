using UnityEngine;

public class RewindTextRenderer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        rt.useMipMap = false;
        rt.autoGenerateMips = false;
        rt.Create();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
