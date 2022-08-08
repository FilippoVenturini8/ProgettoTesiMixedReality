import io.vertx.core.AbstractVerticle;
import io.vertx.core.Handler;
import io.vertx.core.Vertx;
import io.vertx.core.buffer.Buffer;
import io.vertx.core.net.NetServer;
import io.vertx.core.net.NetSocket;
import model.Cube;
import model.Position;
import model.Rotation;
import model.Scale;

public class TCPServer extends AbstractVerticle {
	private NetSocket netSocketProva;
	private Cube cube;
	private boolean isConnected = false;
	
	private static float DELTA_ROTATION = 0.5f;
	
	@Override
    public void start() throws Exception {
		this.initCube();
		
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
                netSocketProva = netSocket;
                isConnected = true;
                
                /*Buffer outBuffer = Buffer.buffer();
                outBuffer.appendString("CIAO");

                netSocket.write(outBuffer);*/
            }
        });
        server.listen(10000);
        System.out.println("Server TCP listening on port 10000...");
    }
	
	public void send() {
		if(!isConnected) {
			return;
		}
		
		this.rotateCube();
		System.out.println("[TCP] Sending rotation: "+ Float.toString(this.cube.getRotation().getX()) +" to 192.168.40.102");
		
		Buffer outBuffer = Buffer.buffer();
        outBuffer.appendString(Float.toString(DELTA_ROTATION));

        netSocketProva.write(outBuffer);
	}
	
	private void initCube() {
		Position position = new Position(0,0,0);
		Rotation rotation = new Rotation(0,0,0);
		Scale scale = new Scale(0.2f, 0.2f, 0.2f);
		
		this.cube = new Cube(position, rotation, scale);
	}
	
	private void rotateCube() {
		Rotation oldRotation = this.cube.getRotation();
		Rotation newRotation = new Rotation (oldRotation.getX()+DELTA_ROTATION,oldRotation.getY(),oldRotation.getZ());
		this.cube.setRotation(newRotation);
	}
}
