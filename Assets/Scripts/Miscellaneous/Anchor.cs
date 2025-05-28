using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : MonoBehaviour
{
    private CameraController cameraController;

    void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cameraController.SnapToAnchor(gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cameraController.ReleaseAnchor();
        }
    }
}
