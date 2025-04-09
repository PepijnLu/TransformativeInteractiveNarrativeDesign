using System;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

class BitmapEncoder
{
    public static void WriteBitmap(Stream stream, int width, int height, byte[] imageData)
    {
        using (BinaryWriter bw = new BinaryWriter(stream))
        {
            // Define the bitmap file header
            bw.Write((UInt16)0x4D42);                             // bfType;
            bw.Write((UInt32)(14 + 40 + (width * height * 4)));    // bfSize;
            bw.Write((UInt16)0);                                   // bfReserved1;
            bw.Write((UInt16)0);                                   // bfReserved2;
            bw.Write((UInt32)14 + 40);                             // bfOffBits;

            // Define the bitmap information header
            bw.Write((UInt32)40);                                  // biSize;
            bw.Write((Int32)width);                                // biWidth;
            bw.Write((Int32)height);                               // biHeight;
            bw.Write((UInt16)1);                                   // biPlanes;
            bw.Write((UInt16)32);                                  // biBitCount;
            bw.Write((UInt32)0);                                   // biCompression;
            bw.Write((UInt32)(width * height * 4));                // biSizeImage;
            bw.Write((Int32)0);                                    // biXPelsPerMeter;
            bw.Write((Int32)0);                                    // biYPelsPerMeter;
            bw.Write((UInt32)0);                                   // biClrUsed;
            bw.Write((UInt32)0);                                   // biClrImportant;

            // Switch the image data from RGB to BGR
            for (int imageIdx = 0; imageIdx < imageData.Length; imageIdx += 3)
            {
                bw.Write(imageData[imageIdx + 2]);
                bw.Write(imageData[imageIdx + 1]);
                bw.Write(imageData[imageIdx + 0]);
                bw.Write((byte)255);
            }
        }
    }
}

[RequireComponent(typeof(Camera))]
public class ScreenRecorder : MonoBehaviour
{
    public Stack<Texture2D> framesToSave = new Stack<Texture2D>(); // Queue to store frames that need saving
    public int frameRate; // number of frames to capture per second

    // Texture Readback Objects
    private RenderTexture tempRenderTexture;

    // Timing Data
    private float captureFrameTime;
    private float lastFrameTime;
    private int savingFrameNumber = 1;
    private int screenWidth;
    private int screenHeight;
    private Camera thisCamera;
    private CommandBuffer commandBuffer;
    [SerializeField] RewindPlayback rewindPlayback;
    public bool stopRecording, recording;

    void Start()
    {
        thisCamera = GetComponent<Camera>();


        // Prepare textures and initial values
        screenWidth = thisCamera.pixelWidth;
        screenHeight = thisCamera.pixelHeight;

        tempRenderTexture = new RenderTexture(screenWidth, screenHeight, 24);

        captureFrameTime = 1.0f / (float)frameRate;
        lastFrameTime = Time.time;

        // Set up the command buffer to capture the camera render
        commandBuffer = new CommandBuffer { name = "ScreenRecorderCommandBuffer" };
        commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, tempRenderTexture);
        thisCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);

        // Assign the RenderTexture to the camera
        thisCamera.targetTexture = tempRenderTexture;

        StartCoroutine(SaveFrames());
    }

    void OnDisable()
    {
        // Reset target frame rate
        Application.targetFrameRate = -1;

        // Clean up command buffer
        thisCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    }

    IEnumerator SaveFrames()
    {
        recording = true;
        while(true)
        {
            if(stopRecording)
            {
                stopRecording = false;
                break;
            }

            // Calculate number of video frames to produce from this game frame
            float thisFrameTime = Time.time;
            int framesToCapture = ((int)(thisFrameTime / captureFrameTime)) - ((int)(lastFrameTime / captureFrameTime));

            // Capture the frame
            if (framesToCapture > 0)
            {
                // Check if the RenderTexture has been updated
                RenderTexture.active = tempRenderTexture;

                Texture2D tempTexture2D = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false);
                tempTexture2D.ReadPixels(new Rect(0, 0, screenWidth, screenHeight), 0, 0);
                tempTexture2D.Apply(); // Apply the changes to texture2D

                if(tempTexture2D == null) Debug.Log("Saved Texture is null (update)");

                // Add the captured data to the queue
                for (int i = 0; i < framesToCapture; ++i)
                {
                    if(!rewindPlayback.savedStartPos) rewindPlayback.GetStartPosition();
                    int currentStackSize = framesToSave.Count;
                    framesToSave.Push(tempTexture2D); // Store the entire Texture2D
                    while(currentStackSize == framesToSave.Count) yield return null;
                }
            }

            lastFrameTime = thisFrameTime;
            yield return null;
        }
        recording = false;
        GameManager.instance.savedFrames = savingFrameNumber - 1;
        rewindPlayback.StartCoroutine(rewindPlayback.LoadImages());
    }
}




