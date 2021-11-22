using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using ESFisobs;
using Menu;
using RWCustom;
using System.IO;

[BepInPlugin("LeeMoriya.ElectricSpear", "ElectricSpear", "1.0")]
public class ElectricSpearPlugin : BaseUnityPlugin
{
    public static bool recharge = false;
    public static BaseUnityPlugin instance;
    public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/4/8";
    public int version = 1;
    public string keyE = "AQAB";
    public string keyN = "lDaM5h0hJUvZcIdiWXH4qfdia/V8UWzikqRIiC9jVGA87jMrafo4EWOTk0MMIQZWHVy+msVzvEAVR3V45wZShFu7ylUndroL5u4zyqHfVeAeDIALfBrM3J4BIM1rMi4wieYdLIF6t2Uj4GVH7iU59AIfobew1vICUILu9Zib/Aw2QY6Nc+0Cz6Lw3xh7DL/trIMaW7yQfYRZUaEZBHelN2JGyUjKkbby4vL6gySfGlVl1OH0hYYhrhNwnQrOow8WXFMIu/WyTA3cY3wqkjd4/WRJ+EvYtMKTwfG+TZiHGst9Bg1ZTFfvEvrTFiPadTf19iUnfyL/QJaTAD8qe+rba5KwirIElovqFpYNH9tAr7SpjixjbT3Igmz+SlqGa9wSbm1QWt/76QqpyAYV/b5G/VzbytoZrhkEVdGuaotD4tXh462AhK5xoigB8PEt+T3nWuPdoZlVo5hRCxoNleH4yxLpVv8C7TpQgQHDqzHMcEX79xjiYiCvigCq7lLEdxUD0fhnxSYVK0O+y7T+NXkk3is/XqJxdesgyYUMT81MSou9Ur/2nv9H8IvA9QeIqso05hK3c496UOaRJS27WJhrxABtU+HHtxo9SifmXjisDj3IV46uTeVp5bivDTu1yBymgnU8qli/xmwWxKvOisi9ZOZsg4vFHaY31gdUBWOz4dU=";

    public ElectricSpearPlugin()
    {
        instance = this;
    }

    public void OnEnable()
    {
        //Fisob
        static IEnumerable<Fisob> GetFisobs()
        {
            yield return ElectricSpearFisob.Instance;
        }
        FisobRegistry reg = new(GetFisobs());
        reg.ApplyHooks();

        //Hooks
        //On.Player.Update += Player_Update;
        On.ScavengerTreasury.ctor += ScavengerTreasury_ctor;
        On.RainWorldGame.ctor += RainWorldGame_ctor;
    }

    private void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig.Invoke(self, manager);
        RechargeCheck();
    }

    private void RechargeCheck()
    {
        string dir = Custom.RootFolderDirectory();
        Debug.Log(dir + "recharge.txt");
        if(Directory.GetFiles(dir).Contains(dir + "recharge.txt"))
        {
            recharge = true;
        }
    }

    private void ScavengerTreasury_ctor(On.ScavengerTreasury.orig_ctor orig, ScavengerTreasury self, Room room, PlacedObject placedObj)
    {
        orig.Invoke(self, room, placedObj);
        if (room.abstractRoom.firstTimeRealized)
        {
            WorldCoordinate coord = room.GetWorldCoordinate(self.tiles[UnityEngine.Random.Range(0, self.tiles.Count - 1)]);
            room.abstractRoom.entities.Add(new AbstractElectricSpear(room.world, null, coord, room.game.GetNewID(), 3));
        }
    }

    private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig.Invoke(self, eu);
        if(self != null && Input.GetKeyDown(KeyCode.Alpha9))
        {
            AbstractElectricSpear apo = new AbstractElectricSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), 3);
            self.room.abstractRoom.AddEntity(apo as AbstractElectricSpear);
            (apo as AbstractElectricSpear).RealizeInRoom();
            self.SlugcatGrab((apo as AbstractElectricSpear).realizedObject, 0);
            Debug.Log("Spawned Electric Spear!");
        }
    }
}