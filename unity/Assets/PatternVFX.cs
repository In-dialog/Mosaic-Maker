using System;
using System.Collections;
using UnityEngine;

public class PatternVFX : MonoBehaviour
{
    public static PatternVFX Instance { get; private set; } // Singleton instance

    public Renderer objectRenderer; // Assign the object with the material you want to animate in the Inspector
    public float animationDuration = 2.0f; // Duration of the animation in seconds
    public Action OnAnimationComplete; // This action is triggered when the animation completes

    private int animationPropertyID;

    private void Awake()
    {
        // Singleton implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure the renderer is assigned, if not, try to get it from the current GameObject
        if (!objectRenderer)
            objectRenderer = GetComponent<Renderer>();

        animationPropertyID = Shader.PropertyToID("_Animation");
    }



    public IEnumerator ProcessImg()
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsedTime / animationDuration);
            objectRenderer.material.SetFloat(animationPropertyID, 1 - normalizedTime);

            yield return null;
        }

        // Ensure the value is set to 1 at the end of the animation
        objectRenderer.material.SetFloat(animationPropertyID, 0f);

    }
}

