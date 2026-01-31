using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;

    [Header("Panels")]
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    [Header("Audio Settings")]
    public AudioMixer masterMixer;
    public Slider masterSlider, musicSlider, sfxSlider;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameMenu");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetMasterVolume(float value) => masterMixer.SetFloat("MasterVol", Mathf.Log10(value) * 20);
    public void SetMusicVolume(float value) => masterMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
    public void SetSFXVolume(float value) => masterMixer.SetFloat("SFXVol", Mathf.Log10(value) * 20);
}