using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ESFisobs;

public sealed class ElectricSpearFisob : Fisob
{
    public static readonly ElectricSpearFisob Instance = new();

    private ElectricSpearFisob() : base("electric_spear")
    {

    }
    private static readonly ElectricSpearProperties properties = new();

    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData)
    {
        string[] p = saveData.CustomData.Split(';');
        if (ElectricSpearPlugin.recharge && world.game.IsStorySession && (world.GetAbstractRoom(saveData.Pos)?.shelter ?? false) && world.rainCycle.CycleProgression < 0.1f)
        {
            return new AbstractElectricSpear(world, null, saveData.Pos, saveData.ID, 3)
            {
                charge = 3
            };
        }
        return new AbstractElectricSpear(world, null, saveData.Pos, saveData.ID, 3)
        {
            charge = int.TryParse(p[0], out var h) ? h : 3
        };
    }

    public override FisobProperties GetProperties(PhysicalObject forObject)
    {
        return properties;
    }

    public override SandboxState GetSandboxState(MultiplayerUnlocks unlocks)
    {
        return SandboxState.Unlocked;
    }

    private sealed class ElectricSpearProperties : FisobProperties
    {
        public override void CanThrow(Player player, ref bool throwable)
            => throwable = true;

        public override void GetScavCollectibleScore(Scavenger scavenger, ref int score)
            => score = 3;

        public override void GetScavWeaponPickupScore(Scavenger scavenger, ref int score)
            => score = 3;

        public override void GetGrabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.BigOneHand;
        }
    }
}