using System;
namespace Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene
{
	public class IsfSceneParameterOfSingle
	{
		public IsfSceneParameterOfSingle(
			OpenGlSceneParameter sceneParameter,
			Single min,
			Single max)
		{
			OpenGlSceneParameter = sceneParameter;
			Min = min;
			Max = max;
		}

		private Single Min { get; }

		private Single Max { get; }

		public Single CalculateValue()
		{
			var distance = Max - Min;
			var coefficient = (Single)OpenGlSceneParameter.Value / Byte.MaxValue;
			var valueOffset = coefficient * distance;
			var result = Min + valueOffset;
			return result;
		}

		OpenGlSceneParameter OpenGlSceneParameter { get; }
	}
}

