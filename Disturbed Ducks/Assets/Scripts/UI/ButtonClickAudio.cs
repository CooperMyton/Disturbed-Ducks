using UnityEngine;
using UnityEngine.UI;

public class ButtonClickAudio : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float clickVolume = 1f;

    private void Start()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            button.onClick.AddListener(PlayClick);
    }

    private void PlayClick()
    {
        SfxPlayer.Play(clickSound, clickVolume);
    }
}