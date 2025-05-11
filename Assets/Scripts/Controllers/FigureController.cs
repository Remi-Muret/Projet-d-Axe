using UnityEngine;

public class FigureController : ObjectController
{
    [Header("Debug")]
    [SerializeField] protected bool _guiDebug;
    [SerializeField] protected bool _showGizmos;

    [Header("Ground Checker")]
    [SerializeField] protected Transform _groundChecker;
    [SerializeField] protected LayerMask _groundLayer;

    [Header("Attacks")]
    [SerializeField] protected GameObject _bombPrefab;
    [SerializeField] protected GameObject _projectilePrefab;

    [Header("Audio")]
    [SerializeField] protected AudioClip _jumpSound;
    [SerializeField] protected AudioClip _attackSound;
    [SerializeField] protected AudioClip _hitSound;

    protected Vector2 _groundCheckerBoxSize = new Vector2(0.98f, 0f);

    protected void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
            _audioSource.PlayOneShot(clip);
    }
}