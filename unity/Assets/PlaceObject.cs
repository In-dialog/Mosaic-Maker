using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class PlaceObject : MonoBehaviour
{
    //private List<ScannedObject> placedObjects = new List<ScannedObject>();
    public Action finishedMove;
    public GameObject finalDestination;
    public static PlaceObject Instance { get; private set; }
    public bool debug;
    private ScannedObject curentObject;

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



    public void PlaceObjectRobot()
    {
        if (debug)
            StartCoroutine(TestMove(curentObject));
        else if (RobotManager.Instance != null)
        {
            // MovementRosPublisher.Instance.CustomPickAndPlace(curentObject);
        }
    }

    public void DisplayPart(ScannedObject part)
    {

        // Create a copy of the scanned object and add it to the placedObjects list
        ScannedObject newObject = new ScannedObject(part);
        // Instantiate a new game object and apply the scanned object's data
        GameObject obj = GameObject.Instantiate(part.Transform.gameObject);
        obj.layer = LayerMask.NameToLayer("robot");
        // Place it in the pick up position

        newObject.Transform = obj.transform;
        newObject.Transform.parent = this.transform;
        newObject.Transform.localPosition = Vector3.zero;
        newObject.Transform.localScale = Vector3.one;
        newObject.OriginalPosition = newObject.Transform.position;

        curentObject = newObject;
    }

    public void SetTarget(ScannedObject part)
    {
        //SetParentWithWorldTransform(curentObject.Transform, finalDestination.transform);
        curentObject.Transform.parent = finalDestination.transform;

        // Convert targerPosition
        curentObject.Transform.localPosition = new Vector3(part.TargetPosition.x, 0, part.TargetPosition.y);
        curentObject.TargetPosition = curentObject.Transform.position;
        // Return to original
        //curentObject.Transform.parent = this.transform;
        curentObject.Transform.position = this.transform.position;



    }

    //public void DisplayPart(ScannedObject part)
    //{
    //    // Create a copy of the scanned object and add it to the placedObjects list
    //    ScannedObject newObject = new ScannedObject(part);
    //    placedObjects.Add(newObject);

    //    // Instantiate a new game object and apply the scanned object's data
    //    GameObject obj = GameObject.Instantiate(part.Transform.gameObject);

    //    // Convert targerPosition
    //    newObject.Transform = obj.transform;
    //    obj.transform.localScale = Vector3.one * 0.1f;
    //    obj.transform.parent = finalDestination.transform;
    //    obj.transform.localPosition = new Vector3(part.TargetPosition.x, 0, part.TargetPosition.y);
    //    newObject.TargetPosition = obj.transform.position;

    //    // Place it in the pick up position
    //    obj.transform.parent = this.transform;
    //    obj.transform.localPosition = Vector3.zero;
    //    newObject.OriginalPosition = obj.transform.localPosition;
    //    curentObject = newObject;
    //}






    private IEnumerator TestMove(ScannedObject part)
    {
        Vector3 originalPosition = part.OriginalPosition;
        Vector3 targetPosition = part.TargetPosition;
        float moveSpeed = 1.5f; // This can be adjusted based on your needs

        while (Vector3.Distance(part.Transform.position, targetPosition) > 0.01f) // A small threshold to account for floating-point inaccuracies
        {
            part.Transform.position = Vector3.MoveTowards(part.Transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        part.Transform.position = targetPosition;
        FinishedMove();
    }

    public void FinishedMove()
    {
        finishedMove?.Invoke(); // Invoke the finishedMove action after moving the object
        curentObject = null;
    }




}
