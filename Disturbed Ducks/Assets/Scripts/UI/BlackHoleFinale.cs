using UnityEngine;

public class BlackHoleFinale : MonoBehaviour
{
    [SerializeField] private GameObject blackHolePanel;
    [SerializeField] private RectTransform blackCircle;
    [SerializeField] private float growDuration = 3f;
    [SerializeField] private Vector3 finalScale = new Vector3(30f, 30f, 30f);
    [SerializeField] private float holdBlackSeconds = 0.5f;
    [SerializeField] [TextArea(3, 8)]
    private string finalWinMessage = "YOU WIN!\nThe King Duck sacrificed himself to take down the mecha and Husky Inc.\nHusky and Beaver Inc are gone for good! The Willamette River is free!";
    private bool _running;
    private bool _holdingBlack;
    private float _timer;

    private void Awake()
    {
        blackHolePanel?.SetActive(false);
    }

    public void Begin()
    {
        _running = true;
        _holdingBlack = false;
        _timer = 0f;

        blackHolePanel?.SetActive(true);

        if (blackCircle != null)
            blackCircle.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (!_running) return;

        _timer += Time.deltaTime;

        if (!_holdingBlack)
        {
            float t = Mathf.Clamp01(_timer / growDuration);

            if (blackCircle != null)
                blackCircle.localScale = Vector3.Lerp(Vector3.zero, finalScale, t);

            if (t >= 1f)
            {
                _holdingBlack = true;
                _timer = 0f;
            }

            return;
        }

        if (_timer >= holdBlackSeconds)
        {
            _running = false;
            blackHolePanel?.SetActive(false);
            if (blackCircle != null)
                blackCircle.gameObject.SetActive(false);

            WinScreenUI.Instance?.Show(finalWinMessage);
        }
    }
}