
using System.Collections;
using RosMessageTypes.Geometry;
using RosMessageTypes.RemoteControl;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class TaskReject : ITask
{
	public IEnumerator ExecuteTask(Vector3 pos, Quaternion rotation,
			RobotArmTaskController controller)
	{
		foreach (PickAndPlaceStage stage in ITask.Stages)
		{
			MoverServiceRequest request = GenerateStage(pos, rotation, stage, controller);
			yield return controller.MoveRobot(request);
			if (controller.armMovementTaskState == RobotArmMovementTaskState.Error)
			{
				Debug.LogWarning($"Stage [{stage}] has failed");
				yield break;
			}

			if (stage == PickAndPlaceStage.MainAction)
			{
				yield return controller.InteractWithGripper(false);
				yield return new WaitForSeconds(1f);
			}
		}
	}

	public MoverServiceRequest GenerateStage(Vector3 pos, Quaternion rotation,
			PickAndPlaceStage curStage, RobotArmTaskController controller)
	{
		MoverServiceRequest request = new MoverServiceRequest();
		request.type = ITask.Type;
		request.destination_pose = new PoseMsg();
		request.destination_pose.orientation = rotation.To<FLU>();
		request.joints = RobotArmController.GetCurrentJointAngles();

		// Setting Up the Speed, Acceleration and Constraints
		switch (curStage)
		{
			case PickAndPlaceStage.PreAction:
				request.destination_pose.position =
						(pos + controller.m_PickPoseOffset * 0.25f).To<FLU>();
				request.constraint =
						RobotArmController.GetOrientationConstraints(
								request.destination_pose.orientation, 180);
				request.scale_velocity = 0.05f;
				request.scale_acceleration = 0.01f;
				break;

			case PickAndPlaceStage.MainAction:
				request.destination_pose.position = (pos).To<FLU>();
				request.constraint =
						RobotArmController.GetOrientationConstraints(
								request.destination_pose.orientation, 100);
				request.scale_velocity = 0.05f;
				request.scale_acceleration = 0.01f;
				break;

			case PickAndPlaceStage.PostAction:
				request.destination_pose.position =
						(pos + controller.m_PickPoseOffset * 0.10f).To<FLU>();
				request.constraint =
						RobotArmController.GetOrientationConstraints(
								request.destination_pose.orientation, 100);
				request.scale_velocity = 0.05f;
				request.scale_acceleration = 0.01f;
				break;
		}

		controller.dummy.position = request.destination_pose.position.From<FLU>();
		controller.dummy.rotation = request.destination_pose.orientation.From<FLU>();
		Debug.Log($"{curStage} with {request}");
		return request;
	}
}