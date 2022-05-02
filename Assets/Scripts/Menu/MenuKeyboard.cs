using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuKeyboard : MonoBehaviour
{
    public GameObject[] buttons;
    public GameObject LSelect;
    public int currentSelection;

    public bool isActive;
    private bool isLSelect;

    void Awake()
    {
        currentSelection = 1; //Keep this at 0 for Browser build or else it can mess up Mouse Controls on the menu

        isActive = true;

        for (var i = 1; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<MenuButton>();
        }

        if (buttons.Length > 4)
        {
            isLSelect = true;
        }
        else isLSelect = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            isActive = true;
            currentSelection -= 1;
            if (currentSelection <= 0)
            {
                currentSelection = 3;
            }
            else if (isLSelect && currentSelection == 3)
            {
                currentSelection = 6;
            }
        }

        if (Input.GetKeyDown("d"))
        {
            isActive = true;
            currentSelection += 1;
            if (currentSelection == 4)
            {
                currentSelection = 1;
            }
            else if (currentSelection == 7)
            {
                currentSelection = 4;
            }
        }

        if (Input.GetKeyDown("w") && isLSelect)
        {
            isActive = true;
            currentSelection -= 3;
            if (currentSelection <= 0)
            {
                currentSelection += 6;
            }
        }

        if (Input.GetKeyDown("s") && isLSelect)
        {
            isActive = true;
            currentSelection += 3;
            if (currentSelection >= 7)
            {
                currentSelection -= 6;
            }
        }

        if(DataSaverLoader.Gd.IsArcadeBuild)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isActive = true;
                buttons[currentSelection].GetComponent<MenuButton>().UseButton();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isActive = true;
                buttons[currentSelection].GetComponent<MenuButton>().UseButton();
            }
        }

        /*
        if (Input.GetKeyDown(DataSaverLoader.Gd.SelectAndPlace))
        {
            isActive = true;
            buttons[currentSelection].GetComponent<MenuButton>().UseButton();
        }
        */
        if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(DataSaverLoader.Gd.MenuAndBack))
        {
            SceneManager.LoadScene(0);
        }

        if (Input.GetKeyDown("x") && isLSelect && currentSelection > 0)
        {
            LoadScoreBoardDisplayScreen(currentSelection);
        }

        if (isActive) UpdateButtons();
    }

    void UpdateButtons()
    {
        for (var i = 1; i < buttons.Length; i++)
        {
            if (currentSelection == i)
            {
                buttons[i].GetComponent<MenuButton>().isSelected = true;
            }
            else buttons[i].GetComponent<MenuButton>().isSelected = false;
        }
    }

    public void DisableAllButtons()
    {
        for (var i = 1; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<MenuButton>().isSelected = false;
        }
    }

    void LoadScoreBoardDisplayScreen(int Level)
    {
        DataSaverLoader.Gd.LatestLevel = Level;
        DataSaverLoader.SaveData();
        SceneManager.LoadScene(8);
    }
}
