using UnityEngine;
using UnityEngine.Rendering;

public class StageLightingOverride : MonoBehaviour
{
    [Header("Lights To Disable While This Stage Is Active")]
    [SerializeField] private Light[] lightsToDisable;

    [Header("Duck Torch")]
    [SerializeField] private GameObject duckTorchLight;

    [Header("Ambient Override")]
    [SerializeField] private bool overrideAmbientLighting = true;
    [SerializeField] private Color darkAmbientColor = new Color(0.01f, 0.01f, 0.015f);
    [SerializeField] private float darkReflectionIntensity = 0f;

    private bool[] _previousLightStates;

    private AmbientMode _previousAmbientMode;
    private Color _previousAmbientSkyColor;
    private Color _previousAmbientEquatorColor;
    private Color _previousAmbientGroundColor;
    private float _previousAmbientIntensity;
    private float _previousReflectionIntensity;

    private void OnEnable()
    {
        SaveAmbientSettings();

        _previousLightStates = new bool[lightsToDisable.Length];

        for (int i = 0; i < lightsToDisable.Length; i++)
        {
            if (lightsToDisable[i] == null)
                continue;

            _previousLightStates[i] = lightsToDisable[i].enabled;
            lightsToDisable[i].enabled = false;
        }

        if (overrideAmbientLighting)
            ApplyDarkAmbientSettings();

        if (duckTorchLight != null)
            duckTorchLight.SetActive(true);
    }

    private void OnDisable()
    {
        if (_previousLightStates != null)
        {
            for (int i = 0; i < lightsToDisable.Length; i++)
            {
                if (lightsToDisable[i] == null)
                    continue;

                lightsToDisable[i].enabled = _previousLightStates[i];
            }
        }

        RestoreAmbientSettings();

        if (duckTorchLight != null)
            duckTorchLight.SetActive(false);
    }

    private void SaveAmbientSettings()
    {
        _previousAmbientMode = RenderSettings.ambientMode;
        _previousAmbientSkyColor = RenderSettings.ambientSkyColor;
        _previousAmbientEquatorColor = RenderSettings.ambientEquatorColor;
        _previousAmbientGroundColor = RenderSettings.ambientGroundColor;
        _previousAmbientIntensity = RenderSettings.ambientIntensity;
        _previousReflectionIntensity = RenderSettings.reflectionIntensity;
    }

    private void ApplyDarkAmbientSettings()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = darkAmbientColor;
        RenderSettings.ambientIntensity = 0f;
        RenderSettings.reflectionIntensity = darkReflectionIntensity;
    }

    private void RestoreAmbientSettings()
    {
        RenderSettings.ambientMode = _previousAmbientMode;
        RenderSettings.ambientSkyColor = _previousAmbientSkyColor;
        RenderSettings.ambientEquatorColor = _previousAmbientEquatorColor;
        RenderSettings.ambientGroundColor = _previousAmbientGroundColor;
        RenderSettings.ambientIntensity = _previousAmbientIntensity;
        RenderSettings.reflectionIntensity = _previousReflectionIntensity;
    }
}