using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionManager : MonoBehaviour
{
    public Vector3 startPosition = Vector3.zero;
    public bool run = false;
    public float speed = 1.0f;
    public float delayBetweenActivations = 1.0f;
    public float initialDelay = 0.5f;

    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Queue<Transform> transformQueue = new Queue<Transform>();

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(initialDelay);

        foreach (Transform child in transform)
        {
            // Record original position
            originalPositions[child] = child.position;

            // Position at start position and disable
            child.position = startPosition;
            child.gameObject.SetActive(false);

            // Queue the transform for movement
            transformQueue.Enqueue(child);
        }

        // Start the MoveObjects coroutine
        StartCoroutine(MoveObjects());
    }

    private IEnumerator MoveObjects()
    {
        while (!run)
        {
            yield return null;
        }
        while (transformQueue.Count > 0)
        {
            Transform currentTransform = transformQueue.Peek();
            currentTransform.gameObject.SetActive(true);
            yield return new WaitForSeconds(delayBetweenActivations);
            Vector3 targetPosition = originalPositions[currentTransform];
            while (currentTransform.position != targetPosition)
            {
                currentTransform.position = Vector3.MoveTowards(currentTransform.position, targetPosition, speed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            // Remove this transform from the queue, it's reached its destination
            transformQueue.Dequeue();

            // Wait for the specified delay before moving the next object
            yield return new WaitForSeconds(delayBetweenActivations);
        }
    }
}
