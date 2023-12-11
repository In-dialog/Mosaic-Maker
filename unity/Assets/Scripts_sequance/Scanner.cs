using UnityEngine;
using Unity.Collections;
using System.Collections;
using System;



public class Scanner : MonoBehaviour
{
    public GameObject fragmentsContainer;
    public event Action<ScannedObject> PartReadyForNesting;


    public void StartScanning(GameObject placeholder)
    {
        StartCoroutine(ScanningCoroutine(placeholder));
    }

    private IEnumerator ScanningCoroutine(GameObject placeholder)
    {
        // Get the original object (child of the placeholder)
        Transform originalObject = fragmentsContainer.transform.GetChild(int.Parse(placeholder.name) - 1);
        yield return ScannerVFX.Instance.StartCoroutine(ScannerVFX.Instance.AnimateScanProgress());

        // Destroy the placeholder
        Destroy(placeholder);

        // TODO: Implement the scanning process that returns a ScannedObject instance with the required data
        // example

        ScannedObject scannedObject = new ScannedObject(
            originalObject,                  // transform
            getColor(originalObject),                    // collor
            placeholder.transform.position,  // original position
            originalObject.position,         // final posion
            ""                               // svg outline
        );
        originalObject.transform.position = placeholder.transform.position;
        originalObject.gameObject.SetActive(true);
        PartReadyForNesting?.Invoke(scannedObject);

        yield return null;
    }


    Color getColor(Transform originalObject)
    {
        Vector2 UV = originalObject.GetComponent<MeshRenderer>().material.GetVector("_UV_position");
        Texture2D texture = ManegeTetureUV.Instance.mainTexture;
        UV = UVToPixelCoordinates(UV, texture);
        return texture.GetPixel((int)UV.x, (int)UV.y);
    }


    public Vector2Int UVToPixelCoordinates(Vector2 uv, Texture2D texture)
    {
        int x = Mathf.Clamp((int)(uv.x * texture.width), 0, texture.width - 1);
        int y = Mathf.Clamp((int)(uv.y * texture.height), 0, texture.height - 1);

        return new Vector2Int(x, y);
    }
}
