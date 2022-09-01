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

public class TCPServerOptimized extends AbstractVerticle{
	
	private NetSocket serverNetSocket;
	private List<Cube> cubes = new LinkedList<>();
	private boolean[] toUpdate;
	private boolean isConnected = false;
	
	private static float DELTA_ROTATION = 2.0f;
	private static int NUMBER_OF_CUBES = 20;
	private static float SCALE = 0.2f;
	
	private long receiveTimestamp;
	
	private List<Long> allSendTimestamp = new LinkedList<>();
	
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
                        
                        //System.out.println("Data: " + data);
                        
                        Date date = new Date();
                    	receiveTimestamp = date.getTime();
                    	
                    	long sendTimeStamp = allSendTimestamp.get(0);
                    	
                    	long delay = (receiveTimestamp-sendTimeStamp)/2;
                    	
                    	allSendTimestamp.remove(0);
                    	
                    	//System.out.println("Send timestamp: " + sendTimestamp + " ms");
                    	//System.out.println("Receive timestamp: " + receiveTimestamp + " ms");
                    	//System.out.println("Delay: "+ delay + " ms");
                    	
                    	System.out.print(delay+" ");
                        
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
		//System.out.println("[TCP] Sending update packet to clients");
		
		JsonArray packetJson = new JsonArray();
		
		for(int i= 0; i < NUMBER_OF_CUBES; i++) {
			if(!toUpdate[i]) {
				continue;
			}
			JsonObject cubeJson = new JsonObject();
			cubeJson.put("cubeID", cubes.get(i).getId());
			cubeJson.put("position", cubes.get(i).getPosition().toString());
			cubeJson.put("rotation", cubes.get(i).getRotation().toString());
			cubeJson.put("scale", cubes.get(i).getScale().toString());
			packetJson.add(cubeJson);
			toUpdate[i] = false;
		}
        
        Buffer outBuffer = Buffer.buffer();
        outBuffer.appendString("{\"Items\":" + packetJson.toString() + "}");
        serverNetSocket.write(outBuffer);
        
        Date date = new Date();
        allSendTimestamp.add(date.getTime());
	}
	
	private void initCubes() {
		for(int i= 0; i < NUMBER_OF_CUBES; i++) {
			Position position = new Position(0,0,-(float)i);
			Rotation rotation = new Rotation(0,0,0);
			Scale scale = new Scale(SCALE, SCALE, SCALE);
			cubes.add(new Cube(i, position, rotation, scale));
		}
		toUpdate = new boolean[NUMBER_OF_CUBES];
	}
	
	private void rotateCubes() {
		for(int i=0; i<cubes.size(); i++) {
			if(i%2 == 0) {
				continue;
			}
			Rotation oldRotation = cubes.get(i).getRotation();
			Rotation newRotation;
			if(i%2 == 0) {
				newRotation = new Rotation (oldRotation.getX()+DELTA_ROTATION,oldRotation.getY(),oldRotation.getZ());
			}else {
				newRotation = new Rotation (oldRotation.getX(),oldRotation.getY()+DELTA_ROTATION,oldRotation.getZ());
			}
			cubes.get(i).setRotation(newRotation);
			toUpdate[i] = true;
		}
	}

}
