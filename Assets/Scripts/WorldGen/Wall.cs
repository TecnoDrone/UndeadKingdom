using UnityEngine;

public class Wall
{
  private GameObject goCap;
  private GameObject goWall;

  public void Apply()
  {

  }
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
