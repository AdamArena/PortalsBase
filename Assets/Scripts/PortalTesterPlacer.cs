using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTesterPlacer : MonoBehaviour
{
    private void OnMouseDown()
    {
        gameObject.SetActive(false);
        PortalTester tester = GameObject.FindObjectOfType<PortalTester>();
        tester.Confirm();
        tester.placementIndicator.SetActive(false);
        tester.gameObject.SetActive(false);
    }
}
