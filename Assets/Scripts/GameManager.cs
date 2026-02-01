using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayMusic(
            AudioManager.Instance.soundLibrary.gameplayMusic
        );
    }
}

