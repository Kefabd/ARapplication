using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Globalization;

public class SendFrames : MonoBehaviour
{
    public Texture2D imageTarget;
    public GameObject objectToPlace;  // The 3D object to place on the marker
    private GameObject instantiatedObject; // Reference to the instantiated object

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

    private IEnumerator SendImageToServer(byte[] imageData, string url)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, "image.png", "image/png");

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
                    Vector2[] markerCoordinates = ParseCoordinates(jsonResponse.coordinates);
                    PlaceObjectOnMarker(markerCoordinates);
                }
                else
                {
                    DestroyInstantiatedObject();
                }
            }
            else
            {
                Debug.Log("Image sent successfully!");
                HandleServerResponse(responseText);
            }
        }
    }

    private void HandleServerResponse(string responseText)
    {
        if (!string.IsNullOrEmpty(responseText))
        {
            Response jsonResponse = JsonUtility.FromJson<Response>(responseText);
            Debug.Log(jsonResponse.message + ": " + jsonResponse.coordinates);
        }
        else
        {
            Debug.LogError("Empty response from the server.");
        }
    }

    Vector2[] ParseCoordinates(string coordinatesString)
    {
        Debug.Log("Original coordinates string: " + coordinatesString);

        string trimmedString = coordinatesString.Trim(new char[] { '[', ']' });
        Debug.Log("Trimmed coordinates string: " + trimmedString);

        string[] pairs = trimmedString.Split(new string[] { "), (" }, System.StringSplitOptions.None);

        List<Vector2> coordinates = new List<Vector2>();
        foreach (string pair in pairs)
        {
            string cleanedPair = pair.Replace("(", "").Replace(")", "");
            Debug.Log("Cleaned pair: " + cleanedPair);

            string[] values = cleanedPair.Split(',');

            if (values.Length == 2)
            {
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
            Vector2[] quadCoordinates = new Vector2[coordinates.Length];
            for (int i = 0; i < coordinates.Length; i++)
            {
                quadCoordinates[i] = coordinates[i];
            }

            Vector3[] worldCoordinates = new Vector3[coordinates.Length];
            for (int i = 0; i < quadCoordinates.Length; i++)
            {
                worldCoordinates[i] = Camera.main.ScreenToWorldPoint(new Vector3(quadCoordinates[i].x, quadCoordinates[i].y, Camera.main.nearClipPlane + 1.0f));

                // worldCoordinates[i].x -= 1.0f;
                worldCoordinates[i].z = -8.8f;
                Debug.Log("World Coordinate " + i + ": " + worldCoordinates[i]);
            }

            Vector3 center = Vector3.zero;
            for (int i = 0; i < worldCoordinates.Length; i++)
            {
                center += worldCoordinates[i];
            }
            center /= worldCoordinates.Length;
            Debug.Log("Center of marker: " + center);

            if (instantiatedObject == null)
            {
                Quaternion rotation = Quaternion.Euler(250, 0, 0);
                instantiatedObject = Instantiate(objectToPlace, center, rotation);
                instantiatedObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // Adjust scale if necessary
                instantiatedObject.SetActive(true); // Ensure the object is active
            }
            else
            {
                instantiatedObject.transform.position = center;
                // Update rotation if necessary
            }
        }
        else
        {
            Debug.LogError("Insufficient marker coordinates received.");
        }
    }
    void DestroyInstantiatedObject()
    {
        if (instantiatedObject != null)
        {
            Destroy(instantiatedObject);
            instantiatedObject = null;
        }
    }
}

[System.Serializable]
public class Response
{
    public string message;
    public string coordinates;
}
