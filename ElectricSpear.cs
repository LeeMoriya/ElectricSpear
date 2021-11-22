using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;

public class ElectricSpear : Spear
{
    public Color redColor = Color.red;
    public Color whiteColor = Color.white;
    public World world;
    private new int stuckInChunkIndex;
    private bool charged;
    private bool depleted;
    private new int stuckBodyPart;
    private new bool spinning;
    protected new bool pivotAtTip;
    public new Appendage.Pos stuckInAppendage;
    public new float stuckRotation;
    public new Vector2? stuckInWall;
    public new bool alwaysStickInWalls;
    public new int pinToWallCounter;
    private new bool addPoles;
    public new float spearDamageBonus;
    private new int stillCounter;
    public LightSource lightSource;
    public float lightFlash;

    public ElectricSpear(AbstractElectricSpear abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        this.abstractPhysicalObject = abstractPhysicalObject;
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
        bodyChunkConnections = new BodyChunkConnection[0];
        airFriction = 0.999f;
        base.gravity = 0.9f;
        bounce = 0.4f;
        surfaceFriction = 0.4f;
        collisionLayer = 2;
        waterFriction = 0.98f;
        buoyancy = 0.4f;
        pivotAtTip = false;
        lastPivotAtTip = false;
        stuckBodyPart = -1;
        firstChunk.loudness = 7f;
        tailPos = firstChunk.pos;
        soundLoop = new ChunkDynamicSoundLoop(firstChunk);
    }

    public override bool HeavyWeapon
    {
        get
        {
            return true;
        }
    }

    public AbstractElectricSpear abstractElectricSpear
    {
        get
        {
            return abstractPhysicalObject as AbstractElectricSpear;
        }
    }

    public new BodyChunk stuckInChunk
    {
        get
        {
            return stuckInObject.bodyChunks[stuckInChunkIndex];
        }
    }

    public new float gravity
    {
        get
        {
            return g * room.gravity;
        }
        protected set
        {
            g = value;
        }
    }
    public int charge
    {
        get
        {
            return (abstractPhysicalObject as AbstractElectricSpear).charge;
        }
        set
        {
            (abstractPhysicalObject as AbstractElectricSpear).charge = value;
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[4];
        sLeaser.sprites[3] = new FSprite("SmallSpear", true);
        sLeaser.sprites[1] = new FSprite("pixel", true);
        sLeaser.sprites[1].scaleX = 2.3f;
        sLeaser.sprites[2] = new FSprite("pixel", true);
        sLeaser.sprites[2].scaleX = 2.3f;
        sLeaser.sprites[0] = new FSprite("pixel", true);
        sLeaser.sprites[0].scaleX = 2.3f;
        AddToContainer(sLeaser, rCam, null);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        sLeaser.sprites[3].color = color;
        sLeaser.sprites[1].color = whiteColor;
        sLeaser.sprites[1].scale = 2f;
        sLeaser.sprites[2].color = whiteColor;
        sLeaser.sprites[2].scale = 2f;
        sLeaser.sprites[0].color = redColor;
        sLeaser.sprites[0].scale = 2f;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        if (vibrate > 0)
        {
            vector += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
        }
        Vector3 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
        for (int i = 3; i >= 0; i--)
        {
            sLeaser.sprites[i].x = vector.x - camPos.x;
            sLeaser.sprites[i].y = vector.y - camPos.y;
            sLeaser.sprites[i].anchorY = Mathf.Lerp(!lastPivotAtTip ? 0.5f : 0.85f, !pivotAtTip ? 0.5f : 0.85f, timeStacker);
            sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), v);
        }
        sLeaser.sprites[1].anchorY += 10f;
        sLeaser.sprites[2].anchorY += 8f;
        sLeaser.sprites[0].anchorY += 6f;
        if (blink > 0 && Random.value < 0.5f)
        {
            sLeaser.sprites[0].color = blinkColor;
            sLeaser.sprites[1].color = blinkColor;
            sLeaser.sprites[2].color = blinkColor;
            sLeaser.sprites[3].color = blinkColor;
        }
        else
        {
            switch (charge)
            {
                case 0:
                    sLeaser.sprites[0].color = redColor;
                    sLeaser.sprites[1].color = redColor;
                    sLeaser.sprites[2].color = redColor;
                    sLeaser.sprites[3].color = color;
                    break;
                case 1:
                    sLeaser.sprites[0].color = redColor;
                    sLeaser.sprites[1].color = whiteColor;
                    sLeaser.sprites[2].color = redColor;
                    sLeaser.sprites[3].color = color;
                    break;
                case 2:
                    sLeaser.sprites[0].color = redColor;
                    sLeaser.sprites[1].color = whiteColor;
                    sLeaser.sprites[2].color = whiteColor;
                    sLeaser.sprites[3].color = color;
                    break;
                case 3:
                    sLeaser.sprites[0].color = whiteColor;
                    sLeaser.sprites[1].color = whiteColor;
                    sLeaser.sprites[2].color = whiteColor;
                    sLeaser.sprites[3].color = color;
                    break;
            }
        }
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        bool result2;
        if (result.obj == null)
        {
            result2 = false;
        }
        else
        {
            bool flag2 = false;
            if (abstractPhysicalObject.world.game.IsArenaSession && abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0 && thrownBy != null && thrownBy is Player && result.obj is Creature)
            {
                flag2 = true;
                if ((result.obj as Creature).State is HealthState && ((result.obj as Creature).State as HealthState).health <= 0f)
                {
                    flag2 = false;
                }
                else
                {
                    if (!((result.obj as Creature).State is HealthState) && (result.obj as Creature).State.dead)
                    {
                        flag2 = false;
                    }
                }
            }
            if (result.obj is Creature)
            {
                if (result.obj is Centipede)
                {
                    if (charge != 3 && !(result.obj as Centipede).dead)
                    {
                        (result.obj as Creature).Violence(firstChunk, new Vector2?(firstChunk.vel * firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, spearDamageBonus, 20f);
                        charged = true;
                        depleted = false;
                    }
                    else
                    {
                        (result.obj as Creature).Violence(firstChunk, new Vector2?(firstChunk.vel * firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, spearDamageBonus, 20f);
                        charged = false;
                        depleted = false;
                    }
                }
                else
                {
                    if (charge != 0)
                    {
                        (result.obj as Creature).Violence(firstChunk, new Vector2?(firstChunk.vel * firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, spearDamageBonus, 20f);
                        (result.obj as Creature).Violence(firstChunk, new Vector2?(firstChunk.vel * firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Electric, 6f, 20f);
                        charged = false;
                        depleted = true;
                    }
                    else
                    {
                        (result.obj as Creature).Violence(firstChunk, new Vector2?(firstChunk.vel * firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, spearDamageBonus, 20f);
                        charged = false;
                        depleted = false;
                    }
                }
            }
            else
            {
                if (result.chunk != null)
                {
                    result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass;
                }
                else
                {
                    if (result.onAppendagePos != null)
                    {
                        (result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass);
                    }
                }
            }
            if (result.obj is Creature && (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, Random.value), result.chunk, result.onAppendagePos, firstChunk.vel))
            {
                if (depleted)
                {
                    room.PlaySound(SoundID.Centipede_Shock, firstChunk);
                    int charge = this.charge;
                    this.charge = charge - 1;
                    for (int i = 0; i < 5; i++)
                    {
                        room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * 5, new Color(0.9f, 1f, 1f), null, 50, 90));
                    }
                }
                if (charged)
                {
                    room.PlaySound(SoundID.Centipede_Electric_Charge_LOOP, firstChunk);
                    int charge = this.charge;
                    this.charge = charge + 1;
                    charged = false;
                }
                room.PlaySound(SoundID.Spear_Stick_In_Creature, firstChunk);
                LodgeInCreature(result, eu);
                lightFlash = 1.3f;
                if (flag2)
                {
                    abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(thrownBy as Player, stuckInObject as Creature);
                }
                result2 = true;
            }
            else
            {
                room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, firstChunk);
                vibrate = 20;
                ChangeMode(Mode.Free);
                firstChunk.vel = firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * firstChunk.vel.magnitude;
                SetRandomSpin();
                result2 = false;
            }
        }
        return result2;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        soundLoop.sound = SoundID.None;
        if (firstChunk.vel.magnitude > 5f)
        {
            if (mode == Mode.Thrown)
            {
                soundLoop.sound = SoundID.Spear_Thrown_Through_Air_LOOP;
            }
            else
            {
                if (mode == Mode.Free)
                {
                    soundLoop.sound = SoundID.Spear_Spinning_Through_Air_LOOP;
                }
            }
            soundLoop.Volume = Mathf.InverseLerp(5f, 15f, firstChunk.vel.magnitude);
        }
        soundLoop.Update();
        lastPivotAtTip = pivotAtTip;
        pivotAtTip = mode == Mode.Thrown || mode == Mode.StuckInCreature;
        if (addPoles && room.readyForAI)
        {
            if (abstractSpear.stuckInWallCycles >= 0)
            {
                room.GetTile(stuckInWall.Value).horizontalBeam = true;
                for (int i = -1; i < 2; i += 2)
                {
                    if (!room.GetTile(stuckInWall.Value + new Vector2(20f * i, 0f)).Solid)
                    {
                        room.GetTile(stuckInWall.Value + new Vector2(20f * i, 0f)).horizontalBeam = true;
                    }
                }
            }
            else
            {
                room.GetTile(stuckInWall.Value).verticalBeam = true;
                for (int j = -1; j < 2; j += 2)
                {
                    if (!room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * j)).Solid)
                    {
                        room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * j)).verticalBeam = true;
                    }
                }
            }
            addPoles = false;
        }
        switch (mode)
        {
            case Mode.Free:
                {
                    if (spinning)
                    {
                        if (Custom.DistLess(firstChunk.pos, firstChunk.lastPos, 4f * room.gravity))
                        {
                            stillCounter++;
                        }
                        else
                        {
                            stillCounter = 0;
                        }
                        if (firstChunk.ContactPoint.y < 0 || stillCounter > 20)
                        {
                            spinning = false;
                            rotationSpeed = 0f;
                            rotation = Custom.DegToVec(Mathf.Lerp(-50f, 50f, Random.value) + 180f);
                            firstChunk.vel *= 0f;
                            room.PlaySound(SoundID.Spear_Stick_In_Ground, firstChunk);
                        }
                    }
                    else
                    {
                        if (!Custom.DistLess(firstChunk.lastPos, firstChunk.pos, 6f))
                        {
                            SetRandomSpin();
                        }
                    }
                    break;
                }
            case Mode.Thrown:
                {
                    if (Custom.DistLess(thrownPos, firstChunk.pos, 560f * Mathf.Max(1f, spearDamageBonus)) && firstChunk.ContactPoint == throwDir && room.GetTile(firstChunk.pos).Terrain == Room.Tile.TerrainType.Air && room.GetTile(firstChunk.pos + throwDir.ToVector2() * 20f).Terrain == Room.Tile.TerrainType.Solid && (Random.value < 0.33f || Custom.DistLess(thrownPos, firstChunk.pos, 140f) || alwaysStickInWalls))
                    {
                        bool flag13 = true;
                        foreach (AbstractWorldEntity abstractWorldEntity in room.abstractRoom.entities)
                        {
                            if (abstractWorldEntity is AbstractSpear && (abstractWorldEntity as AbstractSpear).realizedObject != null && ((abstractWorldEntity as AbstractSpear).realizedObject as Weapon).mode == Mode.StuckInWall && abstractWorldEntity.pos.Tile == abstractPhysicalObject.pos.Tile)
                            {
                                flag13 = false;
                                break;
                            }
                        }
                        if (flag13)
                        {
                            for (int k = 0; k < room.roomSettings.placedObjects.Count; k++)
                            {
                                if (room.roomSettings.placedObjects[k].type == PlacedObject.Type.NoSpearStickZone && Custom.DistLess(room.MiddleOfTile(firstChunk.pos), room.roomSettings.placedObjects[k].pos, (room.roomSettings.placedObjects[k].data as PlacedObject.ResizableObjectData).Rad))
                                {
                                    flag13 = false;
                                    break;
                                }
                            }
                        }
                        if (flag13)
                        {
                            stuckInWall = new Vector2?(room.MiddleOfTile(firstChunk.pos));
                            vibrate = 10;
                            ChangeMode(Mode.StuckInWall);
                            room.PlaySound(SoundID.Spear_Stick_In_Wall, firstChunk);
                            firstChunk.collideWithTerrain = false;
                        }
                    }
                    break;
                }
            case Mode.StuckInCreature:
                {
                    if (stuckInWall == null)
                    {
                        if (stuckInAppendage != null)
                        {
                            setRotation = new Vector2?(Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage))));
                            firstChunk.pos = stuckInAppendage.appendage.OnAppendagePosition(stuckInAppendage);
                        }
                        else
                        {
                            if (depleted)
                            {
                                if (lightSource != null)
                                {
                                    lightSource.stayAlive = true;
                                    lightSource.setPos = new Vector2?(firstChunk.pos);
                                    lightSource.setRad = new float?(300f * Mathf.Pow(lightFlash * Random.value, 0.01f) * Mathf.Lerp(0.5f, 2f, 0.8f) - 1.3f);
                                    lightSource.setAlpha = new float?(Mathf.Pow(lightFlash * Random.value, 0.01f) - 0.8f);
                                    float num = lightFlash * Random.value;
                                    num = Mathf.Lerp(num, 1f, 0.5f * (1f - room.Darkness(firstChunk.pos)));
                                    lightSource.color = new Color(num, num, 1.5f);
                                    if (lightFlash <= 0f)
                                    {
                                        lightSource.Destroy();
                                    }
                                    if (lightSource.slatedForDeletetion)
                                    {
                                        if (depleted)
                                        {
                                            depleted = false;
                                        }
                                        lightSource = null;
                                    }
                                }
                                else
                                {
                                    if (lightFlash > 0f)
                                    {
                                        lightSource = new LightSource(firstChunk.pos, false, new Color(1f, 1f, 1f), this);
                                        lightSource.affectedByPaletteDarkness = 0f;
                                        lightSource.requireUpKeep = true;
                                        room.AddObject(lightSource);
                                    }
                                }
                                if (lightFlash > 0f)
                                {
                                    lightFlash = Mathf.Max(0f, lightFlash - 0.0333933346f);
                                }
                            }
                            firstChunk.vel = stuckInChunk.vel;
                            if (stuckBodyPart == -1 || !room.BeingViewed || (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart) == null)
                            {
                                setRotation = new Vector2?(Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInChunk.Rotation)));
                                firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
                            }
                            else
                            {
                                setRotation = new Vector2?(Custom.DegToVec(stuckRotation + Custom.AimFromOneVectorToAnother(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos)));
                                firstChunk.MoveWithOtherObject(eu, stuckInChunk, Vector2.Lerp(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos, 0.5f) - stuckInChunk.pos);
                            }
                        }
                    }
                    else
                    {
                        if (pinToWallCounter > 0)
                        {
                            pinToWallCounter--;
                        }
                        if (stuckInChunk.vel.magnitude * stuckInChunk.mass > Custom.LerpMap(pinToWallCounter, 160f, 0f, 7f, 2f))
                        {
                            setRotation = new Vector2?((Custom.DegToVec(stuckRotation) + Vector2.ClampMagnitude(stuckInChunk.vel * stuckInChunk.mass * 0.005f, 0.1f)).normalized);
                        }
                        else
                        {
                            setRotation = new Vector2?(Custom.DegToVec(stuckRotation));
                        }
                        firstChunk.vel *= 0f;
                        firstChunk.pos = stuckInWall.Value;
                        if (stuckInChunk.owner is Creature && (stuckInChunk.owner as Creature).enteringShortCut != null || pinToWallCounter < 160 && Random.value < 0.025f && stuckInChunk.vel.magnitude > Custom.LerpMap(pinToWallCounter, 160f, 0f, 140f, 30f / (1f + stuckInChunk.owner.TotalMass * 0.2f)))
                        {
                            stuckRotation = Custom.Angle(setRotation.Value, stuckInChunk.Rotation);
                            stuckInWall = default;
                        }
                        else
                        {
                            stuckInChunk.MoveFromOutsideMyUpdate(eu, stuckInWall.Value);
                            stuckInChunk.vel *= 0f;
                        }
                    }
                    if (stuckInChunk.owner.slatedForDeletetion)
                    {
                        ChangeMode(Mode.Free);
                    }
                    break;
                }
            case Mode.StuckInWall:
                firstChunk.pos = stuckInWall.Value;
                firstChunk.vel *= 0f;
                break;
        }
        for (int l = abstractPhysicalObject.stuckObjects.Count - 1; l >= 0; l--)
        {
            if (abstractPhysicalObject.stuckObjects[l] is AbstractPhysicalObject.ImpaledOnSpearStick)
            {
                if (abstractPhysicalObject.stuckObjects[l].B.realizedObject != null && (abstractPhysicalObject.stuckObjects[l].B.realizedObject.slatedForDeletetion || abstractPhysicalObject.stuckObjects[l].B.realizedObject.grabbedBy.Count > 0))
                {
                    abstractPhysicalObject.stuckObjects[l].Deactivate();
                }
                else
                {
                    if (abstractPhysicalObject.stuckObjects[l].B.realizedObject != null && abstractPhysicalObject.stuckObjects[l].B.realizedObject.room == room)
                    {
                        abstractPhysicalObject.stuckObjects[l].B.realizedObject.firstChunk.MoveFromOutsideMyUpdate(eu, firstChunk.pos + rotation * Custom.LerpMap((abstractPhysicalObject.stuckObjects[l] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition, 0f, 4f, 15f, -15f));
                        abstractPhysicalObject.stuckObjects[l].B.realizedObject.firstChunk.vel *= 0f;
                    }
                }
            }
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        if (abstractSpear.stuckInWall)
        {
            stuckInWall = new Vector2?(placeRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile));
            ChangeMode(Mode.StuckInWall);
        }
    }

    public override void ChangeMode(Mode newMode)
    {
        if (mode == Mode.StuckInCreature)
        {
            if (room != null)
            {
                room.PlaySound(SoundID.Spear_Dislodged_From_Creature, firstChunk);
            }
            PulledOutOfStuckObject();
            ChangeOverlap(true);
        }
        else
        {
            if (newMode == Mode.StuckInCreature)
            {
                ChangeOverlap(false);
            }
        }
        if (newMode != Mode.Thrown)
        {
            spearDamageBonus = 1f;
        }
        if (newMode == Mode.StuckInWall)
        {
            if (abstractSpear.stuckInWallCycles == 0)
            {
                abstractSpear.stuckInWallCycles = Random.Range(3, 7) * (throwDir.y == 0 ? 1 : -1);
            }
            for (int i = -1; i < 2; i += 2)
            {
                if (abstractSpear.stuckInWallCycles >= 0 && !room.GetTile(stuckInWall.Value + new Vector2(20f * i, 0f)).Solid || abstractSpear.stuckInWallCycles < 0 && !room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * i)).Solid)
                {
                    setRotation = new Vector2?(abstractSpear.stuckInWallCycles < 0 ? new Vector2(0f, -i) : new Vector2(-i, 0f));
                    break;
                }
            }
            if (setRotation != null)
            {
                stuckInWall = new Vector2?(room.MiddleOfTile(stuckInWall.Value) - setRotation.Value * 5f);
            }
            rotationSpeed = 0f;
        }
        if (newMode > Mode.Free)
        {
            spinning = false;
        }
        if (newMode != Mode.StuckInWall && newMode != Mode.StuckInCreature)
        {
            stuckInWall = default;
        }
        base.ChangeMode(newMode);
    }

    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        room.PlaySound(SoundID.Slugcat_Throw_Spear, firstChunk);
        alwaysStickInWalls = false;
    }

    private new void LodgeInCreature(SharedPhysics.CollisionResult result, bool eu)
    {
        stuckInObject = result.obj;
        ChangeMode(Mode.StuckInCreature);
        if (result.chunk != null)
        {
            stuckInChunkIndex = result.chunk.index;
            if (spearDamageBonus > 0.9f && room.GetTile(room.GetTilePosition(stuckInChunk.pos) + throwDir).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(stuckInChunk.pos).Terrain == Room.Tile.TerrainType.Air)
            {
                stuckInWall = new Vector2?(room.MiddleOfTile(stuckInChunk.pos) + throwDir.ToVector2() * (10f - stuckInChunk.rad));
                stuckInChunk.MoveFromOutsideMyUpdate(eu, stuckInWall.Value);
                stuckRotation = Custom.VecToDeg(rotation);
                stuckBodyPart = -1;
                pinToWallCounter = 300;
            }
            else
            {
                if (stuckBodyPart == -1)
                {
                    stuckRotation = Custom.Angle(throwDir.ToVector2(), stuckInChunk.Rotation);
                }
            }
            firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
            Debug.Log("Add electric spear to creature chunk " + stuckInChunk.index);
            new AbstractPhysicalObject.AbstractSpearStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, stuckInChunkIndex, stuckBodyPart, stuckRotation);
        }
        else
        {
            if (result.onAppendagePos != null)
            {
                stuckInChunkIndex = 0;
                stuckInAppendage = result.onAppendagePos;
                stuckRotation = Custom.VecToDeg(rotation) - Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage));
                Debug.Log("Add electric spear to creature Appendage");
                new AbstractPhysicalObject.AbstractSpearAppendageStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, result.onAppendagePos.appendage.appIndex, result.onAppendagePos.prevSegment, result.onAppendagePos.distanceToNext, stuckRotation);
            }
        }
        if (room.BeingViewed)
        {
            for (int i = 0; i < 8; i++)
            {
                room.AddObject(new WaterDrip(result.collisionPoint, -firstChunk.vel * Random.value * 0.5f + Custom.DegToVec(360f * Random.value) * firstChunk.vel.magnitude * Random.value * 0.5f, false));
            }
        }
    }

    public new virtual void TryImpaleSmallCreature(Creature smallCrit)
    {
        int num = 0;
        int num2 = 0;
        for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
        {
            if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.ImpaledOnSpearStick)
            {
                if ((abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition == num2)
                {
                    num2++;
                }
                num++;
            }
        }
        if (!(num > 5 || num2 >= 5))
        {
            new AbstractPhysicalObject.ImpaledOnSpearStick(abstractPhysicalObject, smallCrit.abstractCreature, 0, num2);
        }
    }

    public override void SetRandomSpin()
    {
        if (room != null)
        {
            rotationSpeed = (Random.value >= 0.5f ? 1f : -1f) * Mathf.Lerp(50f, 150f, Random.value) * Mathf.Lerp(0.05f, 1f, room.gravity);
        }
        spinning = true;
    }

    public new void ProvideRotationBodyPart(BodyChunk chunk, BodyPart bodyPart)
    {
        stuckBodyPart = bodyPart.bodyPartArrayIndex;
        stuckRotation = Custom.Angle(firstChunk.vel, (bodyPart.pos - chunk.pos).normalized);
        bodyPart.vel += firstChunk.vel;
    }

    public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
    {
        base.HitSomethingWithoutStopping(obj, chunk, appendage);
        if (obj is Fly)
        {
            TryImpaleSmallCreature(obj as Creature);
        }
    }

    public new void PulledOutOfStuckObject()
    {
        for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
        {
            if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject)
            {
                abstractPhysicalObject.stuckObjects[i].Deactivate();
                break;
            }
            if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == abstractPhysicalObject)
            {
                abstractPhysicalObject.stuckObjects[i].Deactivate();
                break;
            }
        }
        stuckInObject = null;
        stuckInAppendage = null;
        stuckInChunkIndex = 0;
    }

    public override void RecreateSticksFromAbstract()
    {
        for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
        {
            if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).LodgedIn.realizedObject != null)
            {
                AbstractPhysicalObject.AbstractSpearStick abstractSpearStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick;
                stuckInObject = abstractSpearStick.LodgedIn.realizedObject;
                stuckInChunkIndex = abstractSpearStick.chunk;
                stuckBodyPart = abstractSpearStick.bodyPart;
                stuckRotation = abstractSpearStick.angle;
                ChangeMode(Mode.StuckInCreature);
            }
            else
            {
                if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).LodgedIn.realizedObject != null)
                {
                    AbstractPhysicalObject.AbstractSpearAppendageStick abstractSpearAppendageStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick;
                    stuckInObject = abstractSpearAppendageStick.LodgedIn.realizedObject;
                    stuckInAppendage = new Appendage.Pos(stuckInObject.appendages[abstractSpearAppendageStick.appendage], abstractSpearAppendageStick.prevSeg, abstractSpearAppendageStick.distanceToNext);
                    stuckRotation = abstractSpearAppendageStick.angle;
                    ChangeMode(Mode.StuckInCreature);
                }
            }
        }
    }
}
