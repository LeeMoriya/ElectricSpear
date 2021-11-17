using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;

[BepInPlugin("LeeMoriya.ElectricSpear", "ElectricSpear", "0.1")]
public class ElectricSpearPlugin : BaseUnityPlugin
{
    public static BaseUnityPlugin instance;
    public ElectricSpearPlugin()
    {
        instance = this;
    }

    public void OnEnable()
    {
        
    }
}