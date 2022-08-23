/*
 * Data Service as a vertx event-loop 
 */
public class RunService {

	public static void main(String[] args) throws Exception{		
		TCPServer tcpServer = new TCPServer();
		tcpServer.start();
		
		/*UDPServer udpServer = new UDPServer();
		udpServer.start();*/
		while(true) {
			//udpServer.send();
			tcpServer.send();
			Thread.sleep(50);
		}
	}
}