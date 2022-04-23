using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
        Time.timeScale = 0;
    }
    public void startGame()
    {
        Time.timeScale = 1;
    }
<<<<<<< HEAD
=======

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
>>>>>>> 342b86504ee61a54557915a895488c1b6ffa8208
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
