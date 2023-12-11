using Unity.VectorGraphics;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

public class BaseSvgScript : MonoBehaviour
{
	[Multiline()]
	public string svg = "...";
	public int pixelsPerUnit;
	public bool flipYAxis;

	protected List<VectorUtils.Geometry> GetGeometries()
	{
		using var textReader = new StringReader(svg);
		var sceneInfo = SVGParser.ImportSVG(textReader);

		return VectorUtils.TessellateScene(sceneInfo.Scene, new VectorUtils.TessellationOptions
		{
				StepDistance = 10,
				SamplingStepSize = 200,
				MaxCordDeviation = 0.5f,
				MaxTanAngleDeviation = 0.1f
		});
	}
	
	protected List<VectorUtils.Geometry> GetGeometriesFromFile(string svgText)
	{
		using var textReader = new StringReader(svgText);
		var sceneInfo = SVGParser.ImportSVG(textReader);

		return VectorUtils.TessellateScene(sceneInfo.Scene, new VectorUtils.TessellationOptions
		{
				StepDistance = 10,
				SamplingStepSize = 200,
				MaxCordDeviation = 0.5f,
				MaxTanAngleDeviation = 0.1f
		});
	}
}
