using System;
using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private Image heart;
    [SerializeField] private Transform heartContent;

    [SerializeField] private float idleAmplitude;
    [SerializeField] private float idleFrequency;
    [SerializeField] private float offsetStep;

    [SerializeField] private float duration;
    [SerializeField] private float initialSize;
    [SerializeField] private float startSize;
    [SerializeField] private float targetSize;

    private Vector3 initialLocalPosition;

    private float idleOffset;
    private float time;
    private int heartIndex;
    private bool isEmpty;
    private bool isFilling;
    private bool isEmptying;

    void Start()
    {
        if (heartContent != null)
            initialLocalPosition = heartContent.localPosition;

        idleOffset = heartIndex * offsetStep;
    }

    void Update()
    {
        if (isEmptying || isFilling)
        {
            float ratio = time / duration;

            if (isEmptying)
            {
                heart.color = Color.Lerp(startColor, endColor, EaseInOutQuart(ratio));
                heart.transform.localScale = Vector3.one * Mathf.Lerp(startSize, targetSize, EaseOutBounce(ratio));
            }
            else if (isFilling)
            {
                heart.color = Color.Lerp(endColor, startColor, EaseInOutQuart(ratio));
                heart.transform.localScale = Vector3.one * Mathf.Lerp(targetSize, initialSize, EaseOutBounce(ratio));
            }

            time += Time.deltaTime;
            if (time >= duration)
            {
                isFilling = false;
                isEmptying = false;
                time = 0f;
            }
        }
        else
        {
            if (!isEmpty)
            {
                float idleY = Mathf.Sin(Time.time * idleFrequency + idleOffset) * idleAmplitude;
                Vector3 newPos = initialLocalPosition + Vector3.up * idleY;
                heartContent.localPosition = newPos;
            }
        }
    }

    public void SetIndex(int index)
    {
        heartIndex = index;
        idleOffset = heartIndex * offsetStep;
    }

    public void SetEmpty(bool empty)
    {
        if (isEmpty == empty) return;

        isEmpty = empty;
        time = 0f;
        heartContent.localPosition = initialLocalPosition;

        if (empty)
        {
            isEmptying = true;
            isFilling = false;
        }
        else
        {
            isFilling = true;
            isEmptying = false;
        }
    }

    float EaseInOutQuart(float x) => x < 0.5f ? 8f * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 4f) / 2f;

    float EaseOutBounce(float x)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (x < 1f / d1)
            return n1 * x * x;
        else if (x < 2f / d1)
        {
            x -= 1.5f / d1;
            return n1 * x * x + 0.75f;
        }
        else if (x < 2.5f / d1)
        {
            x -= 2.25f / d1;
            return n1 * x * x + 0.9375f;
        }
        else
        {
            x -= 2.625f / d1;
            return n1 * x * x + 0.984375f;
        }
    }
}
