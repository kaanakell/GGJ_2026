using System.Collections;
using UnityEngine;

public static class HitStop
{
    private static bool active;

    public static void Freeze(float duration)
    {
        if (active) return;

        active = true;
        Time.timeScale = 0f;
        CoroutineRunner.Instance.StartCoroutine(Resume(duration));
    }

    private static IEnumerator Resume(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        active = false;
    }
}
