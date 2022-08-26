import io.vertx.core.AbstractVerticle;
import io.vertx.core.Future;
import io.vertx.core.Handler;
import io.vertx.core.Vertx;
import io.vertx.core.buffer.Buffer;
import io.vertx.core.datagram.DatagramSocket;
import io.vertx.core.datagram.DatagramSocketOptions;
import io.vertx.core.net.NetServer;
import io.vertx.core.net.NetSocket;
import model.Cube;
import model.Position;
import model.Rotation;
import model.Scale;

public class UDPServer extends AbstractVerticle{

	private static final int SERVER_PORT = 10000;
	private static final String SERVER_HOST = "192.168.40.100";
	
	private DatagramSocket udpSocket;
	
	private Cube cube;
	private boolean isConnected = false;
	private Future<Void> succeded;
	
	@Override
    public void start() throws Exception {
		this.initCube();
		
		Vertx vertx = Vertx.vertx();
        
		udpSocket = vertx.createDatagramSocket(new DatagramSocketOptions().setIpV6(false).setReuseAddress(true));
		
		udpSocket.listen(SERVER_PORT, SERVER_HOST, asyncResult -> {
            if (asyncResult.succeeded()) {
            	System.out.println("[UDP] Server is listening on "+SERVER_HOST+":"+SERVER_PORT);

                udpSocket.handler(packet -> {
                    String commandData = packet.data().getString(0, packet.data().length());
                    System.out.println("[UDP] Command received from "+packet.sender().host()+":"+packet.sender().port()+", length: "+packet.data().length()+", data: "+commandData);
                    try {
						Thread.sleep(100);
					} catch (InterruptedException e) {
						e.printStackTrace();
					}
                    succeded = udpSocket.send("RISPOSTA DAL SERVER", 1336, "192.168.40.102");
                    isConnected = true;
                });
            } else {
            	System.out.println("[UDP] Server listen failed on "+SERVER_HOST+":"+SERVER_PORT+" - "+asyncResult.cause().toString());
            }
        });
	}
	
	public void send() {
		if(!isConnected) {
			return;
		}
		this.rotateCube();
		//System.out.println(succeded.succeeded());
		//System.out.println("[UDP] Sending rotation: "+ Float.toString(this.cube.getRotation().getX()) +" to 192.168.40.102");
		//udpSocket.send(Float.toString(this.cube.getRotation().getX()), 1336, "192.168.40.102");
	}
	
	private void initCube() {
		Position position = new Position(0,0,0);
		Rotation rotation = new Rotation(0,0,0);
		Scale scale = new Scale(0.2f, 0.2f, 0.2f);
		
		this.cube = new Cube(0, position, rotation, scale);
	}
	
	private void rotateCube() {
		Rotation oldRotation = this.cube.getRotation();
		Rotation newRotation = new Rotation (oldRotation.getX()+0.1f,oldRotation.getY(),oldRotation.getZ());
		this.cube.setRotation(newRotation);
	}
	
}
