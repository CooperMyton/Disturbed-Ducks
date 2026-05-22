using UnityEngine;

public class CollisionSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float minImpactSpeed = 2f;
    [SerializeField] private float cooldown = 0.15f;

    private float _lastPlayTime = -999f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hitSound == null || audioSource == null) return;
        if (Time.time - _lastPlayTime < cooldown) return;
        if (collision.relativeVelocity.magnitude < minImpactSpeed) return;

        _lastPlayTime = Time.time;
        audioSource.PlayOneShot(hitSound);
    }
}