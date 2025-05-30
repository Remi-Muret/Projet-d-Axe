using UnityEngine;

public class VictoryZone : MonoBehaviour
{
    [SerializeField] private MainUIController uiController;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
            uiController.ActivateVictoryScreen();
    }
}
