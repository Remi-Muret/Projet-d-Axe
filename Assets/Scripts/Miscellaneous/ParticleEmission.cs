using UnityEngine;

public class ParticleEmission : MonoBehaviour
{
    [SerializeField] private GameObject jumpParticlePrefab;
    [SerializeField] private GameObject landParticlePrefab;

    private PlayerController _player;
    private bool _wasGroundedLastFrame;

    void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        _wasGroundedLastFrame = _player.CheckGroundCollision();
    }

    void Update()
    {
        bool isGroundedNow = _player.CheckGroundCollision();

        if (_wasGroundedLastFrame && !isGroundedNow)
            EmitParticle(jumpParticlePrefab);

        if (!_wasGroundedLastFrame && isGroundedNow)
            EmitParticle(landParticlePrefab);

        _wasGroundedLastFrame = isGroundedNow;
    }

    void EmitParticle(GameObject prefab)
    {
        if (prefab != null && _player != null)
        {
            Vector3 spawnPos = _player.transform.position + new Vector3(0, -1f, 0);
            Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);
            GameObject particle = Instantiate(prefab, spawnPos, spawnRot);
            Destroy(particle, 1f);
        }
    }
}
