using UnityEngine;

public class LevelCompleteTrigger : MonoBehaviour
{
    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            GameStateManager.Instance.SetState(GameState.LevelComplete);
            UIManager.Instance.ShowLevelComplete();
        }
    }
}
