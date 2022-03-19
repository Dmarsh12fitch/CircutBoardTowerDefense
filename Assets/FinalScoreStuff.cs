using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class FinalScoreStuff : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI FinalScoreText;
    [SerializeField] private ScoreTracker ScoreTrackerScript;
    [SerializeField] private int Level;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        FinalScoreText.text = "Final Score\n" + ScoreTrackerScript.GetScore().ToString();
        PlayerPrefs.SetInt("TempFinalScore", ScoreTrackerScript.GetScore());
        PlayerPrefs.SetInt("TempLevelPlayed", Level);
        StartCoroutine(EnterName());   
    }

    IEnumerator EnterName()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(7);
    }
}