using System.Collections;
using RosMessageTypes.RemoteControl;
using UnityEngine;

public enum PickAndPlaceStage
{
	PreAction,
	MainAction,
	PostAction,
	//To be deleted after finishing clean up
	PreGrasp,
	Grasp,
	PostGrasp,
	PreRelease,
	Release,
	PostRelease,
}

public interface ITask
{
	public static string Type = "move";

	public static PickAndPlaceStage[] Stages =
	{
			PickAndPlaceStage.PreAction,
			PickAndPlaceStage.MainAction,
			PickAndPlaceStage.PostAction,
	};

	public IEnumerator ExecuteTask(Vector3 pos, Quaternion rotation, RobotArmTaskController controller);

	public MoverServiceRequest GenerateStage(Vector3 pos, Quaternion rotation, PickAndPlaceStage curStage, RobotArmTaskController controller);
}
