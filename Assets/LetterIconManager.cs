using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LetterIconManager : MonoBehaviour
{

    private string[] AlphabetAndNums = { "_", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    [SerializeField] private GameObject[] LetterIcons;
    [SerializeField] private TextMeshPro SelectedLetterIcon;
    [SerializeField] private TextMeshPro ScoreText;
    private int CurrentLetterIndex;
    private int SelectedLeterIconIndex;
    private bool Flip;
    private string FinalName;
    private bool thingie;

    private void Start()
    {
        FinalName = "";
        CurrentLetterIndex = 0;
        SelectedLeterIconIndex = 0;
        ScoreText.text = "Score: " + DataSaverLoader.Gd.LatestScore.ToString();
        InvokeRepeating("Blink", 0, 0.5f);
        thingie = false;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            CurrentLetterIndex++;
            CurrentLetterIndex = MyMod(CurrentLetterIndex, AlphabetAndNums.Length);
            SelectedLetterIcon.text = AlphabetAndNums[CurrentLetterIndex];
            SelectedLetterIcon.fontSize = 25;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            CurrentLetterIndex--;
            CurrentLetterIndex = MyMod(CurrentLetterIndex, AlphabetAndNums.Length);
            SelectedLetterIcon.text = AlphabetAndNums[CurrentLetterIndex];
            SelectedLetterIcon.fontSize = 25;
        }
        else if(DataSaverLoader.Gd.IsArcadeBuild)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                thingie = true;
            }
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            {
                thingie = true;
            }
        }
        
        if(thingie)
        {
            CurrentLetterIndex = 0;
            FinalName = FinalName + SelectedLetterIcon.text;
            SelectedLeterIconIndex++;
            SelectedLetterIcon.fontSize = 25;

            if (SelectedLeterIconIndex < LetterIcons.Length)
            {
                SelectedLetterIcon = LetterIcons[SelectedLeterIconIndex].GetComponent<TextMeshPro>();
            }
            else if (DataSaverLoader.Gd.LatestScore > DataSaverLoader.Gd.Scoreboards[DataSaverLoader.Gd.LatestLevel - 1].Slots[DataSaverLoader.Gd.Scoreboards[DataSaverLoader.Gd.LatestLevel - 1].Slots.Length - 1].Score)
            {
                DataSaverLoader.Gd.Scoreboards[DataSaverLoader.Gd.LatestLevel - 1].Slots[DataSaverLoader.Gd.Scoreboards[DataSaverLoader.Gd.LatestLevel - 1].Slots.Length - 1].PlayerName = FinalName;
                DataSaverLoader.Gd.Scoreboards[DataSaverLoader.Gd.LatestLevel - 1].Slots[DataSaverLoader.Gd.Scoreboards[DataSaverLoader.Gd.LatestLevel - 1].Slots.Length - 1].Score = DataSaverLoader.Gd.LatestScore;
                DataSaverLoader.SortData(DataSaverLoader.Gd.LatestLevel);
                DataSaverLoader.SaveData();

                //go back to main menu
                SceneManager.LoadScene(0);
            }
            else
            {
                //go back to main menu
                SceneManager.LoadScene(0);
            }

            thingie = false;
        }
    }

    private void Blink()
    {
        if (Flip)
        {
            SelectedLetterIcon.fontSize = 23;
            Flip = false;
        }
        else
        {
            SelectedLetterIcon.fontSize = 25;
            Flip = true;
        }
    }

    private static int MyMod(int num, int mod)
    {
        return ((num % mod) + mod) % mod;
    }
}
