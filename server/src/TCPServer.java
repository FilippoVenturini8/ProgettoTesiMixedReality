import io.vertx.core.AbstractVerticle;
import io.vertx.core.Handler;
import io.vertx.core.Vertx;
import io.vertx.core.buffer.Buffer;
import io.vertx.core.net.NetServer;
import io.vertx.core.net.NetSocket;

public class TCPServer extends AbstractVerticle {
	
	@Override
    public void start() throws Exception {
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

                        Buffer outBuffer = Buffer.buffer();
                        outBuffer.appendString("Response...");

                        netSocket.write(outBuffer);
                    }
                });
            }
        });
        server.listen(10000);
        System.out.println("Server TCP listening on port 10000...");
    }
}
