using System.Collections;
using TCD.System.TouchInjection;
using UnityEngine;

public static class WinTouch {
	public static void Init() {
		TouchInjector.InitializeTouchInjection();
	}


	public static PointerTouchInfo[] single = new PointerTouchInfo[1];
	private static int prevDragX = 0;
	private static int prevDragY = 0;


	public static IEnumerator TryPress(int x, int y) {
		var pointer = new PointerTouchInfo();

		//We can add different additional touch data
		pointer.TouchMasks = TouchMask.PRESSURE;
		pointer.Pressure = 100;


		//Pointer ID is for gesture tracking
		pointer.PointerInfo.PointerId = 1;
		pointer.PointerInfo.pointerType = PointerInputType.TOUCH;

		pointer.PointerInfo.PtPixelLocation.X = x;
		pointer.PointerInfo.PtPixelLocation.Y = y;

		pointer.PointerInfo.PointerFlags = PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.DOWN;

		TouchInjector.InjectTouchInput(1, new[] {pointer});

		//Hold touch for some time
		yield return new WaitForSeconds(0.1f);

		pointer.PointerInfo.PointerFlags = PointerFlags.UPDATE;

		TouchInjector.InjectTouchInput(1, new[] {pointer});
	}


	public static void SimulateSingleTouchDown(int x, int y, int radius = 1) {
		prevDragX = x;
		prevDragY = y;

		single[0] = MakePointerTouchInfo(x, y, radius, 1);
		bool success = TouchInjector.InjectTouchInput(1, single);
	}

	public static void SimulateSingleTouchDrag(int x, int y, int radius = 1) {
		//PointerTouchInfo[] contacts = new PointerTouchInfo[1];
		//contacts[0] = MakePointerTouchInfo(x, y, radius, 1);
		single[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
		single[0].Move(x - prevDragX, y - prevDragY);

		prevDragX = x;
		prevDragY = y;

		bool success = TouchInjector.InjectTouchInput(1, single);
	}

	public static void SimulateSingleTouchUp(int x, int y) {
		//PointerTouchInfo[] contacts = new PointerTouchInfo[1];
		//contacts[0] = MakePointerTouchInfo(0, 0, 2, 1);
		single[0].PointerInfo.PointerFlags = PointerFlags.UP;

		bool success = TouchInjector.InjectTouchInput(1, single);
	}


	public static PointerTouchInfo MakePointerTouchInfo(int x, int y, int radius, uint id, uint orientation = 90,
	                                                    uint pressure = 32000) {
		PointerTouchInfo contact = new PointerTouchInfo();
		contact.PointerInfo.pointerType = PointerInputType.TOUCH;
		contact.TouchFlags = TouchFlags.NONE;
		contact.Orientation = orientation;
		contact.Pressure = pressure;
		contact.PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
		contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
		contact.PointerInfo.PtPixelLocation.X = x;
		contact.PointerInfo.PtPixelLocation.Y = y;
		contact.PointerInfo.PointerId = id;
		contact.ContactArea.left = x - radius;
		contact.ContactArea.right = x + radius;
		contact.ContactArea.top = y - radius;
		contact.ContactArea.bottom = y + radius;
		return contact;
	}
}