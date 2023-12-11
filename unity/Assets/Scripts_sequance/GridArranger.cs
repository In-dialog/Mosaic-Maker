using System.Collections.Generic;
using UnityEngine;

public class GridArranger : MonoBehaviour
{
    public bool ArrangeInGrid = false;
    public Vector3 startPosition = Vector3.zero;
    public Vector3 offset = new Vector3(1f, 0f, 1f);

    private Dictionary<int, Vector3> originalPositions = new Dictionary<int, Vector3>();

    private void Start()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            // Record original position
            originalPositions[i] = transform.GetChild(i).position;
        }
    }

    private void Update()
    {
        if (ArrangeInGrid)
        {
            ArrangeObjectsInGrid();
        }
        else
        {
            ResetObjectsPosition();
        }
    }

    private void ArrangeObjectsInGrid()
    {
        int gridSideLength = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
        for (int i = 0; i < transform.childCount; ++i)
        {
            int row = i / gridSideLength;
            int column = i % gridSideLength;
            Vector3 gridSpot = startPosition + new Vector3(column * offset.x, 0, row * offset.z);
            transform.GetChild(i).localPosition = gridSpot;
        }
    }

    private void ResetObjectsPosition()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (originalPositions.ContainsKey(i))
            {
                transform.GetChild(i).position = originalPositions[i];
            }
        }
    }
}
