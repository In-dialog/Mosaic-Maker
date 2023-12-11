using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    public float spacing = 1.0f;
    public float firstObjectSpacing = 2.0f;
    public Vector3 startPosition = new Vector3(0, 0, 0);
    public GameObject placeholderPrefab;
    public float moveSpeed = 1.0f;
    public float animationTime = 1.0f;

    private GameObject placeholderContainer;
    private GameObject binToAnalize;

    private List<Transform> placeholders;
    private List<Vector3> targetPositions;

    public event Action OnAllPartsShifted;
    public event Action<GameObject> PartReadyForScan;

    void Start()
    {
        placeholderContainer = new GameObject("PlaceholderContainer");
        binToAnalize = new GameObject("binToAnalize");

        int childCount = transform.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        placeholders = new List<Transform>(childCount);
        targetPositions = new List<Vector3>(childCount);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            Vector3 position = startPosition + Vector3.left * spacing * i;

            GameObject placeholder = CreatePlaceholder(position, child.gameObject);
            placeholders.Add(placeholder.transform);
            targetPositions.Add(placeholders[i].position + Vector3.right * spacing);
            child.gameObject.SetActive(false);
        }
    }


    private GameObject CreatePlaceholder(Vector3 position, GameObject originalObject)
    {
        GameObject placeholder = Instantiate(placeholderPrefab, position, Quaternion.identity, placeholderContainer.transform);
        placeholder.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        Renderer renderer = originalObject.GetComponent<Renderer>();
        Vector3 originalSize = renderer.bounds.size;
        placeholder.transform.localScale = new Vector3(originalSize.x * originalObject.transform.localScale.x, originalSize.y * originalObject.transform.localScale.y, 1);

        Regex regex = new Regex(@"\d+(\.\d+)?");
        MatchCollection matches = regex.Matches(originalObject.name);
        placeholder.name = matches[0].ToString();

        return placeholder;
    }

    public IEnumerator MoveFirstObject()
    {
        Transform firstPlaceholder = placeholders[0];

        Vector3 targetPosition = startPosition + Vector3.right * (firstObjectSpacing + Random.Range(0, 0.02f));
        float threshold = 0.1f; // Define a suitable threshold value here

        while (Vector3.Distance(firstPlaceholder.position, targetPosition) > threshold)
        {
            firstPlaceholder.position = Vector3.MoveTowards(firstPlaceholder.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        //firstPlaceholder.position = targetPosition; // Ensure the first placeholder is exactly at the target position
        firstPlaceholder.SetParent(binToAnalize.transform);
        targetPositions.RemoveAt(placeholders.IndexOf(firstPlaceholder));
        placeholders.Remove(firstPlaceholder);
        PartReadyForScan?.Invoke(firstPlaceholder.gameObject);
        StartCoroutine(MoveAllObjects());
    }

    public IEnumerator MoveAllObjects()
    {
        int placeholderCount = placeholders.Count;
        float moveDistance = spacing;
        float moveTime = moveDistance / moveSpeed;

        // Calculate target positions
        List<Vector3> targetPositions = new List<Vector3>(placeholderCount);
        for (int i = 0; i < placeholderCount; i++)
        {
            targetPositions.Add(placeholders[i].position + Vector3.right * spacing);
        }

        float startTime = Time.time;

        while (Time.time - startTime < moveTime)
        {
            for (int i = 0; i < placeholderCount; i++)
            {
                if (placeholders[i] != null) // Check if the placeholder is not null and is still active
                {
                    placeholders[i].position = Vector3.MoveTowards(placeholders[i].position, targetPositions[i], moveSpeed * Time.deltaTime);
                }
            }
            yield return null;
        }

        // Ensure placeholders are exactly at target positions
        for (int i = 0; i < placeholderCount; i++)
        {
            if (placeholders[i] != null)
            {
                placeholders[i].position = targetPositions[i];
            }
        }

        OnAllPartsShifted?.Invoke();
    }

}
