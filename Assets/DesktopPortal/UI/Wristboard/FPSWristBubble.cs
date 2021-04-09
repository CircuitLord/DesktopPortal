using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DesktopPortal.UI;
using DesktopPortal.Wristboard;
using DPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;


namespace DesktopPortal.UI {
	public class FPSWristBubble : WristBubble {


		private uint sizeOfCompFrameTime;

		private List<Image> bars = new List<Image>();

		[SerializeField] private RectTransform barGridTrans;

		//[SerializeField] private RectTransform maxGoodLine;
		

		[SerializeField] private TextMeshProUGUI fpsText;
		
		
		[SerializeField] private Color frameGoodColor;
		[SerializeField] private Color frameBadColor;
		
		
		
		protected override void Start() {
			sizeOfCompFrameTime = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Compositor_FrameTiming));

			
			Image[] images = barGridTrans.GetComponentsInChildren<Image>();

			foreach (Image image in images) {
				bars.Add(image);
			}

			StartCoroutine(UpdateBarGraph());
		}

		public override void OnHover(bool hovering) {
			
		}

		public override void OnShortClick() {
			
		}
		
		public override void UpdateVisuals() {
			
		}

		private IEnumerator UpdateBarGraph() {

			//while (!SteamVRManager.isConnected) yield return null;
			
			
			int framesToUpdate = 20;
				
			/*//Yield for
			for (int i = 0; i < framesToUpdate; i++){
				yield return null;
			}*/
			

			while (true) {
				
				while (!SteamVRManager.isConnected) yield return null;

				while (!SteamVRManager.isWearingHeadset) yield return null;

				while (!wristboardDP.overlay.shouldRender) yield return null;

				int targetFPS = SteamVRManager.hmdRefreshRate;
				
				
				//If frametimes go beyond this, they're bad frames
				float goodFrameTime = (1f / targetFPS) * 1000f;


				
				var timing = new Compositor_FrameTiming[framesToUpdate];
				timing[0].m_nSize = sizeOfCompFrameTime;
				OpenVR.Compositor.GetFrameTimings(timing);

				float avgFPS = 0f;
				
				//Loop through backwards so frames are always displayed framesToUpdate frames behind when they actually were
				for (int i = timing.Length - 1; i >= 0 ; i -= 2) {


					
					
					Compositor_FrameTiming otherFrame = timing[i];
					if (i > 1) otherFrame = timing[i - 1];

					Compositor_FrameTiming frame = timing[i];

					//if (frame.m_flTotalRenderGpuMs > otherFrame.m_flTotalRenderGpuMs) frame = otherFrame;

					float value = 0f;
					
					List<float> frameTimes = new List<float>() {
						frame.m_flTotalRenderGpuMs,
						otherFrame.m_flTotalRenderGpuMs,
						frame.m_flCompositorRenderCpuMs,
						otherFrame.m_flCompositorRenderCpuMs
					};


					value = frameTimes.Max();
					

					Image last = bars.Last();


					float frameImageHeight = Maths.LinearUnclamped(value, 0f, goodFrameTime, 0f, 45f);
					
					last.rectTransform.sizeDelta = new Vector2(3f, frameImageHeight);

					if (value > goodFrameTime) {
						last.color = frameBadColor;
						
						int fps = Mathf.RoundToInt(1f / (value / 1000f));
						fpsText.SetText(fps.ToString());

						avgFPS += fps;
						avgFPS += fps;
					}
					else {
						last.color = frameGoodColor;

						//fpsText.SetText(targetFPS.ToString());

						avgFPS += targetFPS;
						avgFPS += targetFPS;
					}


					//Move it to the start
					bars.Remove(last);
					bars.Insert(0, last);
					
					last.rectTransform.SetSiblingIndex(bars.Count - 1);

					//Return twice since we're displaying two frames in one
					yield return null;
					yield return null;
				}
				
				//Update the text:
				string avgFPSText = Mathf.RoundToInt((avgFPS / framesToUpdate)).ToString();
				fpsText.SetText(avgFPSText);



				if (timing.Length <= 0) yield return null;


			}
			
		}
	

		
	}
}