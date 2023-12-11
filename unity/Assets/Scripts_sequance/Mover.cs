using System;
using Random = UnityEngine.Random;

using System.Collections;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public event Action<ScannedObject> MoveOver;

    // Public static constant reference to the Mover component
    public static Mover Instance { get; private set; }


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

    public void Move(ScannedObject scannedObject, float moveSpeed)
    {
        /// replace with live trackking from the robot 
        StartCoroutine(MoveCoroutine(scannedObject, moveSpeed));
    }

    private IEnumerator MoveCoroutine(ScannedObject scannedObject, float moveSpeed)
    {
        float distanceToMove = Vector3.Distance(scannedObject.OriginalPosition, scannedObject.TargetPosition);
        float timeToMove = distanceToMove / moveSpeed;
        float startTime = Time.time;
        while (Time.time - startTime < timeToMove)
        {
            float t = (Time.time - startTime) / timeToMove;
            scannedObject.Transform.position = Vector3.Lerp(scannedObject.OriginalPosition, scannedObject.TargetPosition, t);
            yield return new WaitForEndOfFrame();
        }
        scannedObject.Transform.position = scannedObject.TargetPosition;
        MoveOver?.Invoke(scannedObject);

    }
}
