using UnityEngine;
using UnityEngine.UI;

public class ButtonClickAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    private void Start()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            button.onClick.AddListener(PlayClick);
    }

    private void PlayClick()
    {
        if (clickSound != null && audioSource != null)
            audioSource.PlayOneShot(clickSound);
    }
}