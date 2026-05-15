using UnityEngine;
using UnityEngine.InputSystem;

public class DebugCheats : MonoBehaviour
{
    [SerializeField] private int coinAmount = 1000000;
    [SerializeField] private Key addCoinsKey = Key.C;
    [SerializeField] private Key advanceStageKey = Key.N;

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!CtrlShiftHeld()) return;

        if (Keyboard.current[addCoinsKey].wasPressedThisFrame)
            CurrencyManager.Instance?.Add(coinAmount);

        if (Keyboard.current[advanceStageKey].wasPressedThisFrame)
        StageManager.Instance?.DebugCompleteCurrentStage();

    }

    private bool CtrlShiftHeld()
    {
        bool ctrl =
            Keyboard.current.leftCtrlKey.isPressed ||
            Keyboard.current.rightCtrlKey.isPressed;

        bool shift =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;

        return ctrl && shift;
    }
}
