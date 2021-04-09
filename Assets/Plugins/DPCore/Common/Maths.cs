using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class Maths
{
	public static float Linear(float value, float min, float max, float newMin, float newMax) {
		float interpolatedValue = ((value - min) / (max - min)) * (newMax - newMin) + newMin;
        
		if (interpolatedValue < newMin) return newMin;
		else if (interpolatedValue > newMax) return newMax;
        
		return interpolatedValue;


	}
    
	public static float LinearUnclamped(float value, float min, float max, float newMin, float newMax) {
		float interpolatedValue = ((value - min) / (max - min)) * (newMax - newMin) + newMin;
        
		//if (interpolatedValue < newMin) return newMin;
		//else if (interpolatedValue > newMax) return newMax;
        
		return interpolatedValue;


	}


	public static float GetCircleValue(float degrees) {

		//float test = 1f - Math.Abs(degrees / 90f + 1 % 4) - 2;
        
		if (degrees > 90f) degrees -= 180f;
		else if (degrees < -90f) degrees += 180f;
        
		if (degrees > 180f) degrees -= 270f;
		else if (degrees < -180f) degrees += 270f;
        
        
		// if (degrees >= -90f && degrees <= 90) {
		return degrees / 90f;
		//}

        
	}


}
