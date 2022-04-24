using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject Home,GamePlay,GameOver;
    public TMPro.TextMeshProUGUI scoreTxt;
    public static UIManager instance;
    public TMPro.TextMeshProUGUI[] BestScoreTxt,Points_txt;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        Home.SetActive(true);
        fetchBestScore();
      
    }

    public int fetchBestScore()
    {
        var value = PlayerPrefs.GetInt("BestScore");
        foreach (var x in BestScoreTxt)
        {
            x.text = "Best Score: " + value;
        }
        return value;
    }
   public void StartGame()
    {
        Home.SetActive(false);
        GamePlay.SetActive(true);
        GameManager.instance.startGame();
    }

    public void GameFinish()
    {
        Debug.Log("GameFinish");
        GameManager.instance.TrySetHighScore();
        fetchBestScore();
    }

    public void SendScore(int value)
    {
        scoreTxt.text ="AGE: "+ value.ToString();
    }

    public void AddPoints(int value)
    {
        foreach(var x in Points_txt)
        {
            x.text = "Score: "+value.ToString();
        }
    }

    public void RestartGame()
    {
        GameManager.instance.RestartGame();
    }

    public void ShowGameOver()
    {
        GameOver.SetActive(true);
        GameFinish();
    }
}
