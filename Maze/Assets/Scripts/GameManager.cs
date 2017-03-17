using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
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
        
    }

    private void RestartGame()
    {   
        BeginGame();
    }
}
