using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using Input;

namespace UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject crosshair;

        [Header("Audio")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private AudioMixerSnapshot unpausedSnapshot;
        [SerializeField] private AudioMixerSnapshot pausedSnapshot;

        private PlayerInputActions _playerInputActions;
        private bool _isPaused;

        private void Awake()
        {
            Initialize();
            BindInputActions();
        }
        private void Start()
        {
            InitializeVolume();
        }
        
        
        private void OnEnable() => _playerInputActions.UI.Enable();
        private void OnDisable() => _playerInputActions.UI.Disable();

        private void Initialize()
        {
            _playerInputActions = new PlayerInputActions();
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            crosshair.SetActive(true);
            
            if (mainMixer != null)
            {
                mainMixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
            }
        }
        
        private void InitializeVolume()
        {
            if (volumeSlider != null)
            {
                float initialVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                volumeSlider.value = initialVolume;
                SetVolume(initialVolume);
            }
        }

        private void BindInputActions()
        {
            _playerInputActions.UI.Pause.performed += ctx => TogglePause();
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                ActivateMenu();
            }
            else
            {
                DeactivateMenu();
            }
        }

        private void ActivateMenu()
        {
            Time.timeScale = 0f;
            pausedSnapshot.TransitionTo(0.1f);
            pausePanel.SetActive(true);
            crosshair.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void DeactivateMenu()
        {
            Time.timeScale = 1f;
            unpausedSnapshot.TransitionTo(0.1f);
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            crosshair.SetActive(true);
            _isPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            unpausedSnapshot.TransitionTo(0.1f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OpenSettings()
        {
            pausePanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            settingsPanel.SetActive(false);
            pausePanel.SetActive(true);
        }

        public void SetVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
            mainMixer.SetFloat("MasterVolume", Mathf.Log10(clampedVolume) * 20);
            
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }
    }
}