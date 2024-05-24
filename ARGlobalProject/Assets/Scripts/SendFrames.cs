using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SendFrames : MonoBehaviour
{
    
    public Texture2D imageTarget;


    private void Start()
    {
        // Check if an image target is assigned
        if (imageTarget != null)
        {
            // Convert the Texture2D to a byte array
            byte[] imageTargetData = imageTarget.EncodeToPNG();

            // Send the image target data to the server
            SendImageTarget(imageTargetData);
        }
        else
        {
            Debug.LogWarning("No image target assigned. Please assign an image target texture.");
        }
    }

    public void SendImageTarget(byte[] imageTargetData)
    {
        StartCoroutine(SendImageToServer(imageTargetData, "http://127.0.0.1:8000/app/image_target/"));
    }


    public void SendImage(byte[] imageData)
    {
        StartCoroutine(SendImageToServer(imageData, "http://127.0.0.1:8000/app/frames/"));
    }

    

    IEnumerator SendImageToServer(byte[] imageData, string url)
    {
        // Create a new form to send the image data
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, "captured_image.png", "image/png");

        // Send the form to the server
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to send image to server: " + request.error);
        }
        else
        {
            Debug.Log("Image sent successfully!");
            // Handle the response from the server
            string responseText = request.downloadHandler.text;
            if (!string.IsNullOrEmpty(responseText))
            {
                // Parse the JSON response
                Response jsonResponse = JsonUtility.FromJson<Response>(responseText);

                Debug.Log(jsonResponse.message);
            }
            else
            {
                Debug.LogError("Empty response from the server.");
            }

        }
    }
}

public class Response
{
    public string message;
}
