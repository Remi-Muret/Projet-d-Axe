using UnityEngine;

public class VictoryZone : MonoBehaviour
{
    [SerializeField] private MainUI mainUI;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
            mainUI.ActivateVictoryScreen();
    }
}
