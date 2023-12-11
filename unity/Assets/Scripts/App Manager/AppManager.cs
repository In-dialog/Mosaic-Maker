using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum AppState
{
    Running,
    Stopped
}

public partial class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    public MosaicNesting mosaicNesting;
    public RobotManager robotManager;
    public UiManager uiManager;

    private Coroutine _mainLoop;
    private AppState _currentState;
    private bool _objPlaced;

    void Awake()
    {
        // Check if an instance already exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Makes sure the object is not destroyed on scene load
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicate instances
        }
    }
    

    private IEnumerator Start()
    {
        yield return LoadSceneAdditively("Scene_2D");
        yield return LoadSceneAdditively("Scene_3D");
        NestingStatus.Instance.UpdateStatusText("idle");
        mosaicNesting = FindObjectOfType<MosaicNesting>();
        if (mosaicNesting != null)
            mosaicNesting.ObjectPlacedState += ObjectMosaicStatus;
        robotManager = RobotManager.Instance;
        uiManager = UiManager.Instance;
        _currentState = AppState.Stopped;
    }

    public void StartProcess()
    {
        if (_mainLoop != null)
            return;
        _currentState = AppState.Running;
        uiManager.StartingProcess();
        _mainLoop = StartCoroutine(MainProcess());
    }

    public void StopProcess()
    {
        if (_mainLoop == null)
            return;
        _currentState = AppState.Stopped;
        NestingStatus.Instance.UpdateStatusText("Stopped");
        uiManager.StoppingProcess();
        robotManager.CleanUp();
        StopCoroutine(_mainLoop);
        _mainLoop = null;
    }

    IEnumerator MainProcess()
    {
        while (_currentState == AppState.Running)
        {
            yield return robotManager.MoveToScan();
            NestingStatus.Instance.UpdateStatusText("Scanning");
            // Get Object From Camera
            yield return robotManager.ObtainObjectFromCamera();
            // Get last scanned object 
            ScannedObjectSvg lastObj = robotManager.GetLastScannedObject();
            // Update status text
            NestingStatus.Instance.UpdateStatusText("Nesting");
            NestingStatus.Instance.SetOjectInfoDisplay(lastObj);
            
            yield return mosaicNesting.PlaceNewObject(lastObj);
            NestingStatus.Instance.SaveLog(lastObj);

            // Place Object
            if (_objPlaced)
            {
                yield return robotManager.PickAndPlace(lastObj);
                SaveAppState();
            }
            else
            {
                NestingStatus.Instance.UpdateStatusText("Rejected");
                yield return robotManager.MoveToReject();
            }

            NestingStatus.Instance.UpdateStatusText("Finished");
        }
    }

    public void ClearAllPlacedObjects()
    {
        Debug.Log("Clear Clicked");
        UiManager.Instance.DisableButton("Clear");
        mosaicNesting.ClearAllPlacedObjects();
        SaveAppState();
        UiManager.Instance.EnableButton("Clear");
    }


    private void ObjectMosaicStatus(bool status)
    {
        _objPlaced = status;
    }


    private static IEnumerator LoadSceneAdditively(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public static bool Scene3dIsLoaded => IsSceneLoaded("Scene_3D");
    public static bool Scene2dIsLoaded => IsSceneLoaded("Scene_2D");

    private static bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }
}