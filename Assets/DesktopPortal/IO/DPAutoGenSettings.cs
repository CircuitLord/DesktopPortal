using System;
using System.Reflection;
using UnityEngine;

namespace DesktopPortal.IO {
	public class DPAutoGenSettings : MonoBehaviour {
		
		
		
		
		
		
		
		

		private void Start() {








		}


		public void Generate() {
			
			
			FieldInfo[] objectFields = typeof(DPConfigJson).GetFields(BindingFlags.Instance | BindingFlags.Public);

			int i = 0;
			foreach (FieldInfo info in objectFields) {

				Attribute[] attributes = Attribute.GetCustomAttributes(objectFields[i]);



				foreach (Attribute attribute in attributes) {

					switch (attribute.GetType().ToString()) {
						
						case "CConfigFloat":

							break;
						
						
					}
					
					
				}
				
				
				
				i++;

			}
			
			
			/*PropertyInfo[] props = typeof(DPConfigJson).GetProperties();

			foreach (PropertyInfo prop in props) {
				
			}*/

			
		}
		
		
		
		
		
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class CConfigID : Attribute {

		public string id;
		
		public CConfigID(string id) {
			this.id = id;
		}
	}
	
	
	
	[AttributeUsage(AttributeTargets.Field)]
	public class CConfigFloat : Attribute {
		public float min;
		public float max;

		public CConfigFloat(float min, float max) {
			this.min = min;
			this.max = max;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class CConfigFloat_Slider : Attribute { }

	[AttributeUsage(AttributeTargets.Field)]
	public class CConfigFloat_Incrementer : Attribute {

		public float increment;
		
		public CConfigFloat_Incrementer(float increment) {
			this.increment = increment;
		}
	}
	

	
	
}