using System;
using System.Collections;
using RosMessageTypes.Assets;
using RosMessageTypes.CommandManager;
using RosMessageTypes.PointCloudProcessing;
using RosMessageTypes.RemoteControl;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using RosMessageTypes.Std;
using RosMessageTypes.Ur;
using UnityEngine.Serialization;

public class RobotManager : MonoBehaviour
{
    public static string mRobotArmServiceName = "listen_move";
    public static string mCameraServiceName = "cameraScanning";
    public static string mGripperServiceName = "ur_hardware_interface/set_io";
    private static string mPlatformServiceName = "robot/command_manager/command";
    private static string mPlatformReturnTopic = "robot/command_manager/action/result";

    private static string mRobotInitServiceName = "ur_hardware_interface/dashboard/";
    public static string mRobotStartServiceName = mRobotInitServiceName + "play";
    public static string mRobotStopServiceName = mRobotInitServiceName + "stop";
    
    private ROSConnection _ros;


    [SerializeField] private RobotArmTaskController robotArmController;
    [SerializeField] private CameraController cameraController;

    public CameraController GetCameraController => cameraController;

    private string[] CmdRobotPlatform =
    {
            "DOCK robot_docking_station_laser robot_base_footprint -0.5 0.0 0.0",
            "GOTO_TAG p1",
            "GOTO_TAG p2"
    };
    private bool _waitResPlatform = false;

    public static RobotManager Instance { get; private set; }

    private void Awake()
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
    
    private void Start()
    {
        _ros = ROSConnection.GetOrCreateInstance();
        _ros.RegisterRosService<MoverServiceRequest, MoverServiceResponse>(mRobotArmServiceName);
        _ros.RegisterRosService<ScanningRequest, ScanningResponse>(mCameraServiceName);
        if (robotArmController.realRobotConnection)
        {
            _ros.RegisterRosService<TriggerRequest, TriggerResponse>(mRobotStartServiceName);
            _ros.RegisterRosService<TriggerRequest, TriggerResponse>(mRobotStopServiceName);
            _ros.RegisterRosService<SetIORequest, SetIOResponse>(mGripperServiceName);

            _ros.RegisterRosService<commandRequest, commandResponse>(mPlatformServiceName);
            _ros.Subscribe<RobotSimpleCommandActionResultMsg>(mPlatformReturnTopic, PlatformResult);
        }

        robotArmController.Initialize(_ros, this);
        cameraController.Initialize(robotArmController, _ros);
    }


    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Alpha1))
        //     StartCoroutine(MoveToScan());
        // if (Input.GetKeyDown(KeyCode.Alpha2))
        //     StartCoroutine(MoveToPickUp());
        // if (Input.GetKeyDown(KeyCode.Alpha3))
        //     StartCoroutine(SuckIt());
        // if (Input.GetKeyDown(KeyCode.Alpha4))
        //     StartCoroutine(MoveToGlue());
        // if (Input.GetKeyDown(KeyCode.Alpha5))
        //     StartCoroutine(ReleaseObject());
        // if (Input.GetKeyDown(KeyCode.Alpha0))
        //     StartCoroutine(ObtainObjectFromCamera());
    }

    public IEnumerator MoveToScan() => robotArmController.MoveToScan();
    public IEnumerator MoveToPickUp() => robotArmController.MoveToPickUp();
    public IEnumerator SuckIt() => robotArmController.InteractWithGripper(true);
    public IEnumerator ReleaseObject() => robotArmController.InteractWithGripper(false);
    public IEnumerator MoveToReleasePrePose() => robotArmController.MoveToRelease();
    public IEnumerator MoveToRelease() => robotArmController.MoveToPlacement();
    public IEnumerator MoveToReject() => robotArmController.RejectSequence();
    public IEnumerator MoveToGlue() => robotArmController.GlueSequence();

    public IEnumerator PickAndPlace(ScannedObjectSvg newObject) =>
            robotArmController.PickAndPlaceSequence(newObject);

    public IEnumerator ObtainObjectFromCamera() => cameraController.DetectObject();
    public ScannedObjectSvg GetLastScannedObject() => cameraController.objectDetected;
    
    public void CleanUp()
    {
        if (cameraController.objectDetected != null)
        {
            Destroy(cameraController.objectDetected.gameObject);
            cameraController.objectDetected = null;
        }
    }
    
    

    // private IEnumerator SequenceRobotScanWithPickUp()
    // {
    //     // while (!ObjectDetectedCorrectly)
    //     // {
    //     //     yield return robotArmController.MoveToScan();
    //     //     _cameraController.ScannedObject = null;
    //     //     _cameraController.GetCameraData();
    //     //     Debug.Log("Object Getting Scanned");
    //     //     yield return new WaitUntil(() => _cameraController.ScannedObject != null);
    //     //     yield return new WaitForSeconds(3);
    //     //     if (ObjectDetectedCorrectly == false)
    //     //     {
    //     //         Destroy(_cameraController.ScannedObject.gameObject);
    //     //     }
    //     // }
    //     //
    //     // ObjectDetectedCorrectly = false;
    //
    //     if (_cameraController.ScannedObject != null)
    //     {
    //         Debug.Log("Starting Pick and Place");
    //         _cameraController.ScannedObject.TargetPosition = TargetPlacement.position;
    //         yield return robotArmController.PickSequence(_cameraController.ScannedObject);
    //     }
    //     else
    //     {
    //         Debug.Log("No object detected");
    //     }
    // }
    
    private void PlatformResult(RobotSimpleCommandActionResultMsg commandStringResultMsg)
    {
        Debug.Log("Platform result =" + commandStringResultMsg);
    }
    
    IEnumerator MoveRobotPlatform(string cmd)
    {
        commandRequest request = new commandRequest();
        request.command = cmd;
        _waitResPlatform = false;
        Debug.LogFormat("Sending message {0}", request.command);
        _ros.SendServiceMessage<commandResponse>(mPlatformServiceName, request,
                ResponsePlatformMove);
        yield return new WaitUntil(() => _waitResPlatform);
    }

    IEnumerator GoToDocking(string point)
    {
        yield return MoveRobotPlatform(point);
        yield return new WaitForSeconds(2);
        yield return MoveRobotPlatform(CmdRobotPlatform[0]);
    }

    private void ResponsePlatformMove(commandResponse response)
    {
        Debug.Log(response);
        _waitResPlatform = true;
    }

    // public const string k_RosMessageName = "robot_simple_command_manager_msgs/CommandStringResult";


}
