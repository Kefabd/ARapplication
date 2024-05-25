using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Globalization;

public class SendFrames : MonoBehaviour
{
<<<<<<< HEAD
=======

>>>>>>> 2d7db4418479da194d9eb3ca9be88de4f3ceed3c
    public Texture2D imageTarget;
    public GameObject objectToPlace;  // The 3D object to place on the marker

    private void Start()
    {
        if (imageTarget != null)
        {
            byte[] imageTargetData = imageTarget.EncodeToPNG();
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

<<<<<<< HEAD
   IEnumerator SendImageToServer(byte[] imageData, string url)
{
    WWWForm form = new WWWForm();
    form.AddBinaryData("image", imageData, "captured_image.png", "image/png");
=======

>>>>>>> 2d7db4418479da194d9eb3ca9be88de4f3ceed3c

    UnityWebRequest request = UnityWebRequest.Post(url, form);
    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("Failed to send image to server: " + request.error);
    }
    else
    {
        string responseText = request.downloadHandler.text;
        if (!string.IsNullOrEmpty(responseText))
        {
            Response jsonResponse = JsonUtility.FromJson<Response>(responseText);
            Debug.Log("Response message: " + jsonResponse.message);
            if (!jsonResponse.message.Equals("No marker detected"))
            {
                Vector2[] markerCoordinates = ParseCoordinates(jsonResponse.message);
                Debug.Log("Coordonnées :");
                for (int i = 0; i < markerCoordinates.Length; i++)
                {
                    Debug.Log("Point " + i + ": " + markerCoordinates[i]);
                }

                PlaceObjectOnMarker(markerCoordinates);
            }
        }
        else
        {
<<<<<<< HEAD
=======
            Debug.Log("Image sent successfully!");
            // Handle the response from the server
            string responseText = request.downloadHandler.text;
            HandleServerResponse(responseText);
        }
    }

            


    private void HandleServerResponse(string responseText)
    {
        if (!string.IsNullOrEmpty(responseText))
        {
            // Parse the JSON response
            Response jsonResponse = JsonUtility.FromJson<Response>(responseText);
            Debug.Log(jsonResponse.message + ": " + jsonResponse.coordinates);
        }
        else
        {
>>>>>>> 2d7db4418479da194d9eb3ca9be88de4f3ceed3c
            Debug.LogError("Empty response from the server.");
        }
    }
}

<<<<<<< HEAD



Vector2[] ParseCoordinates(string coordinatesString)
{
    Debug.Log("Original coordinates string: " + coordinatesString);

    // Trim the surrounding square brackets
    string trimmedString = coordinatesString.Trim(new char[] { '[', ']' });
    Debug.Log("Trimmed coordinates string: " + trimmedString);

    // Split by "), (" to separate each pair of coordinates
    string[] pairs = trimmedString.Split(new string[] { "), (" }, System.StringSplitOptions.None);

    List<Vector2> coordinates = new List<Vector2>();
    foreach (string pair in pairs)
    {
        // Remove any remaining parentheses
        string cleanedPair = pair.Replace("(", "").Replace(")", "");
        Debug.Log("Cleaned pair: " + cleanedPair);

        // Split by comma to get individual x and y values
        string[] values = cleanedPair.Split(',');
        if (values.Length == 2)
        {
            // Trim any extra whitespace
            string xStr = values[0].Trim();
            string yStr = values[1].Trim();

            Debug.Log("X: " + xStr + ", Y: " + yStr);

            if (float.TryParse(xStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float x) && 
                float.TryParse(yStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                coordinates.Add(new Vector2(x, y));
                Debug.Log($"Parsed coordinates: ({x}, {y})");
            }
            else
            {
                Debug.LogWarning($"Failed to parse coordinates: ({xStr}, {yStr})");
            }
        }
        else
        {
            Debug.LogWarning("Invalid coordinate pair: " + pair);
        }
    }

    return coordinates.ToArray();
}


void PlaceObjectOnMarker(Vector2[] coordinates)
{
    if (coordinates != null && coordinates.Length >= 4)
    {
        Vector2[] quadCoordinates = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            quadCoordinates[i] = coordinates[i];
        }

        Vector3[] worldCoordinates = new Vector3[4];
        for (int i = 0; i < quadCoordinates.Length; i++)
        {
            worldCoordinates[i] = Camera.main.ScreenToWorldPoint(new Vector3(quadCoordinates[i].x, quadCoordinates[i].y, Camera.main.nearClipPlane + 1.0f));
            Debug.Log("World Coordinate " + i + ": " + worldCoordinates[i]);
        }

        Vector3 center = (worldCoordinates[0] + worldCoordinates[1] + worldCoordinates[2] + worldCoordinates[3]) / 4;
        Debug.Log("Center of marker: " + center);

        objectToPlace.transform.position = center;
        objectToPlace.transform.localScale = new Vector3(1, 1, 1); // Adjust scale if necessary
        objectToPlace.SetActive(true); // Ensure the object is active
    }
    else
    {
        Debug.LogError("Insufficient marker coordinates received.");
    }
}


    [System.Serializable]
    public class Response
    {
        public string message;
    }
=======
[System.Serializable]
public class Response
{
    public string message;
    public string coordinates;
>>>>>>> 2d7db4418479da194d9eb3ca9be88de4f3ceed3c
}
