public class RunService {

	public static void main(String[] args) throws Exception{		
		/*TCPServerBase tcpServerBase = new TCPServerBase();
		tcpServerBase.start();*/
		
		TCPServerOptimized tcpServerOptimized = new TCPServerOptimized();
		tcpServerOptimized.start();
		
		/*UDPServer udpServer = new UDPServer();
		udpServer.start();*/
		while(true) {
			//udpServer.send();
			//tcpServerBase.send();
			tcpServerOptimized.send();
			Thread.sleep(100);
		}
	}
}