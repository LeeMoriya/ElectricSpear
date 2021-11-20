using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFisobs;

public sealed class ElectricSpearFisob : Fisob
{
    public static readonly ElectricSpearFisob Instance = new();

    private ElectricSpearFisob() : base("electric_spear") { }
    private static readonly ElectricSpearProperties properties = new();

    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData)
    {
        string[] p = saveData.CustomData.Split(';');

        return new AbstractElectricSpear(world, null, saveData.Pos, saveData.ID, 3)
        {
            charge = int.Parse(p[0])
        };
    }

    public override FisobProperties GetProperties(PhysicalObject forObject)
    {
        return properties;
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