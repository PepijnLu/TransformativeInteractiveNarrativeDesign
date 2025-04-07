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
    private Queue<Texture2D> framesToSave = new Queue<Texture2D>(); // Queue to store frames that need saving
    public int maxFrames; // maximum number of frames you want to record in one video
    public int frameRate = 30; // number of frames to capture per second

    private Thread encoderThread;

    // Texture Readback Objects
    private RenderTexture tempRenderTexture;
    private Texture2D tempTexture2D;

    // Timing Data
    private float captureFrameTime;
    private float lastFrameTime;
    private int frameNumber;
    private int savingFrameNumber;

    // Encoder Thread Shared Resources
    private Queue<byte[]> frameQueue;
    private string persistentDataPath;
    private int screenWidth;
    private int screenHeight;
    private bool threadIsProcessing;
    private bool terminateThreadWhenDone;
    private Camera thisCamera;
    bool isDone;
    private CommandBuffer commandBuffer;
    [SerializeField] RewindPlayback rewindPlayback;

    void Start()
    {
        thisCamera = GetComponent<Camera>();

        // Prepare the data directory
        persistentDataPath = Application.streamingAssetsPath + "/ScreenRecorder";
        if (!System.IO.Directory.Exists(persistentDataPath))
        {
            System.IO.Directory.CreateDirectory(persistentDataPath);
        }

        // Prepare textures and initial values
        screenWidth = thisCamera.pixelWidth;
        screenHeight = thisCamera.pixelHeight;

        tempRenderTexture = new RenderTexture(screenWidth, screenHeight, 24);
        tempTexture2D = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false);
        frameQueue = new Queue<byte[]>();

        captureFrameTime = 1.0f / (float)frameRate;
        lastFrameTime = Time.time;

        // Kill the encoder thread if running from a previous execution
        if (encoderThread != null && (threadIsProcessing || encoderThread.IsAlive))
        {
            threadIsProcessing = false;
            encoderThread.Join();
        }

        // Start a new encoder thread
        threadIsProcessing = true;
        encoderThread = new Thread(EncodeAndSave);
        encoderThread.Start();

        // Set up the command buffer to capture the camera render
        commandBuffer = new CommandBuffer { name = "ScreenRecorderCommandBuffer" };
        commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, tempRenderTexture);
        thisCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);

        // Assign the RenderTexture to the camera
        thisCamera.targetTexture = tempRenderTexture;
    }

    void OnDisable()
    {
        // Reset target frame rate
        Application.targetFrameRate = -1;

        // Inform thread to terminate when finished processing frames
        terminateThreadWhenDone = true;

        // Clean up command buffer
        thisCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);

        // Ensure the encoding thread is complete before proceeding with cleanup
        if (encoderThread != null && encoderThread.IsAlive)
        {
            encoderThread.Join(); // Wait for the encoder thread to finish
        }
    }

    void Update()
    {
        if (frameNumber <= maxFrames)
        {
            // Calculate number of video frames to produce from this game frame
            float thisFrameTime = Time.time;
            int framesToCapture = ((int)(thisFrameTime / captureFrameTime)) - ((int)(lastFrameTime / captureFrameTime));

            // Capture the frame
            if (framesToCapture > 0)
            {
                // Check if the RenderTexture has been updated
                RenderTexture.active = tempRenderTexture;
                tempTexture2D.ReadPixels(new Rect(0, 0, screenWidth, screenHeight), 0, 0);
                tempTexture2D.Apply(); // Apply the changes to texture2D

                // Add the captured data to the queue
                for (int i = 0; i < framesToCapture && frameNumber <= maxFrames; ++i)
                {
                    if(!rewindPlayback.savedStartPos) rewindPlayback.GetStartPosition();
                    framesToSave.Enqueue(tempTexture2D); // Store the entire Texture2D
                    frameNumber++;
                }
            }

            lastFrameTime = thisFrameTime;
            ProcessFrameSaving();
        }
        else
        {
            terminateThreadWhenDone = true;
            this.enabled = false;
            rewindPlayback.StartCoroutine(rewindPlayback.LoadImages());
        }
    }

    private void EncodeAndSave()
    {
        while (threadIsProcessing)
        {
            if (framesToSave.Count > 0)
            {
                // Generate file path for PNG
                string path = persistentDataPath + "/frame" + savingFrameNumber + ".png";
                var frame = framesToSave.Dequeue();  // Dequeue the frame first to avoid modifying the queue in the background thread

                // Add frame to the save queue
                framesToSave.Enqueue(frame);

                // Done
                savingFrameNumber++;
            }
            else
            {
                if (terminateThreadWhenDone)
                {
                    break;
                }

                Thread.Sleep(1);
            }
        }

        terminateThreadWhenDone = false;
        threadIsProcessing = false;
    }

    private void ProcessFrameSaving()
    {
        if(framesToSave.Count > 0)
        {
            Texture2D frame = framesToSave.Dequeue();
            SaveFrameAsPng(frame);
        }
    }

    private void SaveFrameAsPng(Texture2D frame)
    {
        try
        {
            // Encode the texture to PNG
            byte[] pngData = frame.EncodeToPNG();

            // Save the PNG to the file
            string path = persistentDataPath + "/frame" + savingFrameNumber + ".png";
            File.WriteAllBytes(path, pngData);
            savingFrameNumber++;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error saving frame: {ex.Message}");
        }
    }

    // Cleanup method that ensures files are deleted after the encoding thread finishes
    private void CleanupScreenRecorderDirectory()
    {
        // Ensure the encoding thread has completed before attempting cleanup
        if (frameQueue.Count == 0 && !threadIsProcessing)
        {
            // Delete the ScreenRecorder directory and its contents
            if (Directory.Exists(persistentDataPath))
            {
                try
                {
                    // Delete all files inside the directory
                    string[] files = Directory.GetFiles(persistentDataPath);
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }

                    // Optionally, delete the directory itself
                    Directory.Delete(persistentDataPath, true);
                    Debug.Log("ScreenRecorder directory cleared.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error while clearing the ScreenRecorder directory: " + e.Message);
                }
            }
        }
        else
        {
            Debug.LogWarning("Encoder thread is still processing, directory will be cleared later.");
        }
    }

    // Optional: Editor callback to ensure cleanup when exiting play mode in the editor
    [UnityEditor.InitializeOnLoadMethod]
    private static void OnPlayModeChanged()
    {
        // Register for play mode state changes
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    // Handle PlayMode state change
    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Cleanup ScreenRecorder directory when exiting play mode in the editor
            var screenRecorder = FindFirstObjectByType<ScreenRecorder>();
            if (screenRecorder != null)
            {
                //screenRecorder.CleanupScreenRecorderDirectory();
            }
        }
    }
}




