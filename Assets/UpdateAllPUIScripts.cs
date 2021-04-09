/*using Sirenix.OdinInspector;
using ThisOtherThing.UI.Shapes;
using ThisOtherThing.UI.ShapeUtils;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

namespace DefaultNamespace {

	public class UpdateAllPUIScripts : MonoBehaviour {


		[Button]
		public void ConvertAll() {

			var all = FindObjectsOfType<ProceduralImage>(true);

			foreach (var image in all) {

				GameObject go = image.gameObject;
				//if (go.name != "BG") continue;

				var uni = go.GetComponent<UniformModifier>();

				if (uni != null) {

					float radius = uni.Radius;
					Color color = image.color;

					bool active = image.gameObject.activeSelf;
					bool raycast = image.raycastTarget;
					
					DestroyImmediate(uni);
					
					DestroyImmediate(image);


					var rect = go.AddComponent<Rectangle>();

					rect.RoundedProperties.Type = RoundedRects.RoundedProperties.RoundedType.Uniform;
					rect.RoundedProperties.UniformRadius = radius / 2f;
					rect.RoundedProperties.UniformResolution.ResolutionMaxDistance = 2;

					rect.ShapeProperties.FillColor = color;
					rect.raycastTarget = raycast;

					rect.AntiAliasingProperties.AntiAliasing = 0.2f;

				}


			}


		}
		

	}

}*/