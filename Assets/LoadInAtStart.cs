using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadInAtStart : MonoBehaviour
{
    private void Awake()
    {


        if (!DataSaverLoader.LoadData())
        {
            DataSaverLoader.NewData();
        }

        DataSaverLoader.Gd.IsArcadeBuild = true;

        if (DataSaverLoader.Gd.IsArcadeBuild)
        {
            //arcade controls
            DataSaverLoader.Gd.MenuAndBack = "c";
            DataSaverLoader.Gd.CallNextWave = "1";
            DataSaverLoader.Gd.SelectAndPlace = "space";
            DataSaverLoader.Gd.SellTurret = "v";
        }
        else
        {
            //keyboard mouse build
            DataSaverLoader.Gd.MenuAndBack = "q";
            DataSaverLoader.Gd.CallNextWave = "space";
            DataSaverLoader.Gd.SelectAndPlace = "e";
            DataSaverLoader.Gd.SellTurret = "r";
        }
        
    }
}
