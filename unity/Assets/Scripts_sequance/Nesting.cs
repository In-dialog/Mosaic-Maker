using System.Collections;
using UnityEngine;
using System;
using Random = UnityEngine.Random;


public class Nesting : MonoBehaviour
{
    // Define an event that will be raised when the nesting is over
    public event Action<ScannedObject> NestingOver;

    // Public static constant reference to the Nesting component
    public static Nesting Instance { get; private set; }

    public Transform fragmentsContainer; // Reference to the fragments container

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

    public void ExecuteNesting(ScannedObject scannedObject)
    {
        // Start the coroutine to shuffle the part around and wait for it to finish
        StartCoroutine(ExecuteNestingCoroutine(scannedObject));
    }

    private IEnumerator ExecuteNestingCoroutine(ScannedObject scannedObject)
    {
        //yield return StartCoroutine(ShufflePartCoroutine(scannedObject, GetTargetByIndex, 5));
        // Start the shuffle coroutine and wait for it to finish
        yield return StartCoroutine(ShufflePartCoroutine(scannedObject, GetTargetAroundPoint, Random.Range(1, 5)));

        // Raise the event to announce that the nesting is over
        NestingOver?.Invoke(scannedObject);
    }

    private delegate void GetTargetDelegate(ScannedObject scannedObject, out Vector3 targetPosition, out Quaternion targetRotation);

    private IEnumerator ShufflePartCoroutine(ScannedObject scannedObject, GetTargetDelegate getTarget, float duration)
    {
        // Save the original position and rotation
        Vector3 originalPosition = scannedObject.OriginalPosition;
        Quaternion originalRotation = scannedObject.Transform.localRotation;

        float shuffleDuration = duration; // Duration of the shuffle in seconds
        float startTime = Time.time;

        while (Time.time - startTime < shuffleDuration)
        {
            // Get the target position and rotation using the delegate
            Vector3 targetPosition;
            Quaternion targetRotation;
            getTarget(scannedObject, out targetPosition, out targetRotation);

            // Move and rotate the object to the target position and rotation
            MovePart(scannedObject, targetPosition, targetRotation);

            // Wait for a short time
            yield return new WaitForEndOfFrame();
        }

        // Return the object to its original position and rotation
        scannedObject.OriginalPosition = scannedObject.Transform.position;
        scannedObject.Transform.localRotation = originalRotation;
    }
    int randomIndex = 0;
    private void GetTargetByIndex(ScannedObject scannedObject, out Vector3 targetPosition, out Quaternion targetRotation)
    {
        randomIndex = GetNextIndex(randomIndex, fragmentsContainer.childCount - 1);
        targetPosition = fragmentsContainer.GetChild(randomIndex).position;
        targetRotation = Quaternion.Euler(
            0,
            Random.Range(-180, 180),
            0
        );
    }

    private void GetTargetAroundPoint(ScannedObject scannedObject, out Vector3 targetPosition, out Quaternion targetRotation)
    {
        float radius = 0.5f; // Define a radius for the random point around the target position
        Vector3 randomPoint = Random.insideUnitSphere * radius;
        targetPosition = scannedObject.TargetPosition + randomPoint;

        targetRotation = Quaternion.Euler(
            0,
            Random.Range(-180, 180),
            0
        );
    }
    private void MovePart(ScannedObject scannedObject, Vector3 targetPosition, Quaternion targetRotation)
    {
        scannedObject.Transform.position = Vector3.Lerp(scannedObject.Transform.position, targetPosition, 0.1f);
        scannedObject.Transform.localRotation = Quaternion.Lerp(scannedObject.Transform.localRotation, targetRotation, 0.1f);
    }

    private int GetNextIndex(int currentIndex, int listLength)
    {
        int increment = Random.Range(20, 50); // Generate a random increment value between 1 and 3
        int nextIndex = currentIndex + increment; // Add the increment to the current index

        if (nextIndex >= listLength - 1) // If the next index is greater than or equal to the list length, wrap around to the beginning
        {
            nextIndex = 0;
        }

        return nextIndex;
    }


}
