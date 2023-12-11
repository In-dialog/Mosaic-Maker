using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public int textureWidth = 512; // Width of the generated texture
    public int textureHeight = 512; // Height of the generated texture
    public Vector2[] attractors; // List of attractor points
    public float attractorRadius = 0.2f; // Radius of the attractor points

    private Texture2D generatedTexture;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        generatedTexture = new Texture2D(textureWidth, textureHeight);
        ApplyTextureToMaterial();
    }

    private void Update()
    {
        GenerateTexture();
        ApplyTextureToMaterial();
    }

    private void GenerateTexture()
    {
        Color[] pixels = generatedTexture.GetPixels();

        float baseFrequency = 5f; // Base frequency of the wave
        float baseAmplitude = 0.5f; // Base amplitude of the wave

        // Generate gradient pattern field
        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                Vector2 point = new Vector2((float)x / textureWidth, (float)y / textureHeight);

                float frequency = baseFrequency;
                float amplitude = baseAmplitude;

                // Find the closest attractor points within the attractor radius
                Vector2 closestAttractor = attractors[0];
                Vector2 secondClosestAttractor = attractors[0];
                float closestDistance = Mathf.Infinity;
                float secondClosestDistance = Mathf.Infinity;

                for (int i = 0; i < attractors.Length; i++)
                {
                    float d = Vector2.Distance(point, attractors[i]);

                    if (d < closestDistance)
                    {
                        secondClosestDistance = closestDistance;
                        secondClosestAttractor = closestAttractor;
                        closestDistance = d;
                        closestAttractor = attractors[i];
                    }
                    else if (d < secondClosestDistance)
                    {
                        secondClosestDistance = d;
                        secondClosestAttractor = attractors[i];
                    }
                }

                // Calculate the interpolation factor based on distance to the attractors
                float t = Mathf.Clamp01((closestDistance - attractorRadius) / (secondClosestDistance - closestDistance + attractorRadius));

                // Interpolate frequency and amplitude between the two closest attractors
                frequency = Mathf.Lerp(closestAttractor.x, secondClosestAttractor.x, t) * baseFrequency;
                amplitude = Mathf.Lerp(closestAttractor.y, secondClosestAttractor.y, t) * baseAmplitude;

                float waveOffset = Mathf.PerlinNoise(point.x * frequency, point.y * frequency) * amplitude;

                // Apply color based on warped wave offset
                Color color = new Color(waveOffset, waveOffset, waveOffset);
                pixels[y * textureWidth + x] = color;
            }
        }

        // Set the pixel colors to the texture
        generatedTexture.SetPixels(pixels);
        generatedTexture.Apply();
    }

    private void ApplyTextureToMaterial()
    {
        if (meshRenderer != null)
        {
            Material material = meshRenderer.material;
            if (material != null)
            {
                material.mainTexture = generatedTexture;
            }
        }
    }
}