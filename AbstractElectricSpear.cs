using ESFisobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AbstractElectricSpear : AbstractSpear
{
    public int charge;

    public AbstractElectricSpear(World world, ElectricSpear realizedObject, WorldCoordinate pos, EntityID ID, int charge) : base(world, realizedObject, pos, ID, false)
    {
        this.type = ElectricSpearFisob.Instance.Type;
        this.charge = charge;
    }

    public bool StuckInWall
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

    public override string ToString()
    {
        return this.SaveToString($"{charge};");
    }
}