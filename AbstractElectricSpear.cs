using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AbstractElectricSpear : AbstractPhysicalObject
{
    public int stuckInWallCycles;
    public bool stuckVertically;
    public int charge;

    public AbstractElectricSpear(World world, ElectricSpear realizedObject, WorldCoordinate pos, EntityID ID, int charge) : base(world, (AbstractPhysicalObject.AbstractObjectType)31, realizedObject, pos, ID)
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
        return string.Concat(new object[]
        {
                this.ID.ToString(),
                "<oA>",
                this.type.ToString(),
                "<oA>",
                this.pos.room,
                ".",
                this.pos.x,
                ".",
                this.pos.y,
                ".",
                this.pos.abstractNode,
                "<oA>",
                this.stuckInWallCycles.ToString(),
                "<oA>",
                this.charge
        });
    }
}