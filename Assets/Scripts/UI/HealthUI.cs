using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] GameObject heartPrefab;
    [SerializeField] Transform heartContainer;

    List<Heart> hearts = new List<Heart>();

    public void SetHealthPoint(int maxHealthValue)
    {
        while (hearts.Count < maxHealthValue)
        {
            GameObject heartGO = Instantiate(heartPrefab, heartContainer);
            Heart heart = heartGO.GetComponent<Heart>();

            heart.SetIndex(hearts.Count);
            hearts.Add(heart);
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < maxHealthValue)
            {
                hearts[i].gameObject.SetActive(true);
                hearts[i].SetEmpty(false);
            }
            else
                hearts[i].gameObject.SetActive(false);
        }
    }

    public void UpdateHealth(bool isDecreased, int previousHealthValue, int currentHealthValue)
    {
        if (isDecreased)
        {
            for (int i = currentHealthValue; i < previousHealthValue; i++)
            {
                if (i >= 0 && i < hearts.Count)
                    hearts[i].SetEmpty(true);
            }
        }
        else
        {
            for (int i = previousHealthValue; i < currentHealthValue; i++)
            {
                if (i >= 0 && i < hearts.Count)
                    hearts[i].SetEmpty(false);
            }
        }
    }
}
