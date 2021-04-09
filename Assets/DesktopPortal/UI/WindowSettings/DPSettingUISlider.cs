using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPortal.UI {
	public class DPSettingUISlider : DPSettingUIBase {

		public Slider slider;

		public float min => slider.minValue;
		public float max => slider.maxValue;


	}
}