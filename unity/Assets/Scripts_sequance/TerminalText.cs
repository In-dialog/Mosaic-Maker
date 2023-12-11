using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TerminalText : MonoBehaviour
{
    public static TerminalText Instance { get; private set; }
    [SerializeField] private TextMeshPro infoText;

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
    private float progress = 0f;

    public float Progress
    {
        get { return progress; }
        set
        {
            progress = Mathf.Clamp01(value);

            //print(progress);
            if (infoText != null)
            {
                UpdateSingleLine($"{progress * 100}%", 1); // Update the text with the current progress
            }
        }
    }

    public void ScaningModeOn()
    {
        StopAllCoroutines();
        StartCoroutine(IncrementProgressToValue(0, 0.2f, 0.1f));

        UpdateStage("Scaning");
        UpdateProcess("Geometry");
    }
    public void NestingMode(bool Start)
    {
        if (Start)
        {

            UpdateStage("Nesting");
            UpdateProcess("Finding Pos.");

            StopAllCoroutines();
            StartCoroutine(IncrementProgressToValue(0.2f, 0.6f, 0.01f));

        }
        else
        {

            UpdateStage("Nesting");
            UpdateProcess("Finish");

            StopAllCoroutines();
            StartCoroutine(IncrementProgressToValue(0.6f, 1, 0.1f));

        }
    }







    // Create a coroutine that advances progress to a specified value
    private IEnumerator IncrementProgressToValue(float startValue, float targetValue, float speed)
    {
        progress = startValue;
        while (progress < targetValue)
        {
            progress += speed * Time.deltaTime;
            Progress = Mathf.Clamp01(progress); // Update the displayed progress and ensure it's within [0, 1]
            yield return null;
        }

        progress = targetValue;
        Progress = Mathf.Clamp01(progress); // Snap the progress to the target value and ensure it's within [0, 1]
    }


    public void UpdateStage(string msg)
    {
        UpdateSingleLine(msg, 2);
        print(msg);
    }

    public void UpdateProcess(string msg)
    {
        UpdateSingleLine(msg, 0);
        print(msg);

    }

    /// name - collor - postion - AABB size
    public void FormatText(string param1, string param2, string param3)
    {
        string newText = string.Format("{0}\n{1}\n{2}\n{3}", param1, param2, param3);
        infoText.text = newText;
    }

    public void UpdateSingleLine(string newValue, int index)
    {
        // Split the existing text into lines based on the newline marker.
        string[] lines = infoText.text.Split('\n');

        // Check if there are at least 3 lines.
        if (lines.Length >= 2)
        {
            // Update the third line with the new value.
            lines[index] = newValue;

            // Join the lines back together and update the text in the TextMeshProUGUI component.
            infoText.text = string.Join("\n", lines);
        }
    }

    public string FormatColor(Color32 color)
    {
        int decimalPlaces = 2;

        float r = Mathf.Round(color.r * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        float g = Mathf.Round(color.g * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        float b = Mathf.Round(color.b * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);

        return string.Format("{0}, {1}, {2}", r, g, b);
    }

    public string FormatPostion(Vector3 position)
    {
        int decimalPlaces = 2;

        float x = Mathf.Round((position.x * 10) * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        float y = Mathf.Round((position.y * 10) * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        return string.Format("{0}, {1}", x, y);
    }
}
