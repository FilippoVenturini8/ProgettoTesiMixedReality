import java.util.LinkedList;
import java.util.List;

import io.vertx.core.AbstractVerticle;
import io.vertx.core.Handler;
import io.vertx.core.Vertx;
import io.vertx.core.buffer.Buffer;
import io.vertx.core.json.JsonObject;
import io.vertx.core.net.NetServer;
import io.vertx.core.net.NetSocket;
import model.Cube;
import model.Position;
import model.Rotation;
import model.Scale;

public class TCPServer extends AbstractVerticle {
	private NetSocket serverNetSocket;
	private List<Cube> cubes = new LinkedList<>();
	private boolean isConnected = false;
	
	private static float DELTA_ROTATION = 0.5f;
	private static int NUMBER_OF_CUBES = 5;
	private static float SCALE = 0.2f;
	
	@Override
    public void start() throws Exception {
		this.initCubes();
		
		Vertx vertx = Vertx.vertx();
        NetServer server = vertx.createNetServer();

        server.connectHandler(new Handler<NetSocket>() {

            @Override
            public void handle(NetSocket netSocket) {
                System.out.println("Incoming connection!");

                netSocket.handler(new Handler<Buffer>() {

                    @Override
                    public void handle(Buffer inBuffer) {
                        System.out.println("Incoming data: " + inBuffer.length());

                        String data = inBuffer.getString(0, inBuffer.length());

                        /*Buffer outBuffer = Buffer.buffer();
                        outBuffer.appendString("Response...");

                        netSocket.write(outBuffer);*/
                    }
                });
                serverNetSocket = netSocket;
                
                JsonObject setupJo = new JsonObject();
                setupJo.put("scale", SCALE);
                setupJo.put("numberOfCube", NUMBER_OF_CUBES);
                
                Buffer outBuffer = Buffer.buffer();
                outBuffer.appendString(setupJo.toString());

                netSocket.write(outBuffer);
                
                try {
					Thread.sleep(100);
					isConnected = true;
				} catch (InterruptedException e) {
					e.printStackTrace();
				}
            }
        });
        server.listen(10000);
        System.out.println("Server TCP listening on port 10000...");
    }
	
	public void send() {
		if(!isConnected) {
			return;
		}
		
		this.rotateCubes();
		System.out.println("[TCP] Sending rotation: "+ DELTA_ROTATION +" to 192.168.40.102");
		
        JsonObject reqJo = new JsonObject();
        reqJo.put("rotation", DELTA_ROTATION);
        
        Buffer outBuffer = Buffer.buffer();
        outBuffer.appendString(reqJo.toString());
        
        serverNetSocket.write(outBuffer);
	}
	
	private void initCubes() {
		for(int i= 0; i < NUMBER_OF_CUBES; i++) {
			Position position = new Position(0,0,-(float)i);
			Rotation rotation = new Rotation(0,0,0);
			Scale scale = new Scale(SCALE, SCALE, SCALE);
			cubes.add(new Cube(position, rotation, scale));
		}
	}
	
	private void rotateCubes() {
		for(Cube cube : cubes){
			Rotation oldRotation = cube.getRotation();
			Rotation newRotation = new Rotation (oldRotation.getX()+DELTA_ROTATION,oldRotation.getY(),oldRotation.getZ());
			cube.setRotation(newRotation);
		}
	}
}
