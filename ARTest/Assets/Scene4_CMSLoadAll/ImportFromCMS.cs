using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImportFromCMS : MonoBehaviour
{
    public PlaceObjectsCMS placeARObject;
    public static Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    IEnumerator Start()
    {
        string apiURL = "https://popar-backend.acstech.vn/api/v3/experiences"; // Replace with your API URL
        UnityWebRequest www = UnityWebRequest.Get(apiURL);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("API request failed: " + www.error);
        }
        else
        {
            Debug.Log("Successfully received API response");
            var jsonResponse = JSON.Parse(www.downloadHandler.text);
            var data = jsonResponse["data"].AsArray;
            int counter = 1;
            foreach (JSONNode item in data)
            {
                string bundleLink;
                string markerLink = item["ar_image"];

                // Get the filename of the bundle image for the current platform
                string filename = item["name_file_apple_image"];
                if (Application.platform == RuntimePlatform.Android)
                {
                    filename = item["name_file_ch_play_image"];
                }

                // Construct the bundle link
                bundleLink = "http://popar-backend.acstech.vn/filename=" + filename + "?bucket=projects";

                Debug.Log("Bundle link: " + bundleLink);
                Debug.Log("Marker link: " + markerLink);

                // Now you can use these links to download and import your asset bundles
                StartCoroutine(DownloadAndCacheAssetBundle(bundleLink, counter.ToString()));
                StartCoroutine(DownloadAndCacheAssetBundle(markerLink, counter.ToString() + ".png"));

                counter++;
            }
        }
    }

    IEnumerator DownloadAndCacheAssetBundle(string url, string fileName)
    {
        // Replace 'http' with 'https' in the URL
        url = url.Replace("http://", "https://");

        string localPath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(localPath))
        {
            Debug.Log("Loading file from cache: " + localPath);
            string extension = Path.GetExtension(localPath); // Get the file extension
            if (extension == ".png")
            {
                // Load and process regular image file
                byte[] imageData = File.ReadAllBytes(localPath); // Read the image data from the file
                Texture2D texture = new Texture2D(2, 2); // Create a new texture
                texture.LoadImage(imageData); // Load the image data into the texture
                Debug.Log("Loading image: " + fileName);
                AddImageToReferenceLibrary(texture, fileName); // Add the image to the reference library
            }
            else
            {
                // Load and process AssetBundle
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(localPath); // Load the AssetBundle asynchronously
                yield return request; // Wait for the request to finish

                if (request.assetBundle == null)
                {
                    Debug.LogError("Failed to load AssetBundle from cache");
                }
                else
                {
                    AddAssetsToPlaceARObject(request.assetBundle, fileName); // Add the assets to the place AR object
                }
            }
        }
        else
        {
            if (url.Contains(".png"))
            {
                // Download and process regular image file
                UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download image: " + request.error);
                }
                else
                {
                    Debug.Log("Successfully downloaded image");
                    File.WriteAllBytes(localPath, request.downloadHandler.data);

                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    AddImageToReferenceLibrary(texture, fileName);
                }
            }
            else
            {
                // Download and process AssetBundle
                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download AssetBundle: " + request.error);
                }
                else
                {
                    Debug.Log("Successfully downloaded AssetBundle");
                    File.WriteAllBytes(localPath, request.downloadHandler.data);

                    AssetBundle bundle = AssetBundle.LoadFromMemory(request.downloadHandler.data);
                    AddAssetsToPlaceARObject(bundle, fileName);
                }
            }
        }
    }


    void AddAssetsToPlaceARObject(AssetBundle bundle, string fileName)
    {
        // Use GetAllAssetNames()
        string[] assetNames = bundle.GetAllAssetNames();
        foreach (string name in assetNames)
        {
            Debug.Log("Asset Name: " + name);
            GameObject prefab = bundle.LoadAsset<GameObject>(name);
            string key = fileName.Replace(".png", "");
            // After loading a prefab from the AssetBundle:
            prefabDictionary.Add(key, prefab);
        }

        // Log the list of prefab names
        foreach (var key in prefabDictionary.Keys)
        {
            Debug.Log("Prefabs: " + key);
        }
    }

    void AddImageToReferenceLibrary(Texture2D image, string name)
    {
        // Get the reference image library
        var imageManager = GetComponent<ARTrackedImageManager>();
        var mutableLibrary = (imageManager.referenceLibrary as MutableRuntimeReferenceImageLibrary);

        // Add the image to the library
#if UNITY_ANDROID && !UNITY_EDITOR
    mutableLibrary.ScheduleAddImageJob(image, name, 0.1f);
#endif

        // Log the contents of the reference image library
        Debug.Log("Reference Image Library:");
        for (int i = 0; i < mutableLibrary.count; i++)
        {
            var referenceImage = mutableLibrary[i];
            Debug.Log("Image " + i + ": " + referenceImage.name);
        }

    }


}