using System.Collections.Generic;

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
            Direction.RIGHT => Direction.LEFT,
            _ => throw new System.NotImplementedException()
        };
    }

    public static List<Direction> Perpendicular(this Direction dir)
    {
        switch (dir)
        {
            case Direction.UP:
            case Direction.DOWN:
                return [Direction.LEFT, Direction.RIGHT];
            case Direction.RIGHT:
            case Direction.LEFT:
                return [Direction.UP, Direction.DOWN];
            default:
                throw new System.NotImplementedException();
        }
    }

    public static List<Direction> Parallel(this Direction dir)
    {
        switch (dir)
        {
            case Direction.UP:
            case Direction.DOWN:
                return [Direction.UP, Direction.DOWN];
            case Direction.RIGHT:
            case Direction.LEFT:
                return [Direction.RIGHT, Direction.LEFT];
            default:
                throw new System.NotImplementedException();
        }
    }
}

public static class Constants
{
    public const int TILE_SIZE = 32;
}