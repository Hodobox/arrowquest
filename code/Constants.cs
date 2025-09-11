public enum Direction
{
    UP,
    RIGHT,
    DOWN,
    LEFT
}

public static class Extensions
{
    public static Direction Opposite(this Direction dir)
    {
        return dir switch
        {
            Direction.UP => Direction.DOWN,
            Direction.DOWN => Direction.UP,
            Direction.LEFT => Direction.RIGHT,
            Direction.RIGHT => Direction.DOWN,
            _ => throw new System.NotImplementedException()
        };
    }
}

public static class Constants
{
    public const int TILE_SIZE = 32;
}