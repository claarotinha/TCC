using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseSoundSettings : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text volumeValueText;

    private void Awake()
    {
        float savedVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(VolumeKey, 1f)
        );

        // Aplica o volume mesmo com o painel de som fechado.
        AudioListener.volume = savedVolume;

    }

    private void Start()
    {
        float savedVolume = AudioListener.volume;
        RecuperarControlesDoPainel();

        if (masterVolumeSlider == null || volumeValueText == null)
        {
            Debug.LogError(
                "PauseSoundSettings: preencha o Slider " +
                "e o texto de porcentagem.",
                this
            );
            enabled = false;
            return;
        }

        masterVolumeSlider.minValue = 0f;
        masterVolumeSlider.maxValue = 1f;
        masterVolumeSlider.wholeNumbers = false;

        masterVolumeSlider.SetValueWithoutNotify(savedVolume);
        UpdateLabel(savedVolume);

        masterVolumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void RecuperarControlesDoPainel()
    {
        // O controlador pode estar em um objeto raiz separado do PauseCanvas.
        // Inclui painéis inativos, mas nunca usa controles de outra cena.
        foreach (GameObject raiz in gameObject.scene.GetRootGameObjects())
        {
            foreach (Transform painel in raiz.GetComponentsInChildren<Transform>(true))
            {
                if (painel.name != "SoundPanel") continue;
                if (masterVolumeSlider == null)
                    masterVolumeSlider = painel.GetComponentInChildren<Slider>(true);
                if (volumeValueText == null)
                    foreach (TMP_Text texto in painel.GetComponentsInChildren<TMP_Text>(true))
                        if (texto.name == "VolumeValueText")
                        {
                            volumeValueText = texto;
                            break;
                        }
            }
        }
    }

    public void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);

        AudioListener.volume = value;
        UpdateLabel(value);

        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void UpdateLabel(float value)
    {
        if (volumeValueText != null)
        {
            volumeValueText.text =
                Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(
                SetVolume
            );
        }

        PlayerPrefs.Save();
    }
}