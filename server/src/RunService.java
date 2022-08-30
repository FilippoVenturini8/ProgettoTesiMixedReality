public class RunService {

	public static void main(String[] args) throws Exception{		
		TCPServerBase tcpServerBase = new TCPServerBase();
		tcpServerBase.start();
		
		/*TCPServerOptimized1 tcpServerOptimized1 = new TCPServerOptimized1();
		tcpServerBase.start();*/
		
		/*UDPServer udpServer = new UDPServer();
		udpServer.start();*/
		while(true) {
			//udpServer.send();
			tcpServerBase.send();
			//tcpServerOptimized1.send();
			Thread.sleep(100);
		}
	}
}