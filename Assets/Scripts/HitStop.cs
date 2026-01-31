using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    private static HitStop instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Freeze(float duration)
    {
        if (instance == null) return;
        instance.StartCoroutine(instance.FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
    }
}
