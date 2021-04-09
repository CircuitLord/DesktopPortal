using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SocketsTestAndroid : MonoBehaviour {
	// Start is called before the first frame update
	void Start() {
		//StartCoroutine(ExecuteClient());
	}

	/*private static Socket sender;

	static IEnumerator ExecuteClient() {
		//try {
			// Establish the remote endpoint  
			// for the socket. This example  
			// uses port 9002 on the local  
			// computer. 
			IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 9002);
			//Debug.Log(ipAddr);

			// Creation TCP/IP Socket using  
			// Socket Class Costructor 
			sender = new Socket(ipAddr.AddressFamily,
				SocketType.Stream, ProtocolType.Tcp);

			//try {
				// Connect Socket to the remote  
				// endpoint using method Connect() 
				sender.Connect(localEndPoint);

				// We print EndPoint information  
				// that we are connected 
				Console.WriteLine("Socket connected to -> {0} ",
					sender.RemoteEndPoint.ToString());

				// Creation of messagge that 
				// we will send to Server 
				//byte[] messageSent = Encoding.ASCII.GetBytes("Test Client<EOF>");
				//int byteSent = sender.Send(messageSent);


				while (true) {
					// Data buffer 
					byte[] messageReceived = new byte[1024];

					// We receive the messagge using  
					// the method Receive(). This  
					// method returns number of bytes 
					// received, that we'll use to  
					// convert them to string 

					int byteRecv = sender.Receive(messageReceived);

					if (byteRecv != null) {
						Console.WriteLine("Message from Server -> {0}",
							Encoding.ASCII.GetString(messageReceived,
								0, byteRecv));
					}

					yield return new WaitForSeconds(1f);


				}
				

			//}

		//}

		
		
	}
	*/


	/*private void OnApplicationQuit() {
		// Close Socket using  
		// the method Close() 
		sender.Shutdown(SocketShutdown.Both);
		sender.Close();
	}*/
}