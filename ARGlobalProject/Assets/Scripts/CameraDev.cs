using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CameraDev : MonoBehaviour
{
    WebCamTexture webcamTexture;
    public Canvas canvasPrefab;
    private SendFrames imageSender;

    void Start()
    {
        Canvas canvasInstance = Instantiate(canvasPrefab);
        // Get the SendFrames component attached to a GameObject in the scene
        imageSender = GetComponent<SendFrames>();

        // Get the RawImage component from the Canvas instance
        RawImage rawImage = canvasInstance.GetComponentInChildren<RawImage>();
        if (rawImage != null)
        {
            // Initialize and start the webcam texture
            webcamTexture = new WebCamTexture();
            rawImage.texture = webcamTexture;
            webcamTexture.Play();
            rawImage.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            Debug.LogError("RawImage component not found in the instantiated Canvas.");
        }
    }

    void Update()
    {
        // Check if the webcam texture is playing and has valid dimensions
        if (webcamTexture != null && webcamTexture.width > 0 && webcamTexture.height > 0)
        {
            // Create a new Texture2D to store the camera frame
            Texture2D texture = new Texture2D(webcamTexture.width, webcamTexture.height);

            // Read pixels from the webcam texture into the Texture2D
            texture.SetPixels(webcamTexture.GetPixels());
            texture.Apply();

            // Convert the Texture2D to a byte array in PNG format
            byte[] bytes = texture.EncodeToPNG();
            imageSender.SendImage(bytes);
            
        }
    }
}