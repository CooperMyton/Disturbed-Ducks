using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DuckSoundController : MonoBehaviour
{
    [SerializeField] private AudioSource flyingLoopSource;

    private AudioSource _audioSource;
    private DuckDefinition _definition;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (flyingLoopSource != null)
        {
            flyingLoopSource.playOnAwake = false;
            flyingLoopSource.loop = true;
        }
    }

    public void SetDefinition(DuckDefinition definition)
    {
        _definition = definition;

        if (flyingLoopSource != null)
            flyingLoopSource.clip = _definition != null ? _definition.flyingLoopSound : null;
    }

    public void PlayLaunch()
    {
        Play(_definition?.launchSound);
        StartFlyingLoop();
    }

    public void PlayCrash()
    {
        StopFlyingLoop();
        Play(_definition?.crashSound);
    }

    public void PlayAbility()
    {
        Play(_definition?.abilitySound);
    }

    public void StopFlyingLoop()
    {
        if (flyingLoopSource != null)
            flyingLoopSource.Stop();
    }

    private void StartFlyingLoop()
    {
        if (flyingLoopSource == null) return;
        if (flyingLoopSource.clip == null) return;

        flyingLoopSource.Stop();
        flyingLoopSource.Play();
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || _audioSource == null) return;
        _audioSource.PlayOneShot(clip);
    }
}