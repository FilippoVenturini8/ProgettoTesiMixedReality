package model;

public class Cube {
	
	private Position position;
	private Rotation rotation;
	private Scale scale;
	
	public Cube(Position position, Rotation rotation, Scale scale) {
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
	}

	public Position getPosition() {
		return position;
	}

	public void setPosition(Position position) {
		this.position = position;
	}

	public Rotation getRotation() {
		return rotation;
	}

	public void setRotation(Rotation rotation) {
		this.rotation = rotation;
	}

	public Scale getScale() {
		return scale;
	}

	public void setScale(Scale scale) {
		this.scale = scale;
	}

}
