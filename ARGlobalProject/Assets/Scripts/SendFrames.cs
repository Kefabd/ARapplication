using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SendFrames : MonoBehaviour
{
    // URL of the Django server endpoint to receive the image data
    private string serverURL = "http://127.0.0.1:8000/app/upload-image/";

    private void Start()
    {
        
    }

    public void SendImage(byte[] imageData)
    {
        StartCoroutine(SendImageToServer(imageData));
    }

    IEnumerator SendImageToServer(byte[] imageData)
    {
        // Create a new form to send the image data
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, "captured_image.png", "image/png");

        // Send the form to the server
        UnityWebRequest request = UnityWebRequest.Post(serverURL, form);
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
