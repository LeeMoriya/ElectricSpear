using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using ESFisobs;

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
        static IEnumerable<Fisob> GetFisobs()
        {
            yield return ElectricSpearFisob.Instance;
        }
        FisobRegistry reg = new(GetFisobs());
        reg.ApplyHooks();

        On.Player.Update += Player_Update;
    }

    private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig.Invoke(self, eu);

        if(self != null && Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("Gonna spawn an Electric Spear");
            AbstractElectricSpear apo = new AbstractElectricSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), 3);
            self.room.abstractRoom.AddEntity(apo as AbstractElectricSpear);
            (apo as AbstractElectricSpear).RealizeInRoom();
            self.SlugcatGrab((apo as AbstractElectricSpear).realizedObject, 0);
            Debug.Log("Spawned Electric Spear!");
        }
    }
}