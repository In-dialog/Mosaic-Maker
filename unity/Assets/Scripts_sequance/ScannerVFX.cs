using UnityEngine;
using System.Collections;

public class ScannerVFX : MonoBehaviour
{
    public Renderer scannerMaterial;
    public static ScannerVFX Instance { get; private set; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);

        }
    }


    // Coroutine to animate the _ScanProgress property over a specified duration.
    public IEnumerator AnimateScanProgress()
    {
        float duration = 2;
        float startTime = Time.time;
        float endTime = startTime + duration;
        float currentTime = startTime;

        while (currentTime <= endTime)
        {
            float t = (currentTime - startTime) / duration;
            float scanProgress = Mathf.Lerp(0, 1, t);
            SetProgress(scanProgress);

            yield return null; // Wait for the next frame.
            currentTime = Time.time;
        }
        SetProgress(1);

    }
    void SetProgress(float state)
    {
        state = state - 0.5f;
        scannerMaterial.material.SetFloat("_ScanProgress", state); // Ensure the final value is set to 0.5.

    }
}
