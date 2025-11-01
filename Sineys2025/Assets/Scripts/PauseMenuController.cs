using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[AddComponentMenu("UI/PauseMenuController")]
public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public GameObject[] volumeIndicators;
    public string sceneToLoadOnQuit = "MainMenu";

    [Header("Input (New Input System)")]
    public InputActionReference pauseAction;

    [Header("Audio")]
    public AudioMixer masterMixer;
    [SerializeField] private AudioSource _soundTrack;
    [Tooltip("Exposed parameter name in AudioMixer (exact name). If blank or not found, script будет использовать AudioListener.volume как fallback.")]
    public string exposedVolumeParam = "MasterVolume";
    public AudioClip[] clickClips;
    public AudioSource audioSource;

    [Header("Settings")]
    [Range(0, 100)] public int volumePercent = 50;
    public float minDb = -80f;
    public float maxDb = 0f;
    public float volumeStepPercent = 10f;

    // internal
    bool isPaused = false;
    bool mixerHasParam = false; // true если masterMixer и exposedVolumeParam валидны
    bool forceUseAudioListenerFallback = false; // если true — принудительно используем AudioListener

    void Awake()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Проверим заранее, существует ли указанный exposed параметр
        RefreshMixerParamAvailability();

        ApplyVolumeToMixer();
        UpdateVolumeIndicators();
    }

    void OnEnable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePerformed;
        }
    }

    void OnDisable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    void RefreshMixerParamAvailability()
    {
        mixerHasParam = false;
        if (masterMixer == null || string.IsNullOrEmpty(exposedVolumeParam)) return;

        // Попытка прочитать параметр. GetFloat вернёт false если параметр не найден.
        float tmp;
        try
        {
            mixerHasParam = masterMixer.GetFloat(exposedVolumeParam, out tmp);
        }
        catch
        {
            mixerHasParam = false;
        }
    }

    void OnPausePerformed(InputAction.CallbackContext ctx) => TogglePause();

    public void TogglePause()
    {
        if (!isPaused)
        {
            DoPause();
        }

        else {
            DoResume(); 
        }
    }

    void DoPause()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        _soundTrack.Pause();
    }

    void DoResume()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        _soundTrack.UnPause();
    }

    public void OnContinueButton()
    {
        PlayRandomClick();
        DoResume();
    }

    public void OnQuitButton()
    {
        PlayRandomClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoadOnQuit);
    }

    public void OnVolumeUpButton()
    {
        PlayRandomClick();
        SetVolumePercent(volumePercent + (int)volumeStepPercent);
    }

    public void OnVolumeDownButton()
    {
        PlayRandomClick();
        SetVolumePercent(volumePercent - (int)volumeStepPercent);
    }

    public void SetVolumePercent(int pct)
    {
        volumePercent = Mathf.Clamp(pct, 0, 100);
        ApplyVolumeToMixer();
        UpdateVolumeIndicators();
    }

    void ApplyVolumeToMixer()
    {
        float t = volumePercent / 100f;

        // Если указали миксер и параметр существует — пробуем SetFloat.
        if (!forceUseAudioListenerFallback && masterMixer != null && mixerHasParam)
        {
            float db = Mathf.Lerp(minDb, maxDb, t);
            // Попробуем установить, обернём в try чтобы избежать ошибок при неверном имени.
            bool ok = false;
            try
            {
                ok = masterMixer.SetFloat(exposedVolumeParam, db);
            }
            catch
            {
                ok = false;
            }

            if (ok) return;

            // Если SetFloat вернул false или бросил — включаем fallback
            forceUseAudioListenerFallback = true;
            Debug.LogWarning("[PauseMenuController] Exposed param not settable. Falling back to AudioListener.volume");
        }

        // Fallback: простой глобальный регулятор
        AudioListener.volume = Mathf.Clamp01(t);
    }

    void UpdateVolumeIndicators()
    {
        if (volumeIndicators == null || volumeIndicators.Length == 0) return;
        int total = volumeIndicators.Length;
        int activeCount = Mathf.RoundToInt((volumePercent / 100f) * total);
        for (int i = 0; i < total; i++)
        {
            GameObject go = volumeIndicators[i];
            if (go == null) continue;
            bool shouldBeActive = i < activeCount;
            if (go.activeSelf != shouldBeActive) go.SetActive(shouldBeActive);
        }
    }

    void PlayRandomClick()
    {
        if (clickClips == null || clickClips.Length == 0 || audioSource == null) return;
        var clip = clickClips[Random.Range(0, clickClips.Length)];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clip);
    }

    // Позволяет вручную обновить статус доступности exposed param в редакторе/рантайме
    [ContextMenu("RefreshMixerParamAvailability")]
    public void EditorRefreshMixerParamAvailability()
    {
        RefreshMixerParamAvailability();
        Debug.Log($"mixerHasParam = {mixerHasParam}; exposedVolumeParam='{exposedVolumeParam}'");
    }
}
