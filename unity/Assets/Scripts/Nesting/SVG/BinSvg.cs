using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BinDataSave
{
    [SerializeField] public List<SvgSaveData> placedObjectsData;
    [SerializeField] public SvgSaveData binSaveData;

    public BinDataSave(BinSvg bin)
    {
        placedObjectsData = new List<SvgSaveData>(bin.placer.PlacedObjects.Count);
        foreach (ScannedObjectSvg scannedObjectSvg in bin.placer.PlacedObjects)
        {
            placedObjectsData.Add(scannedObjectSvg.GetSaveData());
        }

        binSaveData = new SvgSaveData(bin);
    }

    public BinSvg InstantiateBinWithObject()
    {
        List<ScannedObjectSvg> objectPlaced = new List<ScannedObjectSvg>();
        GameObject savedBin = new GameObject(binSaveData.Name);
        BinSvg binSvg = savedBin.AddComponent<BinSvg>();
        binSvg.GenerateObject(binSaveData.SvgString);
        // binSvg.transform.position =
        //         new Vector3(binSaveData.PosX, binSaveData.PosY, binSaveData.PosZ);

        foreach (SvgSaveData saveData in placedObjectsData)
        {
            objectPlaced.Add(saveData.InstantiateObjectFromSaveData(savedBin.transform));
            binSvg.AreaTotal -= objectPlaced[^1].AreaTotal;
        }
        if (objectPlaced.Count != 0)
            binSvg.placer.PlacedObjects.AddRange(objectPlaced);
        return binSvg;
    }
}


[Serializable]
[RequireComponent(typeof(BinPlacer))]
public class BinSvg : ScannedObjectSvg
{
    public Vector2 bottomLeft;
    public float areaLeft;
    public BinPlacer placer;
    public List<Vector2> holeOutline;
    public Transform my3DClone;

    public override void Awake()
    {
        if (testing)
            GenerateObject(svg);
    }

    public void GenerateObject(string svg)
    {
        base.GenerateObject(svg, 0);
        if (OutlineVertices == null)
            return;
        
        bottomLeft = new Vector2(OutlineVertices.Min(v => v.x),
                OutlineVertices.Min(v => v.y));
        areaLeft = AreaTotal;
        placer = this.transform.GetComponent<BinPlacer>();
        placer.bin = this;
        CreateLine(OutlineVertices);
    }

    public void ClearPlacedObjects()
    {
        Debug.Log("CLearing");
        foreach (ScannedObjectSvg scannedObjectSvg in placer.PlacedObjects)
        {
            // Destroy(scannedObjectSvg.MyClone);
            Destroy(scannedObjectSvg.gameObject);
        }
        placer.PlacedObjects.Clear();
    }

    
    public void CreateLine(List<Vector2> points)
    {

        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        // Set up the line renderer properties
        // You can modify these properties as per your requirement
        lineRenderer.startWidth = 0.001f;
        lineRenderer.endWidth = 0.001f;
        lineRenderer.positionCount = points.Count;
        lineRenderer.useWorldSpace = false;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.loop = true;

        // Add the points to the line renderer
        lineRenderer.SetPositions(ConvertToVector3Array(points, -0.1f));
    }

    public Vector3[] ConvertToVector3Array(List<Vector2> vector2List, float zValue = 0f)
    {
        Vector3[] vector3Array = new Vector3[vector2List.Count];

        for (int i = 0; i < vector2List.Count; i++)
        {
            Vector2 vec = vector2List[i];
            // Convert each Vector2 to a Vector3, adding a z-value
            vector3Array[i] = new Vector3(vec.x, vec.y, zValue);
        }

        return vector3Array;
    }

    public void SetUp3dClone(Transform clone)
    {
        this.my3DClone = clone;
        clone.gameObject.layer = 6;

        for (int i = 0; i < clone.childCount; i++)
        {
            clone.GetChild(i).gameObject.layer = 6;
        }

        BinSvg tmp = clone.gameObject.GetComponent<BinSvg>();
        if (tmp != null)
        {
            Destroy(tmp);
        }
        else
        {
            Debug.Log("Cant find Bin Svg in clone");
        }

        BinPlacer tmpPlacer = clone.gameObject.GetComponent<BinPlacer>();
        if (tmpPlacer != null)
        {
            Destroy(tmpPlacer);
        }
        else
        {
            Debug.Log("Cant find Bin Svg in clone");
        }
        Destroy(clone.GetComponent<LineRenderer>());
    }

    public bool CheckIfCanBePlaced(ScannedObjectSvg newObject)
    {
        return newObject.AreaTotal <= areaLeft;
    }

    public BinDataSave GetBinData()
    {
        BinDataSave dataSave = new BinDataSave(this);
        return dataSave;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        VisualizeVertices(holeOutline, Color.yellow, Color.blue, 0.001f);
    }

}
