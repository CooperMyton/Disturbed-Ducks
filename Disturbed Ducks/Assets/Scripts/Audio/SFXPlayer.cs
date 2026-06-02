using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer Instance { get; private set; }

    [SerializeField] private AudioSource source;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (source == null)
            source = GetComponent<AudioSource>();

        if (source != null)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f; // 2D audio
            source.volume = 1f;
        }
    }

    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || Instance == null || Instance.source == null) return;
        Instance.source.PlayOneShot(clip, volume);
    }
}