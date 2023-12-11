using System;
using System.Collections;
using UnityEngine;


[Serializable]
public class RobotArmTaskController : RobotArmController
{
    [Header("Robot Task Settings")]

    public Transform dummy;
    public Transform eeLink;
    public Transform camera;
    public Transform attachmentEnd;

    public Transform scanningPose;
    public Transform pickingPose;
    public Transform placementPose;
    public Transform releasePose;
    public Transform gluePose;
    public Transform rejectPose;
    public readonly Vector3 m_PickPoseOffset = Vector3.up;


    public void StartPickAndPlaceSequence(ScannedObjectSvg objToBePickUp)
    {
        MonoBehaviorAttached.StartCoroutine(PickAndPlaceSequence(objToBePickUp));
    }

    public IEnumerator MoveToScan()
    {
        yield return MoveRobot(scanningPose.position, scanningPose.rotation, 180f);
    }

    public IEnumerator MoveToPickUp() => PickSequence();

    public IEnumerator MoveToPlacement()
    {
        yield return MoveRobot(placementPose.position, placementPose.rotation, 360f);
    }

    public IEnumerator MoveToRelease()
    {
        yield return MoveRobot(releasePose.position, releasePose.rotation);
    }


    public IEnumerator PickAndPlaceSequence(ScannedObjectSvg objToBePickUp)
    {
        NestingStatus.Instance.UpdateStatusText("Picking");
        yield return PickSequence();
        if (armMovementTaskState == RobotArmMovementTaskState.Error)
            yield break;
        NestingStatus.Instance.UpdateStatusText("Applying Glue");
        yield return GlueSequence();
        if (armMovementTaskState == RobotArmMovementTaskState.Error)
            yield break;
        NestingStatus.Instance.UpdateStatusText("Placing");
        yield return PlaceSequence(objToBePickUp);
    }

    public IEnumerator RejectSequence()
    {
        yield return PickSequence();
        TaskReject taskReject = new TaskReject();
        yield return taskReject.ExecuteTask(rejectPose.position, rejectPose.rotation, this);
    }

    private IEnumerator PickSequence()
    {
        TaskPick taskPick = new TaskPick();

        yield return taskPick.ExecuteTask(pickingPose.position, pickingPose.rotation, this);
    }
    
    public IEnumerator GlueSequence()
    {
        TaskGlue taskPick = new TaskGlue();

        yield return taskPick.ExecuteTask(gluePose.position, gluePose.rotation, this);
    }

    private IEnumerator PlaceSequence(ScannedObjectSvg objToBePickUp)
    {
        TaskPlace taskPlace = new TaskPlace();

        Vector3 positionEeLink = CalculatePositionEeLink(eeLink.position, attachmentEnd.position, objToBePickUp.TargetPosition);
        Quaternion rotation = Quaternion.Euler(placementPose.rotation.eulerAngles.x, placementPose.rotation.eulerAngles.y, objToBePickUp.TargetRotation.eulerAngles.z);

        yield return taskPlace.ExecuteTask(positionEeLink, rotation, this);
    }
    

    public Vector3 CalculatePositionEeLink(Vector3 positionE, Vector3 positionAttachment,
            Vector3 positionObj)
    {
        Vector3 adjustment = positionObj - positionAttachment;
        positionE += adjustment;

        return positionE;
    }

    private Quaternion OrientEndEffector(Vector3 position)
    {
        Vector3 robotPos = myRobot.transform.position;
        Vector3 directionFromRobot = position - robotPos;
        directionFromRobot.y = 0; // Eliminate vertical difference to get a horizontal direction

        // Calculate the angle between the world's forward direction and the directionFromRobot on the XZ plane
        float angleY = Vector3.SignedAngle(Vector3.forward, directionFromRobot, Vector3.up);

        // Construct the rotation with the specified angles
        Quaternion rotation = Quaternion.Euler(90, angleY, 0);
        return rotation;
    }
}
