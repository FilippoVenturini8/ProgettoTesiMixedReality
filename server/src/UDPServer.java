import io.vertx.core.AbstractVerticle;
import io.vertx.core.Handler;
import io.vertx.core.Vertx;
import io.vertx.core.buffer.Buffer;
import io.vertx.core.datagram.DatagramSocket;
import io.vertx.core.datagram.DatagramSocketOptions;
import io.vertx.core.net.NetServer;
import io.vertx.core.net.NetSocket;

public class UDPServer extends AbstractVerticle{

	private static final int SERVER_PORT = 10000;
	private static final String SERVER_HOST = "127.0.0.1";
	
	private DatagramSocket udpSocket;
	
	@Override
    public void start() throws Exception {
		Vertx vertx = Vertx.vertx();
        
		udpSocket = vertx.createDatagramSocket(new DatagramSocketOptions().setIpV6(false).setReuseAddress(true));
		
		udpSocket.listen(SERVER_PORT, SERVER_HOST, asyncResult -> {
            if (asyncResult.succeeded()) {
            	System.out.println("[UDP] Server is listening on "+SERVER_HOST+":"+SERVER_PORT);

                udpSocket.handler(packet -> {
                    String commandData = packet.data().getString(0, packet.data().length());
                    System.out.println("[UDP] Command received from "+packet.sender().host()+":"+packet.sender().port()+", length: "+packet.data().length()+", data: "+commandData);

                });
            } else {
            	System.out.println("[UDP] Server listen failed on "+SERVER_HOST+":"+SERVER_PORT+" - "+asyncResult.cause().toString());
            }
        });
	}
	
	public void send() {
		udpSocket.send("PROVA DI SEND", 8051, SERVER_HOST);
	}
	
}
