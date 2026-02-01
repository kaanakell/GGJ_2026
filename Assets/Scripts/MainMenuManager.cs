using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayMusic(
            AudioManager.Instance.soundLibrary.menuMusic
        );
    }
}
