using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DuckSoundController : MonoBehaviour
{
    private AudioSource _audioSource;
    private DuckDefinition _definition;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    public void SetDefinition(DuckDefinition definition)
    {
        _definition = definition;
    }

    public void PlayLaunch()  => Play(_definition?.launchSound);
    public void PlayCrash()   => Play(_definition?.crashSound);
    public void PlayAbility() => Play(_definition?.abilitySound);

    private void Play(AudioClip clip)
    {
        if (clip == null || _audioSource == null) return;
        _audioSource.PlayOneShot(clip);
    }
}