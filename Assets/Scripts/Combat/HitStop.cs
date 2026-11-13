/*
Hitstop (curs C6 - Tips & Tricks): jocul ingheata cateva zeci de ms la impact,
ca lovitura sa se simta "grea". Se apeleaza cu HitStop.Freeze(0.05f).
*/
using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    private static HitStop instance;
    private Coroutine routine;

    // Proiectul are Domain Reload dezactivat: resetam manual starea statica la fiecare Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => instance = null;

    public static void Freeze(float duration)
    {
        if (duration <= 0f) return;
        if (instance == null)
            instance = new GameObject("HitStop").AddComponent<HitStop>();
        instance.StartFreeze(duration);
    }

    private void StartFreeze(float duration)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        Time.timeScale = 0f;
        // Timpul jocului e oprit, deci asteptam in timp real.
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        routine = null;
    }

    private void OnDisable()
    {
        // Daca scena se descarca in timpul unui freeze, nu lasam jocul blocat.
        Time.timeScale = 1f;
    }
}
