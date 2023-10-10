using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class PlaceObjectsCMS : MonoBehaviour
{
    public List<Texture2D> ArImages;

    // Reference to AR tracked image manager component
    private ARTrackedImageManager _trackedImagesManager;

    // List of prefabs to instantiate - these should be named the same
    // as their corresponding 2D images in the reference image library 
    public List<GameObject> ArPrefabs; // Changed from array to list

    // Keep dictionary array of created prefabs
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Cache a reference to the Tracked Image Manager component
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        // Attach event handler when tracked images change
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        // Remove event handler
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(5f);  // Wait for ARSession to initialize

        var mutableLibrary = (_trackedImagesManager.referenceLibrary as MutableRuntimeReferenceImageLibrary);

        foreach (var texture in ArImages)
        {
            var jobHandle = mutableLibrary.ScheduleAddImageJob(texture, texture.name, 0.1f);
            yield return jobHandle;  // Wait for the job to complete
        }
    }

    // Event Handler
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Loop through all new tracked images that have been detected
        foreach (var trackedImage in eventArgs.added)
        {
            Debug.Log("Tracked image added: " + trackedImage.referenceImage.name);

            string key = trackedImage.referenceImage.name.Replace(".png", "");
            if (ImportFromCMS.prefabDictionary.ContainsKey(key))
            {
                // Instantiate the prefab and parent it to the tracked image
                GameObject prefab = ImportFromCMS.prefabDictionary[key];
                GameObject instance = Instantiate(prefab, trackedImage.transform.position, trackedImage.transform.rotation);
                instance.transform.parent = trackedImage.transform;
                Debug.Log("Instantiated prefab: " + key);
                // Add the instance to the dictionary
                _instantiatedPrefabs.Add(key, instance);
            }
        }

        // For all prefabs that have been created so far, set them active or not depending
        // on whether their corresponding image is currently being tracked
        foreach (var trackedImage in eventArgs.updated)
        {
            string key = trackedImage.referenceImage.name.Replace(".png", "");
            if (_instantiatedPrefabs.ContainsKey(key))
            {
                _instantiatedPrefabs[key]
                    .SetActive(trackedImage.trackingState == TrackingState.Tracking);
            }
        }

        // If the AR subsystem has given up looking for a tracked image
        foreach (var trackedImage in eventArgs.removed)
        {
            string key = trackedImage.referenceImage.name.Replace(".png", "");
            // Destroy its prefab
            Destroy(_instantiatedPrefabs[key]);
            // Also remove the instance from our array
            _instantiatedPrefabs.Remove(key);
            // Or, simply set the prefab instance to inactive
            //_instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(false);
        }
    }
}
