using ESFisobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AbstractElectricSpear : AbstractPhysicalObject
{
    public int stuckInWallCycles;
    public bool stuckVertically;
    public int charge;

    public AbstractElectricSpear(World world, ElectricSpear realizedObject, WorldCoordinate pos, EntityID ID, int charge) : base(world, ElectricSpearFisob.Instance.Type, realizedObject, pos, ID)
    {
        this.charge = charge;
    }

    public bool stuckInWall
    {
        get
        {
            return this.stuckInWallCycles != 0;
        }
    }

    public override void Realize()
    {
        realizedObject ??= new ElectricSpear(this, this.world);
        base.Realize();
    }

    public void StuckInWallTick(int ticks)
    {
        if (this.stuckInWallCycles > 0)
        {
            this.stuckInWallCycles = Math.Max(0, this.stuckInWallCycles - ticks);
        }
        else if (this.stuckInWallCycles < 0)
        {
            this.stuckInWallCycles = Math.Min(0, this.stuckInWallCycles + ticks);
        }
    }

    public override string ToString()
    {
        return this.SaveToString($"{charge};");
    }
}