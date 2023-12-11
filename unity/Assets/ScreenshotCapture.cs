using UnityEngine;
using System.IO;
using System.Collections;

[ExecuteInEditMode]
public class ScreenshotCapture : MonoBehaviour
{
    public bool run = false;
    public bool runVideoCapture = false;
    public bool StopCapture = false;
    public bool useCustomFrameRate = true;
    public int customFrameRate = 30;

    public Vector2Int resolution = new Vector2Int(1920, 1080);

    private void Update()
    {
        if (run)
        {
            TakeScreenshot();
            run = false;
        }

        if (runVideoCapture)
        {
            StartCoroutine(CaptureVideoFrames());
            runVideoCapture = false;
        }
    }

    private IEnumerator CaptureVideoFrames()
    {
        float frameDuration = 1.0f / (useCustomFrameRate ? customFrameRate : Application.targetFrameRate);
        while (!StopCapture)
        {
            TakeScreenshot();
            // System.GC.Collect(); // Uncomment this line only if absolutely necessary
            yield return new WaitForSeconds(frameDuration);
        }
    }

    void TakeScreenshot()
    {
        // Create a render texture
        RenderTexture rt = new RenderTexture(resolution.x, resolution.y, 24);
        Camera.main.targetTexture = rt;

        // Render the camera's view to the target texture
        Camera.main.Render();

        // Set the current render target back to null so that the camera renders to the screen again
        Camera.main.targetTexture = null;

        // Create a new texture and read the active RenderTexture into it
        Texture2D screenshot = new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        screenshot.Apply();

        // Convert the screenshot to PNG data
        byte[] bytes = screenshot.EncodeToPNG();

        // Properly destroy the RenderTexture
        Destroy(rt);

        // Destroy the texture to free up memory
        Destroy(screenshot);

        // Asynchronously save the screenshot
        System.Threading.Tasks.Task.Run(() => SaveScreenshotAsync(bytes));
    }

    private void SaveScreenshotAsync(byte[] bytes)
    {
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string directoryPath = Path.Combine(desktopPath, "Mosaic_Output");
        Directory.CreateDirectory(directoryPath);
        string filename = $"screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = Path.Combine(directoryPath, filename);
        File.WriteAllBytes(path, bytes);

        // Note: Using Debug.Log in a separate thread can cause issues. You might want to signal the main thread to log the message or use other means of logging.
    }
}
