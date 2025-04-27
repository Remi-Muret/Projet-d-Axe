using UnityEngine;

public class ItemController : MonoBehaviour
{
    [SerializeField] private AudioClip _catchSound;

    private ItemData _data;
    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        _data = GameManager.Instance.GetItemData().GetItemData(itemType);;

        transform.localScale = Vector3.one * _data.scaleCoef;

        if (_data.sprite != null)
            _spriteRenderer.sprite = _data.sprite;
    }

    void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
            _audioSource.PlayOneShot(clip);
    }

    public ItemData.ITEM itemType;
}
