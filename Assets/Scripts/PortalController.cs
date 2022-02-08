using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    // There are no nested portals. If a user is in a portal, they cannot
    // enter another portal.
    public static bool inAnyPortal = false;
    [HideInInspector] public bool inPortal = false;
    public Camera portalCam;
    public Camera playerCam;

    public Renderer portalRenderer;

    private RenderTexture renderTexture;


    public GameObject portalToEnterVR;
    public GameObject portalToExitVR;

    public bool portalHasReappeared = true;

    public GameObject portalContents;

    // Start is called before the first frame update
    void OnEnable()
    {
        portalCam = transform.parent.GetComponentInChildren<Camera>();
        playerCam = Camera.main;
        renderTexture = new RenderTexture(1920, 1080, 24);
        portalCam.targetTexture = renderTexture;
        portalRenderer.materials[1].SetTexture("_MainTex", portalCam.targetTexture);

        //Shader standard = Shader.Find("Custom/PortalObjectShader");
        //Shader unlit = Shader.Find("Custom/PortalObjectUnlitShader");
        

        foreach (Renderer renderer in portalContents.GetComponentsInChildren<Renderer>())
        {
            foreach (Material material in renderer.materials)
            {
                material.shader = Shader.Find("Custom/PortalObjectShader");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        portalRenderer.materials[1].SetTexture("_MainTex", portalCam.targetTexture);

        var localToWorldMatrix = playerCam.transform.localToWorldMatrix;
        var renderPosition = new Vector3();
        var renderRotation = new Quaternion();

        portalCam.projectionMatrix = playerCam.projectionMatrix;

        localToWorldMatrix = transform.localToWorldMatrix * transform.worldToLocalMatrix * localToWorldMatrix;

        renderPosition = localToWorldMatrix.GetColumn(3);
        renderRotation = localToWorldMatrix.rotation;

        portalCam.transform.SetPositionAndRotation(renderPosition, renderRotation);


        SetNearClipPlane();
        portalCam.Render();

        

        if (Vector3.Distance(transform.position, playerCam.transform.position) > 2 && !portalHasReappeared)
        {
            portalHasReappeared = true;
            if (inPortal)
            {
                portalToExitVR.SetActive(true);
            }
            else
            {
                portalToEnterVR.SetActive(true);
            }
        }

        
    }

    void SetNearClipPlane()
    {
        float nearClipOffset = 0.05f;
        float nearClipLimit = 0.2f;

        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCam.transform.position));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            // Update projection based on new clip plane
            // Calculate matrix with player cam so that player camera settings (fov, etc) are used
            portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCam.projectionMatrix = playerCam.projectionMatrix;
        }
    }

    // What layers should be visible when the user is in this portal?
    public LayerMask portalContentsMask;

    // What is the default layer mask which should be used if the user isn't in a portal?
    public LayerMask defaultLayerMask;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Transitioning through portal ... ?");

        // User is trying to exit this portal - let them.

        if (portalHasReappeared)
        {
            if (inPortal)
            {
                inPortal = false;
                inAnyPortal = false;
                Camera.main.cullingMask = defaultLayerMask;
                portalToExitVR.SetActive(false);
                portalHasReappeared = false;
            }

            // User is trying tho enter this portal - let  them.
            else if (!inAnyPortal)
            {
                inPortal = true;
                inAnyPortal = true;
                Camera.main.cullingMask = portalContentsMask;
                portalToEnterVR.SetActive(false);
                portalHasReappeared = false;
            }
        }
    }
}
