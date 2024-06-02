using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CameraTest : MonoBehaviour
{
    WebCamTexture webcamTexture;
    public Camera mainCamera;
    public RawImage rawImage; // UI element to display the camera feed in the game
    public GameObject objectToPlace; // The 3D object to place in the scene
    private GameObject placedObject;
    private RenderTexture renderTexture; // Component to send images to the server

    void Start()
    {
        // Initialize webcam feed
        webcamTexture = new WebCamTexture();
        if (webcamTexture != null)
        {
            webcamTexture.Play();
        }
        else
        {
            Debug.LogError("Failed to initialize WebCamTexture.");
            return;
        }

        renderTexture = new RenderTexture(webcamTexture.width, webcamTexture.height, 24);
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
        // Capture and send the frame to the server at regular intervals
        if (Time.frameCount % 60 == 0) // Example: Send every 60 frames
        {
            CaptureAndSendFrame();
        }
    }

    private void CaptureAndSendFrame()
    {
        mainCamera.Render();
        RenderTexture.active = renderTexture;

        // Check if the webcamTexture is playing
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

        // Send the image to the server
        StartCoroutine(SendImageToServer(bytes));
        
        // Cleanup
        Destroy(texture);
        mainCamera.targetTexture = null;
    }

    private IEnumerator SendImageToServer(byte[] imageBytes)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, "frame.png", "image/png");

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/app/detect_features/", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + www.error);
        }
        else
        {
            string jsonResponse = www.downloadHandler.text;
            ProcessResponse(jsonResponse);
        }
    }

    private void ProcessResponse(string jsonResponse)
    {
        KeypointResponse response = JsonUtility.FromJson<KeypointResponse>(jsonResponse);
        if (response != null && response.keypoints != null && response.keypoints.Length > 0)
        {
            // Example: Place object at the first detected keypoint
            Vector2 keyPoint = new Vector2(response.keypoints[0].x, response.keypoints[0].y);
            PlaceObject(keyPoint);
        }
    }

    private void PlaceObject(Vector2 screenPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane + 1.0f));
        worldPosition.z = -8.8f; // Adjust depth as needed

        if (placedObject == null)
        {
            Quaternion rotation = Quaternion.Euler(250, 0, 0);
            placedObject = Instantiate(objectToPlace, worldPosition, rotation);
            placedObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // Adjust scale if necessary
        }
        else
        {
            placedObject.transform.position = worldPosition;
        }
    }

    [System.Serializable]
    public class Keypoint
    {
        public int x;
        public int y;
    }

    [System.Serializable]
    public class KeypointResponse
    {
        public Keypoint[] keypoints;
    }
}
