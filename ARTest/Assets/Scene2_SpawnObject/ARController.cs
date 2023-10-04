using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARController : MonoBehaviour
{
    public GameObject targetImage;
    public SpawnTrigger spawnModeController;

    public ShootTrigger shootModeController;

    public GameObject arObjectToSpawn;
    public GameObject placementIndicator;
    private GameObject spawnedObject;
    private Pose PlacementPose;
    private ARRaycastManager aRRaycastManager;
    private bool placementPoseIsValid = false;

    void Start()
    {
        aRRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    // need to update placement indicator, placement pose and spawn 
    void Update()
    {
        if (spawnModeController.IsSpawnModeActive() && placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ARPlaceObject();
        }

        if (shootModeController.IsShootModeActive() && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (spawnModeController.IsSpawnModeActive())
            {
                spawnModeController.SetSpawnModeActive(false);
            }
            ShootObject();
        }

        UpdateTargetIndicator();
        UpdatePlacementPose();
        UpdatePlacementIndicator();
    }
    void UpdatePlacementIndicator()
    {
        if (spawnModeController.IsSpawnModeActive() && placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(PlacementPose.position, PlacementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    void UpdateTargetIndicator() {
        if (shootModeController.IsShootModeActive()) {
            targetImage.SetActive(true);
        } else {
            targetImage.SetActive(false);
        }
    }

    void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        aRRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            PlacementPose = hits[0].pose;
        }
    }

    void ARPlaceObject()
    {
        Instantiate(arObjectToSpawn, PlacementPose.position, PlacementPose.rotation);
    }

    public GameObject arCamera;
    public GameObject explosion;

    RaycastHit hit;
    void ShootObject()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (Physics.Raycast(arCamera.transform.position, arCamera.transform.forward, out hit))
            {
                if (hit.transform.tag == "Target")
                {
                    Destroy(hit.transform.gameObject);
                    GameObject explosionInstance = Instantiate(explosion, hit.transform.position, hit.transform.rotation);
                    StartCoroutine(DestroyAfterPlaying(explosionInstance));
                }
            }
        }
    }

    IEnumerator DestroyAfterPlaying(GameObject explosion)
    {
        yield return new WaitForSeconds(2);
        // Then destroy the explosion object
        Destroy(explosion);
    }

}
