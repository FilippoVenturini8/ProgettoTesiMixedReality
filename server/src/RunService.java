import io.vertx.core.Vertx;

/*
 * Data Service as a vertx event-loop 
 */
public class RunService {
	
	private static DataService service;	

	public static void main(String[] args) throws Exception{
		Vertx vertx = Vertx.vertx();
		service = new DataService(8080);
		vertx.deployVerticle(service);
		System.out.println("Ready.");
	}
}