using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private Image TimerFill;

    private void Update()
    {
        if (TimeSystem.Instance == null || TimerFill == null) return;

        float remainingTime = TimeSystem.Instance.GetRemainingTime();
        float maxTime = TimeSystem.Instance.GetMaxTime();

        float fillAmount = Mathf.Clamp01(remainingTime / maxTime);
        TimerFill.fillAmount = fillAmount;
    }
}
