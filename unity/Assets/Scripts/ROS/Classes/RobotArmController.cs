
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Geometry;
using RosMessageTypes.RemoteControl;
using RosMessageTypes.Sensor;
using RosMessageTypes.Shape;
using RosMessageTypes.Std;
using RosMessageTypes.Trajectory;
using RosMessageTypes.Ur;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.UrdfImporter;
using UnityEngine;
using UnityEngine.Serialization;

public enum RobotArmMovementTaskState
{
	Waiting,
	Error,
	Successful,
}

[Serializable]
public class RobotArmController
{
	[Header("Robot Arm Settings")]
	public GameObject myRobot;
	public List<Transform> sceneCollisionObjects;
	public List<Transform> robotArmAttachments;
	public bool sendWithConstraints = true;
	public bool realRobotConnection;

	public readonly string[] LinkNames =
	{
			"shoulder_link", "/upper_arm_link", "/forearm_link", "/wrist_1_link", "/wrist_2_link",
			"/wrist_3_link"
	};

	protected ROSConnection RosConnection;
	protected MonoBehaviour MonoBehaviorAttached;

	private static ArticulationBody[] _mJointArticulationBodies;
	private const int KNumRobotJoints = 6;
	private const float KJointAssignmentWait = 0.1f; //0.1
	private const float KPoseAssignmentWait = 0.5f; //0.5

	private bool _curStateSet = false;
	private bool _waitingForDashboardResponse = false;
	private bool _waitingForSceneInit = false;
	private bool _waitingForGripperToFinish = false;
	public RobotArmMovementTaskState armMovementTaskState;

	public RobotArmController()
	{
	}

	public void Initialize(ROSConnection ros, MonoBehaviour monoBehaviorAttached)
	{
		RosConnection = ros;
		MonoBehaviorAttached = monoBehaviorAttached;

		_mJointArticulationBodies = new ArticulationBody[LinkNames.Length];
		string linkName = string.Empty;
		for (int i = 0; i < KNumRobotJoints; i++)
		{

			linkName += LinkNames[i];
			_mJointArticulationBodies[i] =
					myRobot.transform.Find(linkName).GetComponent<ArticulationBody>();
		}

		MonoBehaviorAttached.StartCoroutine(InitRobotState());
	}

	private IEnumerator InitRobotState()
	{
		yield return new WaitUntil(() => RosConnection.HasConnectionThread);
		RosConnection.Subscribe<JointStateMsg>("/joint_states", RobotStateCallback);
		_curStateSet = false;
		yield return new WaitUntil(() => _curStateSet);

		yield return new WaitForSeconds(2f);
		yield return InitializeRosScene();
		yield return new WaitForSeconds(2f);

		if (realRobotConnection)
		{
			// yield return InteractWithUrDashboard(true);
		}
	}

	private void RobotStateCallback(JointStateMsg jointStateMsg)
	{
		// Debug.Log(jointStateMsg);
		if (_curStateSet)
			return;
		// Set the joint values for every joint
		for (int joint = 0; joint < _mJointArticulationBodies.Length; joint++)
		{
			string curJointName = _mJointArticulationBodies[joint]
					.GetComponent<UrdfJointRevolute>().jointName;
			int indexOfJoint = Array.IndexOf(jointStateMsg.name, curJointName);

			float result = (float)jointStateMsg.position[indexOfJoint] * Mathf.Rad2Deg;

			ArticulationDrive joint1XDrive = _mJointArticulationBodies[joint].xDrive;
			joint1XDrive.target = result;
			_mJointArticulationBodies[joint].xDrive = joint1XDrive;
		}

		_curStateSet = true;
	}

	private IEnumerator InteractWithUrDashboard(bool turnOn)
	{
		TriggerRequest request = new TriggerRequest();
		_waitingForDashboardResponse = false;
		string serviceName = turnOn
				? RobotManager.mRobotStartServiceName
				: RobotManager.mRobotStopServiceName;
		RosConnection.SendServiceMessage<TriggerResponse>(serviceName, request,
				ResponseUrDashboard);

		yield return new WaitUntil(() => _waitingForDashboardResponse == true);

	}

	private void ResponseUrDashboard(TriggerResponse response)
	{
		if (response.success)
			Debug.Log("Dashboard accepted request");
		else
			Debug.Log("Dashboard error : " + response.message);

		_waitingForDashboardResponse = true;
	}


	private IEnumerator InitializeRosScene()
	{
		MoverServiceRequest request = new MoverServiceRequest();
		request.type = "initialize";
		request.scene_objects = GetSceneCollisionObjects();
		request.static_attachments = GetStaticAttachments();
		RosConnection.SendServiceMessage<MoverServiceResponse>(RobotManager.mRobotArmServiceName,
				request, InitSceneRosCallback);
		yield return new WaitUntil(() => _waitingForSceneInit);
	}

	private void InitSceneRosCallback(MoverServiceResponse response)
	{
		if (response.success)
		{
			Debug.Log("Ros Scene its set up now we can move");
		}
		else
		{
			Debug.LogError("Ros Scene failed initialization");
		}

		_waitingForSceneInit = true;
	}

	public static double[] GetCurrentJointAngles()
	{
		double[] joints = new double[KNumRobotJoints];
		for (int i = 0; i < KNumRobotJoints; i++)
		{
			joints[i] = _mJointArticulationBodies[i].jointPosition[0];
		}

		return joints;
	}

	private CollisionBoxMsg[] GetSceneCollisionObjects()
	{
		CollisionBoxMsg[] collisionObjects = new CollisionBoxMsg[sceneCollisionObjects.Count];

		for (int i = 0; i < sceneCollisionObjects.Count; i++)
		{
			collisionObjects[i] = CreateCollisionBoxMsg(sceneCollisionObjects[i], false);
		}

		return collisionObjects;
	}

	private CollisionBoxMsg[] GetStaticAttachments()
	{
		CollisionBoxMsg[] staticAttachments = new CollisionBoxMsg[robotArmAttachments.Count];

		for (int i = 0; i < robotArmAttachments.Count; i++)
		{
			staticAttachments[i] = CreateCollisionBoxMsg(robotArmAttachments[i], true);
		}

		return staticAttachments;
	}

	public Vector3 GetPositionRelativeTo(Vector3 otherObjectPosition)
	{
		// Calculate the relative position
		Vector3 relativePosition = otherObjectPosition - myRobot.transform.position;

		return relativePosition;
	}


	private CollisionBoxMsg CreateCollisionBoxMsg(Transform transformObj, bool isAttachment)
	{

		CollisionBoxMsg boxMsg = new CollisionBoxMsg();
		boxMsg.id = transformObj.name;
		boxMsg.primitive.type = SolidPrimitiveMsg.BOX;
		boxMsg.primitive.dimensions = new double[3];
		boxMsg.primitive.dimensions[0] = transformObj.localScale.z;
		boxMsg.primitive.dimensions[1] = transformObj.localScale.x;
		boxMsg.primitive.dimensions[2] = transformObj.localScale.y;
		if (isAttachment)
		{
			boxMsg.pose.position = transformObj.localPosition.To<FLU>();
			boxMsg.pose.orientation = transformObj.localRotation.To<FLU>();
		}
		else
		{
			boxMsg.pose.position = transformObj.position.To<FLU>();
			boxMsg.pose.orientation = transformObj.rotation.To<FLU>();
		}

		return boxMsg;
	}


	private CollisionBoxMsg CreateCollisionBoxMsg(ScannedObjectSvg scannedObject)
	{
		Debug.Log(scannedObject.MyCollider.bounds);
		Vector3 posCenterObject = (scannedObject.transform.localPosition + (scannedObject.MyCollider.bounds.center - scannedObject.transform.position));
		CollisionBoxMsg boxMsg = new CollisionBoxMsg();
		boxMsg.id = scannedObject.name;
		boxMsg.primitive.type = SolidPrimitiveMsg.BOX;
		boxMsg.primitive.dimensions = new double[3];
		
		boxMsg.primitive.dimensions[0] = scannedObject.MyCollider.bounds.size.z;
		boxMsg.primitive.dimensions[1] = scannedObject.MyCollider.bounds.size.x;
		boxMsg.primitive.dimensions[2] = scannedObject.MyCollider.bounds.size.y;
		boxMsg.pose.position = (posCenterObject).To<FLU>();
		boxMsg.pose.orientation = scannedObject.transform.rotation.To<FLU>();
		
		return boxMsg;
	}

	public static OrientationConstraintsMsg GetOrientationConstraints(QuaternionMsg currentValue, float toleranceX = 100)
	{
		// Define orientation constraints for ROS
		OrientationConstraintsMsg orientationConstraint = new OrientationConstraintsMsg();
		orientationConstraint.orientation = currentValue;

		// Convert Unity Tolerances to ROS Tolerances
		float unityToleranceX = toleranceX; // Unity tolerance in degrees Default = 100
		float unityToleranceY = 50f; // Unity tolerance in degrees
		float unityToleranceZ = 50f; // Unity tolerance in degrees
		orientationConstraint.absolute_x_axis_tolerance =
				unityToleranceX * Mathf.Deg2Rad; // Convert to radians
		orientationConstraint.absolute_y_axis_tolerance =
				unityToleranceY * Mathf.Deg2Rad; // Convert to radians
		orientationConstraint.absolute_z_axis_tolerance =
				unityToleranceZ * Mathf.Deg2Rad; // Convert to radians
		
		orientationConstraint.header.frame_id = "base_link";
		orientationConstraint.link_name = "ee_link";
		orientationConstraint.weight = 1;
		
		return orientationConstraint;
	}

	//--------------------------------------- Move Robot To One Position ---------------------------------------

	public IEnumerator MoveRobot(Vector3 pos, Quaternion rotation, float toleranceX = 100)
	{
		if (!RosConnection.HasConnectionThread)
		{
			Debug.LogWarning("No connection to ROS in MoveRobot");
			yield break;
			
		}
		MoverServiceRequest request = new MoverServiceRequest();
		request.joints = GetCurrentJointAngles();
		request.type = "move";
		request.scale_velocity = .05f;//0.01f;
		request.scale_acceleration = .01f; //0.01f;
		request.destination_pose = new PoseMsg
		{
				position = pos.To<FLU>(),
				orientation = rotation.To<FLU>()
		};

		if (sendWithConstraints)
			request.constraint = GetOrientationConstraints(request.destination_pose.orientation, toleranceX);

		armMovementTaskState = RobotArmMovementTaskState.Waiting;
		RosConnection.SendServiceMessage<MoverServiceResponse>(
				RobotManager.mRobotArmServiceName, request, PositionTrajectoryResponse);
		yield return new WaitUntil(() => armMovementTaskState != RobotArmMovementTaskState.Waiting );

		// Debug.Log("Movement Finished");
	}
	
	public IEnumerator MoveRobot(MoverServiceRequest request)
	{
		if (!RosConnection.HasConnectionThread)
		{
			Debug.LogWarning("No connection to ROS in MoveRobot");
			armMovementTaskState = RobotArmMovementTaskState.Error;
			yield break;
		}
		
		armMovementTaskState = RobotArmMovementTaskState.Waiting;
		RosConnection.SendServiceMessage<MoverServiceResponse>(
				RobotManager.mRobotArmServiceName, request, PositionTrajectoryResponse);
		yield return new WaitUntil(() => armMovementTaskState != RobotArmMovementTaskState.Waiting );
	}

	private void PositionTrajectoryResponse(MoverServiceResponse response)
	{
		if (response.success)
		{
			Debug.Log("Trajectory returned.");
			MonoBehaviorAttached.StartCoroutine(ExecuteTrajectories(response));
		}
		else
		{
			Debug.LogError("No trajectory returned from MoverService.");
			armMovementTaskState = RobotArmMovementTaskState.Error;
		}
	}

	private IEnumerator ExecuteTrajectories(MoverServiceResponse response)
	{
		if (response.trajectories != null)
		{
			// For every trajectory plan returned
			for (int poseIndex = 0; poseIndex < response.trajectories.Length; poseIndex++)
			{
				// For every robot pose in trajectory plan
				foreach (JointTrajectoryPointMsg t in response.trajectories[poseIndex]
						         .joint_trajectory.points)
				{
					double[] jointPositions = t.positions;
					float[] result = jointPositions.Select(r => (float)r * Mathf.Rad2Deg).ToArray();

					// Set the joint values for every joint
					for (int joint = 0; joint < _mJointArticulationBodies.Length; joint++)
					{
						ArticulationDrive joint1XDrive = _mJointArticulationBodies[joint].xDrive;
						joint1XDrive.target = result[joint];
						_mJointArticulationBodies[joint].xDrive = joint1XDrive;
					}

					// Wait for robot to achieve pose for all joint assignments
					yield return new WaitForSeconds(KJointAssignmentWait);
				}

				// Wait for the robot to achieve the final pose from joint assignment
				yield return new WaitForSeconds(KPoseAssignmentWait);
			}

			armMovementTaskState = RobotArmMovementTaskState.Successful;
			// All trajectories have been executed, open the gripper to place the target cube
		}
	}

	public IEnumerator InteractWithGripper(bool activate)
	{
		SetIORequest request = new SetIORequest();
		request.fun = 1;
		request.pin = 0;
		request.state = activate ? 1 : 0;


		_waitingForGripperToFinish = false;
		RosConnection.SendServiceMessage<SetIOResponse>(
				RobotManager.mGripperServiceName,
				request, ResponseGripper);

		yield return new WaitUntil(() => _waitingForGripperToFinish);

	}

	private void ResponseGripper(SetIOResponse response)
	{
		if (response.success)
		{
			Debug.Log("Gripper Interaction success");
		}
		else
		{
			Debug.Log("Gripper Interaction error");
		}

		_waitingForGripperToFinish = true;
	}
}
