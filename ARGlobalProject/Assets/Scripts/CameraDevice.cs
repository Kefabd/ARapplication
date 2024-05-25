using UnityEngine;
using UnityEngine.UI;

public class CameraDevice : MonoBehaviour
{
    WebCamTexture webcamTexture;
    public Camera mainCamera;
    public RawImage rawImage; // UI element to display the camera feed in the game
    private SendFrames imageSender; 
    
    RenderTexture renderTexture;// Component to send images to the server

    void Start()
    {
        // Initialize webcam feed
        webcamTexture = new WebCamTexture();
        imageSender = GetComponent<SendFrames>();
        if (webcamTexture != null)
        {
            webcamTexture.Play();
        }
        else
        {
            Debug.LogError("Failed to initialize WebCamTexture.");
            return;
        }
        RenderTexture renderTexture = new RenderTexture(webcamTexture.width, webcamTexture.height, 24);
        mainCamera.targetTexture = renderTexture;

        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = webcamTexture;

        // Create a fullscreen quad and apply the material
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.position = mainCamera.transform.position + mainCamera.transform.forward * 1.2f;
        quad.transform.rotation = mainCamera.transform.rotation;
        quad.transform.localScale = new Vector3(mainCamera.aspect * 1.5f, 1.5f, 1f);
        quad.GetComponent<Renderer>().material = material;

        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        mainCamera.targetTexture = renderTexture;

        // Set the webcam texture to the RawImage UI element
        if (rawImage != null)
        {
            rawImage.texture = webcamTexture;
        }
        else
        {
            Debug.LogError("RawImage component not assigned!");
        }
    }

    void Update()
    {
        // Capture and send the frame to the server at regular intervals or based on some condition
        if (Time.frameCount % 60 == 0) // Example: Send every 60 frames
        {
            CaptureAndSendFrame();
        }
    }

    private void CaptureAndSendFrame()
    {
        // Check if the webcamTexture is playing
        mainCamera.Render();
        RenderTexture.active = renderTexture;
        if (webcamTexture == null || !webcamTexture.isPlaying)
        {
            Debug.LogError("Webcam texture is not playing.");
            return;
        }

        // Create a new Texture2D to store the camera frame
        Texture2D texture = new Texture2D(webcamTexture.width, webcamTexture.height);
        if (texture == null)
        {
            Debug.LogError("Failed to create Texture2D.");
            return;
        }

        // Read pixels from the webcam texture into the Texture2D
        texture.SetPixels(webcamTexture.GetPixels());
        texture.Apply();

        // Convert the Texture2D to a byte array in PNG format
        byte[] bytes = texture.EncodeToPNG();

        // Send the image using the SendFrames component
        if (imageSender != null)
        {
            imageSender.SendImage(bytes);
        }
        else
        {
            Debug.LogError("SendFrames component not assigned!");
        }

        // Cleanup
        Destroy(texture);
        mainCamera.targetTexture = null;
    }
}
