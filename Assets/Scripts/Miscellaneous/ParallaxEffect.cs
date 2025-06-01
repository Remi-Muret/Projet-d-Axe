using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public Transform mainCamera;
    public Vector2[] parallaxScales;

    private Transform[] layers;
    private Vector3 lastCameraPosition;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main.transform;

        lastCameraPosition = mainCamera.position;
        layers = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
            layers[i] = transform.GetChild(i);
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = mainCamera.position - lastCameraPosition;

        for (int i = 0; i < layers.Length; i++)
        {
            if (i < parallaxScales.Length)
            {
                Vector3 newLayerPosition = layers[i].position;
                newLayerPosition += new Vector3(deltaMovement.x * parallaxScales[i].x, deltaMovement.y * parallaxScales[i].y, 0);
                layers[i].position = newLayerPosition;
            }
        }

        lastCameraPosition = mainCamera.position;
    }
}
