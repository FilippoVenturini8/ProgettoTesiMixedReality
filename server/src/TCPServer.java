import java.sql.Timestamp;
import java.util.Date;
import java.util.LinkedList;
import java.util.List;

import io.vertx.core.AbstractVerticle;
import io.vertx.core.Handler;
import io.vertx.core.Vertx;
import io.vertx.core.buffer.Buffer;
import io.vertx.core.json.JsonArray;
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
	
	private long sendTimestamp;
	private long receiveTimestamp;
	
	private boolean updateSent = false;
	
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
                        //System.out.println("Incoming data: " + inBuffer.length());

                        String data = inBuffer.getString(0, inBuffer.length());
                        
                        System.out.println("Data: " + data);
                        
                        if(updateSent) {
                        	Date date = new Date();
                        	receiveTimestamp = date.getTime();
                        	updateSent = false;
                        	
                        	long delay = (receiveTimestamp-sendTimestamp)/2;
                        	
                        	System.out.println("Send timestamp: " + sendTimestamp + " ms");
                        	System.out.println("Receive timestamp: " + receiveTimestamp + " ms");
                        	
                        	System.out.println("Delay: "+ delay + " ms");
                        }
                        
                    }
                });
                serverNetSocket = netSocket;
                
                JsonArray packetJson = new JsonArray();
                
                for(Cube cube : cubes) {
        			JsonObject cubeJson = new JsonObject();
        			cubeJson.put("cubeID", cube.getId());
        			cubeJson.put("position", cube.getPosition().toString());
        			cubeJson.put("rotation", cube.getRotation().toString());
        			cubeJson.put("scale", cube.getScale().toString());
        			packetJson.add(cubeJson);
        		}
                
                Buffer outBuffer = Buffer.buffer();
                outBuffer.appendString("{\"Items\":" + packetJson.toString() + "}");
                serverNetSocket.write(outBuffer);

                System.out.println("[TCP] Sending setup packet to clients");
                
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
		System.out.println("[TCP] Sending update packet to clients");
		
		JsonArray packetJson = new JsonArray();
		
		for(Cube cube : cubes) {
			JsonObject cubeJson = new JsonObject();
			cubeJson.put("cubeID", cube.getId());
			cubeJson.put("position", cube.getPosition().toString());
			cubeJson.put("rotation", cube.getRotation().toString());
			cubeJson.put("scale", cube.getScale().toString());
			packetJson.add(cubeJson);
		}
        
        Buffer outBuffer = Buffer.buffer();
        outBuffer.appendString("{\"Items\":" + packetJson.toString() + "}");
        serverNetSocket.write(outBuffer);
        
        Date date = new Date();
        sendTimestamp = date.getTime();
        updateSent = true;
	}
	
	private void initCubes() {
		for(int i= 0; i < NUMBER_OF_CUBES; i++) {
			Position position = new Position(0,0,-(float)i);
			Rotation rotation = new Rotation(0,0,0);
			Scale scale = new Scale(SCALE, SCALE, SCALE);
			cubes.add(new Cube(i, position, rotation, scale));
		}
	}
	
	private void rotateCubes() {
		for(int i=0; i<cubes.size(); i++) {
			Rotation oldRotation = cubes.get(i).getRotation();
			Rotation newRotation;
			if(i%2 == 0) {
				newRotation = new Rotation (oldRotation.getX()+DELTA_ROTATION,oldRotation.getY(),oldRotation.getZ());
			}else {
				newRotation = new Rotation (oldRotation.getX(),oldRotation.getY()+DELTA_ROTATION,oldRotation.getZ());
			}
			cubes.get(i).setRotation(newRotation);
		}
	}
}
