public class RunService {

	public static void main(String[] args) throws Exception{		
		/*TCPServerBase tcpServerBase = new TCPServerBase();
		tcpServerBase.start();*/
		
		/*TCPServerOptimized tcpServerOptimized = new TCPServerOptimized();
		tcpServerOptimized.start();*/
		
		MultiClientTCPServer multiClientTcpServer = new MultiClientTCPServer();
		multiClientTcpServer.start();
		
		/*UDPServer udpServer = new UDPServer();
		udpServer.start();*/
		while(true) {
			//udpServer.send();
			//tcpServerBase.send();
			//tcpServerOptimized.send();
			multiClientTcpServer.send();
			Thread.sleep(100);
		}
	}
}