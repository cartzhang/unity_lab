// mazegen.cs
// https://forum.unity3d.com/threads/quick-maze-generator.173370/
// remember you can NOT have even numbers of height or width in this style of block maze
// to ensure we can get walls around all tunnels...  so use 21 x 13 , or 7 x 7 for examples.
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public enum ExitSide
{
    E_RANDOM,
    E_LEFT,
    E_RIGHT,
}

public class MazeGen : MonoBehaviour
{
    public int width, height;
    public Material brick;
    public Vector3 brickScale = new Vector3(1.5f, 2.8f, 1.5f);
         
    //
    private int[,] Maze;
    private Stack<Vector2> _tiletoTry = new Stack<Vector2>();
    private List<Vector2> offsets = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    private System.Random rnd = new System.Random();
    private int _width, _height;
    private Vector2 _currentTile;
    public static String MazeString;
    private GameObject ptype = null;
    public Vector2 CurrentTile
    {
        get { return _currentTile; }
        private set
        {
            if (value.x < 1 || value.x >= this.width - 1 || value.y < 1 || value.y >= this.height - 1)
            {
                throw new ArgumentException("Width and Height must be greater than 2 to make a maze");
            }
            _currentTile = value;
        }
    }
    private static MazeGen instance;
    public static MazeGen Instance
    {
        get { return instance; }
    }
    [Header("出口方向")]
    public ExitSide exitSide = ExitSide.E_RANDOM;
    void Awake()
    {
        instance = this;
        exitSide = (rnd.Next(2) % 2 == 0) ? ExitSide.E_LEFT : ExitSide.E_RIGHT;
        Debug.Log(exitSide.ToString());
        MakeBlocks();
    }
    // end of main program
    // ============= subroutines ============
    void MakeBlocks()
    {
        Maze = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Maze[x, y] = 1;
            }
        }
        CurrentTile = Vector2.one;
        _tiletoTry.Push(CurrentTile);
        Maze = CreateMaze();  // generate the maze in Maze Array.
       
        for (int i = 0; i <= Maze.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= Maze.GetUpperBound(1); j++)
            {
                if (CreateExitGate(exitSide,i,j))
                {
                    MazeString = MazeString + "."; // added to create String
                    continue;
                }
                if (Maze[i, j] == 1)
                {
                    MazeString = MazeString + "X";  // added to create String
                    CreatBrick(i, j);
                }
                else if (Maze[i, j] == 0)
                {
                    MazeString = MazeString + "."; // added to create String
                }
            }
            MazeString = MazeString + "\n";  // added to create String
        }

        // start from middle,to check if has block at start.
        CheckIfBlockedAtStart();
        print(MazeString);  // added to create String`
    }
    // =======================================
    public int[,] CreateMaze()
    {

        //local variable to store neighbors to the current square as we work our way through the maze
        List<Vector2> neighbors;
        //as long as there are still tiles to try
        while (_tiletoTry.Count > 0)
        {
            //excavate the square we are on
            Maze[(int)CurrentTile.x, (int)CurrentTile.y] = 0;
            //get all valid neighbors for the new tile
            neighbors = GetValidNeighbors(CurrentTile);
            //if there are any interesting looking neighbors
            if (neighbors.Count > 0)
            {
                //remember this tile, by putting it on the stack
                _tiletoTry.Push(CurrentTile);
                //move on to a random of the neighboring tiles
                CurrentTile = neighbors[rnd.Next(neighbors.Count)];
            }
            else
            {
                //if there were no neighbors to try, we are at a dead-end toss this tile out
                //(thereby returning to a previous tile in the list to check).
                CurrentTile = _tiletoTry.Pop();
            }
        }
        print("Maze Generated ...");
        return Maze;
    }

    // ================================================
    // Get all the prospective neighboring tiles "centerTile" The tile to test
    // All and any valid neighbors</returns>
    private List<Vector2> GetValidNeighbors(Vector2 centerTile)
    {
        List<Vector2> validNeighbors = new List<Vector2>();
        //Check all four directions around the tile
        foreach (var offset in offsets)
        {
            //find the neighbor's position
            Vector2 toCheck = new Vector2(centerTile.x + offset.x, centerTile.y + offset.y);
            //make sure the tile is not on both an even X-axis and an even Y-axis
            //to ensure we can get walls around all tunnels
            if (toCheck.x % 2 == 1 || toCheck.y % 2 == 1)
            {

                //if the potential neighbor is unexcavated (==1)
                //and still has three walls intact (new territory)
                if (Maze[(int)toCheck.x, (int)toCheck.y] == 1 && HasThreeWallsIntact(toCheck))
                {
                    //add the neighbor
                    validNeighbors.Add(toCheck);
                }
            }
        }
        return validNeighbors;
    }
    // ================================================
    // Counts the number of intact walls around a tile
    //"Vector2ToCheck">The coordinates of the tile to check
    //Whether there are three intact walls (the tile has not been dug into earlier.
    private bool HasThreeWallsIntact(Vector2 Vector2ToCheck)
    {

        int intactWallCounter = 0;
        //Check all four directions around the tile
        foreach (var offset in offsets)
        {

            //find the neighbor's position
            Vector2 neighborToCheck = new Vector2(Vector2ToCheck.x + offset.x, Vector2ToCheck.y + offset.y);
            //make sure it is inside the maze, and it hasn't been dug out yet
            if (IsInside(neighborToCheck) && Maze[(int)neighborToCheck.x, (int)neighborToCheck.y] == 1)
            {
                intactWallCounter++;
            }
        }
        //tell whether three walls are intact
        return intactWallCounter == 3;
    }

    // ================================================
    private bool IsInside(Vector2 p)
    {
        //return p.x >= 0  p.y >= 0  p.x < width  p.y < height;
        return p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
    }
    // current coordinate to check, whether to make cube.
    private bool CreateExitGate(ExitSide side,int i,int j)
    {
        bool isOk = false;
        int halfPos = width >> 1;
        switch (side)
        {   
            case ExitSide.E_LEFT:
                if ((j == 0 && i == halfPos) || (j == (height - 2) && i == 0))
                {
                    isOk = true;
                }
                break;
            case ExitSide.E_RIGHT:
                if ((j == 0 && i == halfPos) || (j == (height - 2) && i == (width - 1)))
                {
                    isOk = true;
                }
                break;
            default:
                Debug.LogError("exit  type error");
                break;
        }
        return isOk;
    }

    private void CheckIfBlockedAtStart()
    {
        int halfPos = width >> 1;
        string blockCubeName = "Cube" + halfPos + "_" + 1;
        GameObject blockCube = GameObject.Find(blockCubeName);
        if (null != blockCube)
        {
            Debug.Log("blocked at start.");
            // move left neighbor to middle.
            blockCubeName = "Cube" + (halfPos-1) + "_" + 0;
            blockCube = GameObject.Find(blockCubeName);
            Destroy(blockCube);
            //
            CreatBrick(halfPos, 0);
        }
    }

    private void CreatBrick(int i,int j)
    {
        ptype = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ptype.transform.localScale = brickScale;
        ptype.transform.position = new Vector3(i * ptype.transform.localScale.x, 0, j * ptype.transform.localScale.z);
        if (brick != null) { ptype.GetComponent<Renderer>().material = brick; }
        ptype.transform.parent = transform;
        ptype.name += i + "_" + j;
        return;
    }
}