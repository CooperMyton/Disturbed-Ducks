using UnityEngine;

public class StageAudioManager : MonoBehaviour
{
    public static StageAudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource oneShotSource;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayStageAudio(StageDefinition stage)
    {
        if (stage == null) return;

        if (stage.backgroundMusic != null)
            PlayLoop(musicSource, stage.backgroundMusic);

        PlayLoop(ambienceSource, stage.ambienceLoop);
    }

    public void PlayClearSong(StageDefinition stage)
    {
        if (stage != null && stage.clearSong != null)
            oneShotSource.PlayOneShot(stage.clearSong);
    }

    private void PlayLoop(AudioSource source, AudioClip clip)
    {
        if (source == null) return;

        if (source.clip == clip && source.isPlaying) return;

        source.Stop();
        source.clip = clip;
        source.loop = true;

        if (clip != null)
            source.Play();
    }
}