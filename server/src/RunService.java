public class RunService {

	public static void main(String[] args) throws Exception{		
		TCPServerBase tcpServer = new TCPServerBase();
		tcpServer.start();
		
		/*UDPServer udpServer = new UDPServer();
		udpServer.start();*/
		while(true) {
			//udpServer.send();
			tcpServer.send();
			Thread.sleep(100);
		}
	}
}