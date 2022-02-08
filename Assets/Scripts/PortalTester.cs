using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PortalTester : MonoBehaviour
{
    private ARRaycastManager arRaycastManager;
    public GameObject placementIndicator;

    public GameObject contentToPlace;

    void Start()
    {
        arRaycastManager = GameObject.FindObjectOfType<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 position = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> arRaycastHits = new List<ARRaycastHit>();
        if (arRaycastManager.Raycast(position, arRaycastHits) && arRaycastHits.Count > 0)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.position = arRaycastHits[0].pose.position;
            placementIndicator.transform.rotation = arRaycastHits[0].pose.rotation;
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    public void Confirm ()
    {
        contentToPlace.transform.position = placementIndicator.transform.position;
        contentToPlace.transform.rotation = placementIndicator.transform.rotation;
        contentToPlace.SetActive(true);
    }
}
