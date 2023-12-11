using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NestingStatus : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI statusText;
    [SerializeField] public TextMeshProUGUI timmerText;
    [SerializeField] public TextMeshProUGUI logText;
    [SerializeField] public int maxLogLines = 10;

    private float elapsedTime = 0f;

    private Coroutine timeON;

    private static NestingStatus instance;

    public static NestingStatus Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NestingStatus>();
            }
            return instance;
        }
    }
    private void Start()
    {
        statusText.SetText("");
        timmerText.SetText("");
        logText.SetText("");
    }
    /// ------------------------------------------------------------------------
    public void SaveLog(ScannedObjectSvg obj)
    {
        SetLogBinSelected(obj);
    }

    public void SetOjectInfoDisplay(ScannedObjectSvg obj)
    {
        ObjectInfoDisplay(obj);
    }

    public void UpdateStatusText(string newStatus)
    {
        if (newStatus == "Nesting")
        {
            timeON = StartCoroutine(TimerCoroutine());
        }

        else if (timeON != null)
        {
            StopCoroutine(timeON);
            timmerText.SetText("--||--");
            timeON = null;
        }

        string currentText = statusText.text;
        string[] parts = currentText.Split('\n');

        if (newStatus == "Finished" || newStatus == "Rejected")
        {
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = "";
            }
        }

        if (parts.Length >= 4)
        {
            // Reconstruct the string with the new status
            string updatedText = $"{newStatus}\n\n{parts[2]}\n\n{parts[4]}";
            statusText.SetText(updatedText);
        }
        else
        {
            statusText.SetText(newStatus);
        }
    }

    /// ------------------------------------------------------------------------
    void ObjectInfoDisplay(ScannedObjectSvg obj)
    {
        string currentText = statusText.text;
        string[] parts = currentText.Split('\n');
        string collor = BreakDownColor(obj.MColor);
        string output = FormatOutput(parts[0], collor, obj.AreaTotal);
        //print(obj.name);
        statusText.SetText(output);
    }

    IEnumerator TimerCoroutine()
    {
        elapsedTime = 0f;

        while (true)
        {
            elapsedTime += 0.1f;
            timmerText.SetText(TruncateFloat(elapsedTime, 2).ToString());
            yield return new WaitForSeconds(0.1f); // Update every 0.1 seconds
        }
    }


    private void SetLogBinSelected(ScannedObjectSvg obj)
    {
        if (obj.MyBin == null) return;
        string formatMsg = FormatMessage(obj.name, obj.MColor, ExtractNumbers(obj.MyBin.name), obj.MyBin.MColor);
        LogToTextMeshPro(formatMsg);
    }

    private void LogToTextMeshPro(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm");
        string newEntry = $"({timestamp}) {message}";

        // Split the existing text into lines
        List<string> lines = new List<string>(logText.text.Split('\n'));

        // Add the new entry
        lines.Add(newEntry);

        // If the number of lines exceeds maxLogLines, remove the oldest entry
        if (lines.Count > maxLogLines)
        {
            lines.RemoveAt(0); // Remove the first/oldest line
        }

        // Join the lines back into a single string and update the TextMeshPro text
        logText.SetText(string.Join("\n", lines));
    }

    private static string FormatMessage(string partIndex, Color partColor, string binIndex, Color binColor)
    {
        string partColorStr = BreakDownColor(partColor);
        string binColorStr = BreakDownColor(binColor);
        return $"P{partIndex}({partColorStr}) -> B{binIndex}({binColorStr})";
    }


    static string BreakDownColor(Color color)
    {
        Color32 c = color;
        return $"{c.r}-{c.g}-{c.b}";
    }

    static string FormatOutput(string status, string colorString, float area)
    {
        return $"{status}\n\n{colorString}\n\n{area} cm²";
    }

    public static string ExtractNumbers(string input)
    {
        return string.IsNullOrEmpty(input) ? "" : (input.IndexOf('_') is int underscoreIndex && underscoreIndex < input.Length - 1) ? input.Substring(underscoreIndex + 1) : "";
    }

    public static float TruncateFloat(float number, int decimalPlaces)
    {
        float factor = Mathf.Pow(10f, decimalPlaces);
        return Mathf.Floor(number * factor) / factor;
    }

}
