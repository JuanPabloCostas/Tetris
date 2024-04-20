using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NewBehaviourScript : MonoBehaviour
{
    public TMPro.TextMeshProUGUI scoreText;

    private void Start()
    {
        if (scoreText != null) { 
            scoreText.text = PlayerPrefs.GetInt("lastHighScore").ToString();
        }
    }
    public void PlayAgain()
    {
        SceneManager.LoadScene("Level");

    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("GameMenu");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
