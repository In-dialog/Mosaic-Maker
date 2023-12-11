using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;


public class UiManager : MonoBehaviour
{
    [Header("UI Element of the app")]
    public RawImage displayImage;
    public GameObject placeHolderCurrentElement;

    public RectTransform popUpNewObject;
    public GameObject popUpObjectPlaceholder;

    private GameObject _displayCurrentObject;
    private Dictionary<string, Button> _mainMenuButtons;
    private Dictionary<string, TMP_InputField> _parametersInput;
    private Button _saveParamsButton;

    [DllImport("libUnityFilePopUp", EntryPoint = "OpenFilePanel")]
    private static extern System.IntPtr OpenFilePanel();

    public static UiManager Instance { get; private set; }
    private const string SERVER_URL = "http://90.16.122.125:5000/upload";

    // private const string SERVER_URL = "http://127.0.0.1:5100/upload";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ParametersManager.LoadParameters();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        Canvas canvasMain = transform.GetComponent<Canvas>();
        displayImage = GameObject.Find("Display Image").GetComponent<RawImage>();
        Init_Pop_Up(canvasMain);
        Init_Buttons_MainMenu();
        InitializeParameters();
        placeHolderCurrentElement = GameObject.Find("PlaceHolder_2D_NewObject");
    }

    private void Init_Pop_Up(Canvas canvas)
    {
        popUpNewObject = canvas.transform.Find("PopUp_UI_Object").GetComponent<RectTransform>();
        AttachFunctionToButton(popUpNewObject.transform.Find("Buttons Holder"), "Yes", ObjectItsCorrect);
        AttachFunctionToButton(popUpNewObject.transform.Find("Buttons Holder"), "No", ObjectItsIncorrect);
        popUpObjectPlaceholder = (popUpNewObject.Find("PlaceHolder").transform.gameObject);
        popUpNewObject.gameObject.SetActive(false);
    }

    private void Init_Buttons_MainMenu()
    {
        RectTransform buttonHolder = GameObject.Find("State_Controller").transform as RectTransform;
        _mainMenuButtons = new Dictionary<string, Button>();

        // Initialize buttons
        InitButton(buttonHolder, "Upload", UploadImage, true);
        InitButton(buttonHolder, "Segment", SendRequest, false);

        buttonHolder = GameObject.Find("State Buttons").transform as RectTransform;

        if (AppManager.Instance != null)
        {
            InitButton(buttonHolder, "Save", AppManager.Instance.SaveAppState, false);
            InitButton(buttonHolder, "Load", AppManager.Instance.LoadApp,
            AppManager.Instance.SaveExists());
            InitButton(buttonHolder, "Start", AppManager.Instance.StartProcess, false);
            InitButton(buttonHolder, "Stop", AppManager.Instance.StopProcess, false);
            InitButton(buttonHolder, "Clear", AppManager.Instance.ClearAllPlacedObjects, false);
        }
    }

    private void InitializeParameters()
    {
        _parametersInput = new Dictionary<string, TMP_InputField>();
        
        GameObject holderInput = GameObject.Find("Input_Param");
        Transform tilesParam = holderInput.transform.Find("Tiles_Param");

        InitInputField(tilesParam, "Gap", ParametersManager.GapTiles);
        InitInputField(tilesParam, "Offset_Z", ParametersManager.OffsetZAxisTiles);
        InitInputField(tilesParam, "Depth", ParametersManager.DepthTiles);
        
        Transform mosaicParam = holderInput.transform.Find("Mosaic_Param");

        InitInputField(mosaicParam, "Dominant_Colors", ParametersManager.NumberDominantColors);
        InitInputField(mosaicParam.Find("Size"), "X", ParametersManager.SizeMosaic.x);
        InitInputField(mosaicParam.Find("Size"), "Y", ParametersManager.SizeMosaic.y);

        AttachFunctionToButton(holderInput.transform, "Save",
                () => ParametersManager.UpdateParameters(_parametersInput));
    }

    private void InitInputField(Transform parent, string name, float startVal)
    {
        Transform component = parent.Find(name);
        if (component == null)
        {
            Debug.LogWarning($"Could not find {name} in parent {parent.name}");
            return;
        }

        TMP_InputField input = component.GetComponentInChildren<TMP_InputField>();
        if (input != null)
        {
            _parametersInput.Add(name, input);
            input.text = startVal.ToString();
        }
        else
        {
            Debug.LogWarning($"Input {name} does not exists in parent {parent.name}");
        }
    }

    private void InitButton(RectTransform container, string buttonName, UnityAction action, bool isInteractable)
    {
        Button button = AttachFunctionToButton(container, buttonName, action);
        if (button != null)
        {
            button.interactable = isInteractable;
            _mainMenuButtons.Add(buttonName, button);
        }
    }

    // Function to enable a button
    public void EnableButton(string buttonName)
    {
        if (_mainMenuButtons.TryGetValue(buttonName, out Button button))
        {
            button.interactable = true;
        }
        else
        {
            Debug.LogError($"EnableButton: Button '{buttonName}' not found.");
        }
    }

    // Function to disable a button
    public void DisableButton(string buttonName)
    {
        if (_mainMenuButtons.TryGetValue(buttonName, out Button button))
        {
            button.interactable = false;
        }
        else
        {
            Debug.LogError($"DisableButton: Button '{buttonName}' not found.");
        }
    }

    public void StartingProcess()
    {
        DisableButton("Upload");
        DisableButton("Segment");
        DisableButton("Save");
        DisableButton("Load");
        DisableButton("Start");
        DisableButton("Clear");
        EnableButton("Stop");
    }

    public void StoppingProcess()
    {
        EnableButton("Upload");
        EnableButton("Segment");
        EnableButton("Save");
        EnableButton("Load");
        EnableButton("Start");
        EnableButton("Clear");
        DisableButton("Stop");

        if (_displayCurrentObject != null)
        {
            Destroy(_displayCurrentObject);
            _displayCurrentObject = null;
        }
    }


    private void UploadImage()
    {
        string path = Marshal.PtrToStringAnsi(OpenFilePanel());
        Debug.Log("Selected file path: " + path);
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadTexture(path));
        }
    }
    private IEnumerator LoadTexture(string filePath)
    {
        // Check if file exists
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found at " + filePath);
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2); // Create a new Texture (size does not matter, it will be replaced)
        bool isLoaded = texture.LoadImage(fileData); // Load the image data into the texture

        if (isLoaded)
        {
            displayImage.texture = texture; // Assign the loaded texture to the RawImage
            EnableButton("Segment");
        }
        else
        {
            Debug.LogError("Failed to load texture from " + filePath);
        }
    }

    private void SendRequest()
    {
        StartCoroutine(SendDataToServer((displayImage.texture as Texture2D).GetPNGData(), ParametersManager.NumberDominantColors));
    }

    private IEnumerator SendDataToServer(byte[] imageData, int additionalData)
    {
        DisableButton("Segment");

        // Create a form and add the image data
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, "image.jpeg", "application/octet-stream");

        // Add the additional integer data
        form.AddField("numberOfDominantColors", additionalData);

        // Create the request with the form
        UnityWebRequest www = UnityWebRequest.Post(SERVER_URL, form);

        yield return www.SendWebRequest();
        Array.Clear(imageData, 0, imageData.Length);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + www.error);
        }
        else
        {
            // Parse the JSON response
            ServerResponseColorSegmentation response = JsonUtility.FromJson<ServerResponseColorSegmentation>(www.downloadHandler.text);
            yield return AppManager.Instance.mosaicNesting.Initialize_From_Server(response);
            EnableButton("Start");
        }
        EnableButton("Segment");
    }

    // Coroutine that sends the request and waits for the response
    private IEnumerator SendDataToServer(byte[] imageData)
    {
        DisableButton("Segment");
        UnityWebRequest www = UnityWebRequest.Post(SERVER_URL, UnityWebRequest.kHttpVerbPOST);
        www.uploadHandler = new UploadHandlerRaw(imageData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/octet-stream");

        yield return www.SendWebRequest();
        Array.Clear(imageData, 0, imageData.Length);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + www.error);
        }
        else
        {
            // Parse the JSON response
            ServerResponseColorSegmentation response = JsonUtility.FromJson<ServerResponseColorSegmentation>(www.downloadHandler.text);
            yield return AppManager.Instance.mosaicNesting.Initialize_From_Server(response);
            EnableButton("Start");
            //Send response to mosaic nesting
        }
        EnableButton("Segment");
    }



    public IEnumerator OpenPopUp(ScannedObjectSvg scannedObjectSvg)
    {
        if (_displayCurrentObject)
        {
            Destroy(_displayCurrentObject);
            _displayCurrentObject = null;
        }

        popUpNewObject.gameObject.SetActive(true);
        yield return new WaitUntil(() => popUpNewObject.gameObject.activeInHierarchy);

        SetupObject2DVisualization(popUpObjectPlaceholder.transform as RectTransform, scannedObjectSvg);
    }

    public void SetUp2dVisualization(ScannedObjectSvg newObject) =>
            SetupObject2DVisualization(placeHolderCurrentElement.transform,
                    newObject);


    private void SetupObject2DVisualization(RectTransform uiElement, ScannedObjectSvg objectDetected)
    {
        Debug.Log(uiElement);
        
        
        if (_displayCurrentObject == null)
        {
            _displayCurrentObject = Instantiate(objectDetected.gameObject);
            _displayCurrentObject.transform.localScale = new Vector3(10, 10, 1);
        }

        Vector2 screenPosition = (uiElement).position;
        Vector3 worldPosition = (new Vector3(screenPosition.x, screenPosition.y, -5));

        _displayCurrentObject.transform.position = worldPosition;
    }


    private void SetupObject2DVisualization(Transform placeHolder, ScannedObjectSvg objectDetected)
    {
        Destroy(_displayCurrentObject);
        _displayCurrentObject = Instantiate(objectDetected.gameObject);
        _displayCurrentObject.transform.localScale = new Vector3(10, 10, 1);
        _displayCurrentObject.transform.position = placeHolder.position;
    }


    public Vector3 GetWorldPositionFromRectTransform(RectTransform rectTransform, Camera uiCamera, float zDistanceFromCamera)
    {
        if (uiCamera == null)
        {
            Debug.LogError("UI Camera is null.");
            return Vector3.zero;
        }

        // Calculate the RectTransform's center in local space (assuming pivot is at the center)
        Vector3 localCenter = new Vector3((rectTransform.rect.min.x + rectTransform.rect.max.x) / 2,
                (rectTransform.rect.min.y + rectTransform.rect.max.y) / 2,
                0f);

        // Convert local center to world space
        Vector3 worldCenter = rectTransform.TransformPoint(localCenter);

        // Adjust the Z coordinate based on the camera's position and the desired distance
        Vector3 worldPosition = new Vector3(worldCenter.x, worldCenter.y, uiCamera.transform.position.z + zDistanceFromCamera);

        return worldPosition;
    }




    private void ObjectItsCorrect()
    {
        AppManager.Instance.robotManager.GetCameraController.ObjectItsCorrect();
        popUpNewObject.gameObject.SetActive(false);
        _displayCurrentObject.transform.position = placeHolderCurrentElement.transform.position;
    }

    private void ObjectItsIncorrect()
    {
        Object.Destroy(_displayCurrentObject);
        _displayCurrentObject = null;
        AppManager.Instance.robotManager.GetCameraController.ObjectItsIncorrect();
        popUpNewObject.gameObject.SetActive(false);
    }



    private Button AttachFunctionToButton(Transform parent, string buttonName, UnityAction functionToAttach)
    {
        if (parent == null)
        {
            Debug.LogError("AttachFunctionToButton: Parent transform is null.");
            return null;
        }

        if (string.IsNullOrEmpty(buttonName))
        {
            Debug.LogError("AttachFunctionToButton: Button name is not specified.");
            return null;
        }

        if (functionToAttach == null)
        {
            Debug.LogError($"AttachFunctionToButton: No function provided to attach to the button '{buttonName}' under parent '{parent.name}'.");
            return null;
        }

        Transform buttonTransform = parent.Find(buttonName);

        if (buttonTransform == null)
        {
            Debug.LogError($"AttachFunctionToButton: Child GameObject '{buttonName}' not found under parent '{parent.name}'.");
            return null;
        }

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"AttachFunctionToButton: No Button component found on the GameObject '{buttonName}' under parent '{parent.name}'.");
            return null;
        }

        button.onClick.AddListener(functionToAttach);
        return button;
    }

}
