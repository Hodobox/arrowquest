using Godot;

public static class MoveUtils {

    public static Vector2 GetVelocityFromDirection(Direction dir) {
        switch(dir) {
            case Direction.UP:
                return Vector2.Up;
            case Direction.RIGHT:
                return Vector2.Right;
            case Direction.DOWN:
                return Vector2.Down;
            case Direction.LEFT:
            default:
                return Vector2.Left;
        }
    }

}