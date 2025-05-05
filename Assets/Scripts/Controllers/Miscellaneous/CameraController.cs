using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _transitionSpeed;

    private float _lastHorizontalOffset;
    private bool _isSnappedToAnchor;
    private GameObject _player;
    private GameObject _currentAnchor;
    private Rigidbody2D playerRigidbody;
    private Vector2 _currentOffset;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");

        if (_player != null)
        {
            playerRigidbody = _player.GetComponent<Rigidbody2D>();
        }

        _lastHorizontalOffset = 0f;
    }

    void Update()
    {
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");

            if (_player != null)
                playerRigidbody = _player.GetComponent<Rigidbody2D>();
        }

        if (_player != null && playerRigidbody != null)
        {
            float horizontalVelocity = playerRigidbody.linearVelocity.x;

            if (horizontalVelocity > 0.5f)
            {
                _lastHorizontalOffset = 1f;
            }
            else if (horizontalVelocity < -0.5f)
            {
                _lastHorizontalOffset = -1f;
            }

            Vector2 playerOffset = new Vector2(0f, 0f);
            playerOffset.x = _lastHorizontalOffset;
            _currentOffset = Vector2.Lerp(_currentOffset, playerOffset, _transitionSpeed * Time.deltaTime);

            Vector2 targetPosition;
            if (_isSnappedToAnchor && _currentAnchor != null)
            {
                targetPosition = (Vector2)_currentAnchor.transform.position;
            }
            else
            {
                targetPosition = (Vector2)_player.transform.position + _currentOffset;
            }

            transform.position = Vector3.Lerp(transform.position, new Vector3(targetPosition.x, targetPosition.y, transform.position.z), _transitionSpeed * Time.deltaTime);
        }
    }

    public void SnapToAnchor(GameObject anchor)
    {
        _currentAnchor = anchor;
        _isSnappedToAnchor = true;
    }

    public void ReleaseAnchor()
    {
        _isSnappedToAnchor = false;
        _currentAnchor = null;
    }
}
