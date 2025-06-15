using Oculus.Interaction.Samples;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MainMenuControls : MonoBehaviour
{
    public List<AudioClip> helpTroll;
    private AudioSource audioSource;
    public Canvas uiCanvas;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    public void OnStart()
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void QuitApp()
    {
        Application.Quit();
    }
    public void QuitMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void PlaySound()
    {
        if (helpTroll == null || helpTroll.Count == 0)
            return;

        int index = Random.Range(0, helpTroll.Count);
        AudioClip clip = helpTroll[index];

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    public void OnResume()
    {
        uiCanvas.gameObject.SetActive(false);
    }
}
