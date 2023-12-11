using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;
using TMPro;



public class ScannedObject
{
    public Transform Transform { get; set; }
    public Color Color { get; set; }
    public Vector3 OriginalPosition { get; set; }
    public Vector3 TargetPosition { get; set; }
    public string SvgOutline { get; set; } // New property

    public ScannedObject(Transform transform, Color color, Vector3 originalPosition, Vector3 targetPosition, string svgOutline)
    {
        Transform = transform;
        Color = color;
        OriginalPosition = originalPosition;
        TargetPosition = targetPosition;
        SvgOutline = svgOutline; // Set the new property
    }
    public ScannedObject(ScannedObject other)
    {
        Transform = other.Transform;
        Color = other.Color;
        OriginalPosition = other.OriginalPosition;
        TargetPosition = other.TargetPosition;
        SvgOutline = other.SvgOutline; // Set the new property
    }
}

public class Master : MonoBehaviour
{
    public ConveyorBelt conveyorBelt;
    public Scanner scanner;
    private bool running;
    bool FirstTime = true;

    private void Start()
    {
        conveyorBelt.PartReadyForScan += HandlePartReadyForScan;
        scanner.PartReadyForNesting += HandlePartReadyForNesting;
        Nesting.Instance.NestingOver += HandleNestingOver;
        Mover.Instance.MoveOver += AfterGraphicMove;
        PlaceObject.Instance.finishedMove += RobotFinished;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && running == false)
        {
            StartCoroutine(StartBelt());
        }
    }

    private IEnumerator StartBelt()
    {
        running = true;
        if (FirstTime)
        {
            yield return PatternVFX.Instance.StartCoroutine(PatternVFX.Instance.ProcessImg());
            FirstTime = false;
        }
        yield return StartCoroutine(conveyorBelt.MoveFirstObject());

        running = false;
    }

    void HandlePartReadyForScan(GameObject part)
    {
        TerminalText.Instance.ScaningModeOn();
        scanner.StartScanning(part);
    }

    void HandlePartReadyForNesting(ScannedObject part)
    {
        TerminalText.Instance.NestingMode(true);
        PartInfoDisplay.Instance.UpdatePart(part);
        PlaceObject.Instance.DisplayPart(part);
        Nesting.Instance.ExecuteNesting(part);
    }

    // Do something when the nesting is over
    void HandleNestingOver(ScannedObject part)
    {
        PlaceObject.Instance.SetTarget(part);
        TerminalText.Instance.NestingMode(false);
        PartInfoDisplay.Instance.StopUpdate();
        Mover.Instance.Move(part, 1.0f);
    }

    void AfterGraphicMove(ScannedObject part)
    {
        PlaceObject.Instance.PlaceObjectRobot();
    }

    void RobotFinished()
    {
        StartCoroutine(StartBelt());
    }

}
