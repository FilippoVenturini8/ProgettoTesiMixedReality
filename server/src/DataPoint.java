class DataPoint {
	private int temperature;
	private int lux;
	
	public DataPoint(int temperature, int lux) {
		this.temperature = temperature;
		this.lux = lux;
	}
	
	public int getTemperature() {
		return temperature;
	}
	
	public int getLux() {
		return lux;
	}
}
