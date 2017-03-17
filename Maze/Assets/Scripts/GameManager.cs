using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Vector3 offset;
    private MazeGen maze;
    private int brickWidth;
    private int brickHeight;
    private void Start()
    {
        BeginGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    

    private void BeginGame()
    {
        GameObject []gameObjs = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < gameObjs.Length;i++)
        {
            maze = gameObjs[i].GetComponent<MazeGen>();
            if (null != maze)
            {
                brickWidth = maze.width;
                brickHeight = maze.height;
                offset = maze.brickScale;
                break;
            }
        }
        this.transform.position = new Vector3((brickWidth>>1) * offset.x, 0, 0 * offset.z);
        return;
    }

    private void RestartGame()
    {   
        BeginGame();
    }
}
