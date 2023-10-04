using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Import : MonoBehaviour
{
    public PlaceARObject placeARObject;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        string path = "https://popar-backend.acstech.vn/filename=deer?bucket=projects";
        Debug.Log("Loading AssetBundle from: " + path);
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to load AssetBundle: " + request.error);
        }
        else
        {
            Debug.Log("Successfully loaded AssetBundle");
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            // Use GetAllAssetNames()
            string[] assetNames = bundle.GetAllAssetNames();
            foreach (string name in assetNames)
            {
                Debug.Log("Asset Name: " + name);
                GameObject prefab = bundle.LoadAsset<GameObject>(name);
                // After loading a prefab from the AssetBundle:
                placeARObject.ArPrefabs.Add(prefab);
            }
        }
    }
}
