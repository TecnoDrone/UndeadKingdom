using UnityEngine;

public class Wall : MonoBehaviour
{
  public Direction facing = Direction.None;
}

public enum Direction
{
  North,
  NorthEast,
  East,
  SouthEast,
  South,
  SouthWest,
  West,
  NorthWest,
  None
}
