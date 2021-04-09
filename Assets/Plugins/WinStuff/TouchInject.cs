using System;
using System.Drawing;
using TCD.System.TouchInjection;

namespace WinStuff {
	public static class TouchInject {
		#region Fields

		public static int contactsPerFrame = 10;
		public static int touchHoldTime = 1000;
		public static int touchTimeResolution = 10;


		private static PointerTouchInfo[] activeContacts;

		private static bool isDragging = false;

		#endregion Fields

		#region Methods

		public static PointerTouchInfo CreateDefaultPointerTouchInfo(int x, int y, int radius, uint id) {
			PointerTouchInfo contact = new PointerTouchInfo();
			contact.PointerInfo.pointerType = PointerInputType.TOUCH;
			contact.TouchFlags = TouchFlags.NONE;
			contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
			contact.PointerInfo.PtPixelLocation.X = x;
			contact.PointerInfo.PtPixelLocation.Y = y;
			contact.PointerInfo.PointerId = id;
			contact.ContactArea.left = x - radius;
			contact.ContactArea.right = x + radius;
			contact.ContactArea.top = y - radius;
			contact.ContactArea.bottom = y + radius;
			contact.Orientation = 130; //See the angle changed from default value 90 to 130
			return contact;
		}

		private static Point previousPoint;

		public static void BeginDragging(Point point) {
			if (isDragging) return;
			
			

			activeContacts = new PointerTouchInfo[1];

			activeContacts[0] = CreateDefaultPointerTouchInfo(point.X, point.Y, 2, 1);

			// Touch down
			activeContacts[0].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
			

			// Initial contact
			TouchInjector.InjectTouchInput(activeContacts.Length, activeContacts);

			previousPoint = point;

			isDragging = true;
		}

		public static void AddToActiveDrag(Point point) {
			if (!isDragging) return;

			//Point moveAmt = new Point(previousPoint.X - point.X, previousPoint.Y - point.Y);
			
			

			//Update flag so we can move it
			activeContacts[0].PointerInfo.PointerFlags =
				PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
			
			activeContacts[0].Move(point.X - previousPoint.X, point.Y - previousPoint.Y);

			TouchInjector.InjectTouchInput(activeContacts.Length, activeContacts);

			previousPoint = point;
		}

		public static void EndActiveDrag() {
			if (!isDragging) return;

			// Release contact
			activeContacts[0].PointerInfo.PointerFlags = PointerFlags.UP;


			TouchInjector.InjectTouchInput(activeContacts.Length, activeContacts);

			isDragging = false;


		}


		public static void SimulateHold(Point point) {
			PointerTouchInfo[] contacts = new PointerTouchInfo[1];
			contacts[0] = CreateDefaultPointerTouchInfo(point.X, point.Y, 2, 1);

			// Touch down
			contacts[0].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;

			// Initial contact
			bool success = TouchInjector.InjectTouchInput(contacts.Length, contacts);

			// Touch update
			contacts[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;

			int steps = touchHoldTime / touchTimeResolution;

			// Move contact
			for (int i = 0; i < steps; i++) {
				success = TouchInjector.InjectTouchInput(contacts.Length, contacts);
				//new ManualResetEvent(false).WaitOne(touchTimeResolution);
			}

			// Release contact
			contacts[0].PointerInfo.PointerFlags = PointerFlags.UP;

			success = TouchInjector.InjectTouchInput(contacts.Length, contacts);
		}

		public static void SimulatePinchAndZoom(Point first, Point second, bool zoomOut, int step = 1) {
			PointerTouchInfo[] contacts = new PointerTouchInfo[2];
			contacts[0] = CreateDefaultPointerTouchInfo(first.X, first.Y, 2, 1);
			contacts[1] = CreateDefaultPointerTouchInfo(second.X, second.Y, 2, 2);

			// Touch down
			contacts[0].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
			contacts[1].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;

			// Initial contact
			bool success = TouchInjector.InjectTouchInput(contacts.Length, contacts);

			// Touch update
			contacts[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
			contacts[1].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;

			int steps = (int) Math.Sqrt(Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2)) / step;

			double deltaXRaw = Math.Abs((double) (first.X - second.X) / steps / 2);

			double deltaYRaw = Math.Abs((double) (first.Y - second.Y) / steps / 2);

			double deltaX = 0;
			double deltaY = 0;

			int direction;

			// if zoomOut move touch points closer
			if (zoomOut) direction = +1;
			else direction = -1;

			// Move contact
			for (int i = 0; i < steps; ++i) {
				deltaX += deltaXRaw;
				deltaY += deltaYRaw;
				contacts[0].Move(direction * (int) -deltaX, direction * (int) -deltaY);
				contacts[1].Move(direction * (int) deltaX, direction * (int) deltaY);
				success = TouchInjector.InjectTouchInput(contacts.Length, contacts);
				//new ManualResetEvent(false).WaitOne(touchTimeResolution);
				deltaX = deltaX - (int) deltaX;
				deltaY = deltaY - (int) deltaY;
			}

			// Release contacts
			contacts[0].PointerInfo.PointerFlags = PointerFlags.UP;
			contacts[1].PointerInfo.PointerFlags = PointerFlags.UP;

			success = TouchInjector.InjectTouchInput(contacts.Length, contacts);
		}

		public static void SimulateSwipe(Point start, Point end, int duration) {
			PointerTouchInfo[] contacts = new PointerTouchInfo[1];
			contacts[0] = CreateDefaultPointerTouchInfo(start.X, start.Y, 2, 0);

			// Touch down
			contacts[0].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;

			// Initial contact
			bool success = TouchInjector.InjectTouchInput(contacts.Length, contacts);

			// Touch update
			contacts[0].PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;

			//CubicBezier cb = new CubicBezier(0.95, 0.05, 0.795, 0.035); // easeInExpo

			double frames = (duration / (touchTimeResolution + 0.3));

			double delta = (double) 1 / frames / contactsPerFrame;

			int distanceX = end.X - start.X;
			int distanceY = end.Y - start.Y;

			int contactRadius = contacts[0].PointerInfo.PtPixelLocation.X - contacts[0].ContactArea.left;

			double topFrame = Math.Ceiling(frames);

			int contactsToSend = (int) (frames * contactsPerFrame);

			// Move contact
			for (int i = 0; i < contactsToSend; ++i) {
				double travelPercent = 0; //cb.GetSplineValue(delta * i);
				int deltaX = start.X + (int) (distanceX * travelPercent) - contacts[0].PointerInfo.PtPixelLocation.X;
				int deltaY = start.Y + (int) (distanceY * travelPercent) - contacts[0].PointerInfo.PtPixelLocation.Y;
				contacts[0].Move(deltaX, deltaY);
				success = TouchInjector.InjectTouchInput(contacts.Length, contacts);
				//if ( i % contactsPerFrame == 0 )
				//new ManualResetEvent(false).WaitOne(touchTimeResolution);
			}

			if (contacts[0].PointerInfo.PtPixelLocation.X < end.X || contacts[0].PointerInfo.PtPixelLocation.Y < end.Y) {
				int deltaX = end.X - contacts[0].PointerInfo.PtPixelLocation.X;
				int deltaY = end.Y - contacts[0].PointerInfo.PtPixelLocation.Y;
				contacts[0].Move(deltaX, deltaY);
				success = TouchInjector.InjectTouchInput(contacts.Length, contacts);
			}

			// Release contact
			contacts[0].PointerInfo.PointerFlags = PointerFlags.UP;

			success = TouchInjector.InjectTouchInput(contacts.Length, contacts);
		}

		public static void SimulateTap(Point point) {
			PointerTouchInfo[] contacts = new PointerTouchInfo[1];
			contacts[0] = CreateDefaultPointerTouchInfo(point.X, point.Y, 2, 1);

			// Touch down
			contacts[0].PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;

			// Initial contact
			bool success = TouchInjector.InjectTouchInput(contacts.Length, contacts);

			// Release contact
			contacts[0].PointerInfo.PointerFlags = PointerFlags.UP;

			success = TouchInjector.InjectTouchInput(contacts.Length, contacts);
		}

		#endregion Methods
	}
}