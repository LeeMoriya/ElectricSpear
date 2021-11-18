﻿using System;
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
	private float conRad = 7f;
	private new int stuckBodyPart;
	private new bool spinning;
	protected new bool pivotAtTip;
	public new PhysicalObject.Appendage.Pos stuckInAppendage;
	public new float stuckRotation;
	public new Vector2? stuckInWall;
	public new bool alwaysStickInWalls;
	public new int pinToWallCounter;
	private new bool addPoles;
	public new float spearDamageBonus;
	private new int stillCounter;
	public LightSource lightSource;
	public float lightFlash;

	public ElectricSpear(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		this.bounce = 0.4f;
		this.surfaceFriction = 0.4f;
		this.collisionLayer = 2;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.4f;
		this.pivotAtTip = false;
		this.lastPivotAtTip = false;
		this.stuckBodyPart = -1;
		base.firstChunk.loudness = 7f;
		this.tailPos = base.firstChunk.pos;
		this.soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
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
			return this.abstractPhysicalObject as AbstractElectricSpear;
		}
	}

	public new BodyChunk stuckInChunk
	{
		get
		{
			return this.stuckInObject.bodyChunks[this.stuckInChunkIndex];
		}
	}

	public new float gravity
	{
		get
		{
			return this.g * this.room.gravity;
		}
		protected set
		{
			this.g = value;
		}
	}
	public int charge
	{
		get
		{
			return (this.abstractPhysicalObject as AbstractElectricSpear).charge;
		}
		set
		{
			(this.abstractPhysicalObject as AbstractElectricSpear).charge = value;
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
		this.AddToContainer(sLeaser, rCam, null);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[3].color = this.color;
		sLeaser.sprites[1].color = this.whiteColor;
		sLeaser.sprites[1].scale = 2f;
		sLeaser.sprites[2].color = this.whiteColor;
		sLeaser.sprites[2].scale = 2f;
		sLeaser.sprites[0].color = this.redColor;
		sLeaser.sprites[0].scale = 2f;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		bool flag = this.vibrate > 0;
		if (flag)
		{
			vector += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
		}
		Vector3 v = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
		for (int i = 3; i >= 0; i--)
		{
			sLeaser.sprites[i].x = vector.x - camPos.x;
			sLeaser.sprites[i].y = vector.y - camPos.y;
			sLeaser.sprites[i].anchorY = Mathf.Lerp((!this.lastPivotAtTip) ? 0.5f : 0.85f, (!this.pivotAtTip) ? 0.5f : 0.85f, timeStacker);
			sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), v);
		}
		sLeaser.sprites[1].anchorY += 10f;
		sLeaser.sprites[2].anchorY += 8f;
		sLeaser.sprites[0].anchorY += 6f;
		bool flag2 = this.blink > 0 && Random.value < 0.5f;
		if (flag2)
		{
			sLeaser.sprites[0].color = base.blinkColor;
			sLeaser.sprites[1].color = base.blinkColor;
			sLeaser.sprites[2].color = base.blinkColor;
			sLeaser.sprites[3].color = base.blinkColor;
		}
		else
		{
			switch (this.charge)
			{
				case 0:
					sLeaser.sprites[0].color = this.redColor;
					sLeaser.sprites[1].color = this.redColor;
					sLeaser.sprites[2].color = this.redColor;
					sLeaser.sprites[3].color = this.color;
					break;
				case 1:
					sLeaser.sprites[0].color = this.redColor;
					sLeaser.sprites[1].color = this.whiteColor;
					sLeaser.sprites[2].color = this.redColor;
					sLeaser.sprites[3].color = this.color;
					break;
				case 2:
					sLeaser.sprites[0].color = this.redColor;
					sLeaser.sprites[1].color = this.whiteColor;
					sLeaser.sprites[2].color = this.whiteColor;
					sLeaser.sprites[3].color = this.color;
					break;
				case 3:
					sLeaser.sprites[0].color = this.whiteColor;
					sLeaser.sprites[1].color = this.whiteColor;
					sLeaser.sprites[2].color = this.whiteColor;
					sLeaser.sprites[3].color = this.color;
					break;
			}
		}
		bool flag3 = base.slatedForDeletetion || this.room != rCam.room;
		if (flag3)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		bool flag = result.obj == null;
		bool result2;
		if (flag)
		{
			result2 = false;
		}
		else
		{
			bool flag2 = false;
			bool flag3 = this.abstractPhysicalObject.world.game.IsArenaSession && this.abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0 && this.thrownBy != null && this.thrownBy is Player && result.obj is Creature;
			if (flag3)
			{
				flag2 = true;
				bool flag4 = (result.obj as Creature).State is HealthState && ((result.obj as Creature).State as HealthState).health <= 0f;
				if (flag4)
				{
					flag2 = false;
				}
				else
				{
					bool flag5 = !((result.obj as Creature).State is HealthState) && (result.obj as Creature).State.dead;
					if (flag5)
					{
						flag2 = false;
					}
				}
			}
			bool flag6 = result.obj is Creature;
			if (flag6)
			{
				bool flag7 = result.obj is Centipede;
				if (flag7)
				{
					bool flag8 = this.charge != 3;
					if (flag8)
					{
						(result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, this.spearDamageBonus, 20f);
						this.charged = true;
						this.depleted = false;
					}
					else
					{
						(result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, this.spearDamageBonus, 20f);
						this.charged = false;
						this.depleted = false;
					}
				}
				else
				{
					bool flag9 = this.charge != 0;
					if (flag9)
					{
						(result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, this.spearDamageBonus, 20f);
						(result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Electric, 6f, 20f);
						this.charged = false;
						this.depleted = true;
					}
					else
					{
						(result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass * 2f), result.chunk, result.onAppendagePos, Creature.DamageType.Stab, this.spearDamageBonus, 20f);
						this.charged = false;
						this.depleted = false;
					}
				}
			}
			else
			{
				bool flag10 = result.chunk != null;
				if (flag10)
				{
					result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
				}
				else
				{
					bool flag11 = result.onAppendagePos != null;
					if (flag11)
					{
						(result.obj as PhysicalObject.IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
					}
				}
			}
			bool flag12 = result.obj is Creature && (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, Random.value), result.chunk, result.onAppendagePos, base.firstChunk.vel);
			if (flag12)
			{
				bool flag13 = this.depleted;
				if (flag13)
				{
					this.room.PlaySound(SoundID.Centipede_Shock, base.firstChunk);
					int charge = this.charge;
					this.charge = charge - 1;
				}
				bool flag14 = this.charged;
				if (flag14)
				{
					this.room.PlaySound(SoundID.Centipede_Electric_Charge_LOOP, base.firstChunk);
					int charge = this.charge;
					this.charge = charge + 1;
					this.charged = false;
				}
				this.room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk);
				this.LodgeInCreature(result, eu);
				this.lightFlash = 1.3f;
				bool flag15 = flag2;
				if (flag15)
				{
					this.abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(this.thrownBy as Player, this.stuckInObject as Creature);
				}
				result2 = true;
			}
			else
			{
				this.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, base.firstChunk);
				this.vibrate = 20;
				this.ChangeMode(Weapon.Mode.Free);
				base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * base.firstChunk.vel.magnitude;
				this.SetRandomSpin();
				result2 = false;
			}
		}
		return result2;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		this.soundLoop.sound = SoundID.None;
		bool flag = base.firstChunk.vel.magnitude > 5f;
		if (flag)
		{
			bool flag2 = base.mode == Weapon.Mode.Thrown;
			if (flag2)
			{
				this.soundLoop.sound = SoundID.Spear_Thrown_Through_Air_LOOP;
			}
			else
			{
				bool flag3 = base.mode == Weapon.Mode.Free;
				if (flag3)
				{
					this.soundLoop.sound = SoundID.Spear_Spinning_Through_Air_LOOP;
				}
			}
			this.soundLoop.Volume = Mathf.InverseLerp(5f, 15f, base.firstChunk.vel.magnitude);
		}
		this.soundLoop.Update();
		this.lastPivotAtTip = this.pivotAtTip;
		this.pivotAtTip = (base.mode == Weapon.Mode.Thrown || base.mode == Weapon.Mode.StuckInCreature);
		bool flag4 = this.addPoles && this.room.readyForAI;
		if (flag4)
		{
			bool flag5 = base.abstractSpear.stuckInWallCycles >= 0;
			if (flag5)
			{
				this.room.GetTile(this.stuckInWall.Value).horizontalBeam = true;
				for (int i = -1; i < 2; i += 2)
				{
					bool flag6 = !this.room.GetTile(this.stuckInWall.Value + new Vector2(20f * (float)i, 0f)).Solid;
					if (flag6)
					{
						this.room.GetTile(this.stuckInWall.Value + new Vector2(20f * (float)i, 0f)).horizontalBeam = true;
					}
				}
			}
			else
			{
				this.room.GetTile(this.stuckInWall.Value).verticalBeam = true;
				for (int j = -1; j < 2; j += 2)
				{
					bool flag7 = !this.room.GetTile(this.stuckInWall.Value + new Vector2(0f, 20f * (float)j)).Solid;
					if (flag7)
					{
						this.room.GetTile(this.stuckInWall.Value + new Vector2(0f, 20f * (float)j)).verticalBeam = true;
					}
				}
			}
			this.addPoles = false;
		}
		switch (base.mode)
		{
			case Weapon.Mode.Free:
				{
					bool flag8 = this.spinning;
					if (flag8)
					{
						bool flag9 = Custom.DistLess(base.firstChunk.pos, base.firstChunk.lastPos, 4f * this.room.gravity);
						if (flag9)
						{
							this.stillCounter++;
						}
						else
						{
							this.stillCounter = 0;
						}
						bool flag10 = base.firstChunk.ContactPoint.y < 0 || this.stillCounter > 20;
						if (flag10)
						{
							this.spinning = false;
							this.rotationSpeed = 0f;
							this.rotation = Custom.DegToVec(Mathf.Lerp(-50f, 50f, Random.value) + 180f);
							base.firstChunk.vel *= 0f;
							this.room.PlaySound(SoundID.Spear_Stick_In_Ground, base.firstChunk);
						}
					}
					else
					{
						bool flag11 = !Custom.DistLess(base.firstChunk.lastPos, base.firstChunk.pos, 6f);
						if (flag11)
						{
							this.SetRandomSpin();
						}
					}
					break;
				}
			case Weapon.Mode.Thrown:
				{
					bool flag12 = Custom.DistLess(this.thrownPos, base.firstChunk.pos, 560f * Mathf.Max(1f, this.spearDamageBonus)) && base.firstChunk.ContactPoint == this.throwDir && this.room.GetTile(base.firstChunk.pos).Terrain == Room.Tile.TerrainType.Air && this.room.GetTile(base.firstChunk.pos + this.throwDir.ToVector2() * 20f).Terrain == Room.Tile.TerrainType.Solid && (Random.value < 0.33f || Custom.DistLess(this.thrownPos, base.firstChunk.pos, 140f) || this.alwaysStickInWalls);
					if (flag12)
					{
						bool flag13 = true;
						foreach (AbstractWorldEntity abstractWorldEntity in this.room.abstractRoom.entities)
						{
							bool flag14 = abstractWorldEntity is AbstractSpear && (abstractWorldEntity as AbstractSpear).realizedObject != null && ((abstractWorldEntity as AbstractSpear).realizedObject as Weapon).mode == Weapon.Mode.StuckInWall && abstractWorldEntity.pos.Tile == this.abstractPhysicalObject.pos.Tile;
							if (flag14)
							{
								flag13 = false;
								break;
							}
						}
						bool flag15 = flag13 && 0 == 0;
						if (flag15)
						{
							for (int k = 0; k < this.room.roomSettings.placedObjects.Count; k++)
							{
								bool flag16 = this.room.roomSettings.placedObjects[k].type == PlacedObject.Type.NoSpearStickZone && Custom.DistLess(this.room.MiddleOfTile(base.firstChunk.pos), this.room.roomSettings.placedObjects[k].pos, (this.room.roomSettings.placedObjects[k].data as PlacedObject.ResizableObjectData).Rad);
								if (flag16)
								{
									flag13 = false;
									break;
								}
							}
						}
						bool flag17 = flag13;
						if (flag17)
						{
							this.stuckInWall = new Vector2?(this.room.MiddleOfTile(base.firstChunk.pos));
							this.vibrate = 10;
							this.ChangeMode(Weapon.Mode.StuckInWall);
							this.room.PlaySound(SoundID.Spear_Stick_In_Wall, base.firstChunk);
							base.firstChunk.collideWithTerrain = false;
						}
					}
					break;
				}
			case Weapon.Mode.StuckInCreature:
				{
					bool flag18 = this.stuckInWall == null;
					if (flag18)
					{
						bool flag19 = this.stuckInAppendage != null;
						if (flag19)
						{
							this.setRotation = new Vector2?(Custom.DegToVec(this.stuckRotation + Custom.VecToDeg(this.stuckInAppendage.appendage.OnAppendageDirection(this.stuckInAppendage))));
							base.firstChunk.pos = this.stuckInAppendage.appendage.OnAppendagePosition(this.stuckInAppendage);
						}
						else
						{
							bool flag20 = this.depleted;
							if (flag20)
							{
								bool flag21 = this.lightSource != null;
								if (flag21)
								{
									this.lightSource.stayAlive = true;
									this.lightSource.setPos = new Vector2?(base.firstChunk.pos);
									this.lightSource.setRad = new float?(300f * Mathf.Pow(this.lightFlash * Random.value, 0.01f) * Mathf.Lerp(0.5f, 2f, 0.8f) - 1.3f);
									this.lightSource.setAlpha = new float?(Mathf.Pow(this.lightFlash * Random.value, 0.01f) - 0.8f);
									float num = this.lightFlash * Random.value;
									num = Mathf.Lerp(num, 1f, 0.5f * (1f - this.room.Darkness(base.firstChunk.pos)));
									this.lightSource.color = new Color(num, num, 1.5f);
									bool flag22 = this.lightFlash <= 0f;
									if (flag22)
									{
										this.lightSource.Destroy();
									}
									bool slatedForDeletetion = this.lightSource.slatedForDeletetion;
									if (slatedForDeletetion)
									{
										bool flag23 = this.depleted;
										if (flag23)
										{
											this.depleted = false;
										}
										this.lightSource = null;
									}
								}
								else
								{
									bool flag24 = this.lightFlash > 0f;
									if (flag24)
									{
										this.lightSource = new LightSource(base.firstChunk.pos, false, new Color(1f, 1f, 1f), this);
										this.lightSource.affectedByPaletteDarkness = 0f;
										this.lightSource.requireUpKeep = true;
										this.room.AddObject(this.lightSource);
									}
								}
								bool flag25 = this.lightFlash > 0f;
								if (flag25)
								{
									this.lightFlash = Mathf.Max(0f, this.lightFlash - 0.0333933346f);
								}
							}
							base.firstChunk.vel = this.stuckInChunk.vel;
							bool flag26 = this.stuckBodyPart == -1 || !this.room.BeingViewed || (this.stuckInChunk.owner as Creature).BodyPartByIndex(this.stuckBodyPart) == null;
							if (flag26)
							{
								this.setRotation = new Vector2?(Custom.DegToVec(this.stuckRotation + Custom.VecToDeg(this.stuckInChunk.Rotation)));
								base.firstChunk.MoveWithOtherObject(eu, this.stuckInChunk, new Vector2(0f, 0f));
							}
							else
							{
								this.setRotation = new Vector2?(Custom.DegToVec(this.stuckRotation + Custom.AimFromOneVectorToAnother(this.stuckInChunk.pos, (this.stuckInChunk.owner as Creature).BodyPartByIndex(this.stuckBodyPart).pos)));
								base.firstChunk.MoveWithOtherObject(eu, this.stuckInChunk, Vector2.Lerp(this.stuckInChunk.pos, (this.stuckInChunk.owner as Creature).BodyPartByIndex(this.stuckBodyPart).pos, 0.5f) - this.stuckInChunk.pos);
							}
						}
					}
					else
					{
						bool flag27 = this.pinToWallCounter > 0;
						if (flag27)
						{
							this.pinToWallCounter--;
						}
						bool flag28 = this.stuckInChunk.vel.magnitude * this.stuckInChunk.mass > Custom.LerpMap((float)this.pinToWallCounter, 160f, 0f, 7f, 2f);
						if (flag28)
						{
							this.setRotation = new Vector2?((Custom.DegToVec(this.stuckRotation) + Vector2.ClampMagnitude(this.stuckInChunk.vel * this.stuckInChunk.mass * 0.005f, 0.1f)).normalized);
						}
						else
						{
							this.setRotation = new Vector2?(Custom.DegToVec(this.stuckRotation));
						}
						base.firstChunk.vel *= 0f;
						base.firstChunk.pos = this.stuckInWall.Value;
						bool flag29 = (this.stuckInChunk.owner is Creature && (this.stuckInChunk.owner as Creature).enteringShortCut != null) || (this.pinToWallCounter < 160 && Random.value < 0.025f && this.stuckInChunk.vel.magnitude > Custom.LerpMap((float)this.pinToWallCounter, 160f, 0f, 140f, 30f / (1f + this.stuckInChunk.owner.TotalMass * 0.2f)));
						if (flag29)
						{
							this.stuckRotation = Custom.Angle(this.setRotation.Value, this.stuckInChunk.Rotation);
							this.stuckInWall = default(Vector2?);
						}
						else
						{
							this.stuckInChunk.MoveFromOutsideMyUpdate(eu, this.stuckInWall.Value);
							this.stuckInChunk.vel *= 0f;
						}
					}
					bool slatedForDeletetion2 = this.stuckInChunk.owner.slatedForDeletetion;
					if (slatedForDeletetion2)
					{
						this.ChangeMode(Weapon.Mode.Free);
					}
					break;
				}
			case Weapon.Mode.StuckInWall:
				base.firstChunk.pos = this.stuckInWall.Value;
				base.firstChunk.vel *= 0f;
				break;
		}
		for (int l = this.abstractPhysicalObject.stuckObjects.Count - 1; l >= 0; l--)
		{
			bool flag30 = this.abstractPhysicalObject.stuckObjects[l] is AbstractPhysicalObject.ImpaledOnSpearStick;
			if (flag30)
			{
				bool flag31 = this.abstractPhysicalObject.stuckObjects[l].B.realizedObject != null && (this.abstractPhysicalObject.stuckObjects[l].B.realizedObject.slatedForDeletetion || this.abstractPhysicalObject.stuckObjects[l].B.realizedObject.grabbedBy.Count > 0);
				if (flag31)
				{
					this.abstractPhysicalObject.stuckObjects[l].Deactivate();
				}
				else
				{
					bool flag32 = this.abstractPhysicalObject.stuckObjects[l].B.realizedObject != null && this.abstractPhysicalObject.stuckObjects[l].B.realizedObject.room == this.room;
					if (flag32)
					{
						this.abstractPhysicalObject.stuckObjects[l].B.realizedObject.firstChunk.MoveFromOutsideMyUpdate(eu, base.firstChunk.pos + this.rotation * Custom.LerpMap((float)(this.abstractPhysicalObject.stuckObjects[l] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition, 0f, 4f, 15f, -15f));
						this.abstractPhysicalObject.stuckObjects[l].B.realizedObject.firstChunk.vel *= 0f;
					}
				}
			}
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		bool flag = base.abstractSpear.stuckInWall;
		if (flag)
		{
			this.stuckInWall = new Vector2?(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos.Tile));
			this.ChangeMode(Weapon.Mode.StuckInWall);
		}
	}

	public override void ChangeMode(Weapon.Mode newMode)
	{
		bool flag = base.mode == Weapon.Mode.StuckInCreature;
		if (flag)
		{
			bool flag2 = this.room != null;
			if (flag2)
			{
				this.room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk);
			}
			this.PulledOutOfStuckObject();
			base.ChangeOverlap(true);
		}
		else
		{
			bool flag3 = newMode == Weapon.Mode.StuckInCreature;
			if (flag3)
			{
				base.ChangeOverlap(false);
			}
		}
		bool flag4 = newMode != Weapon.Mode.Thrown;
		if (flag4)
		{
			this.spearDamageBonus = 1f;
		}
		bool flag5 = newMode == Weapon.Mode.StuckInWall;
		if (flag5)
		{
			bool flag6 = base.abstractSpear.stuckInWallCycles == 0;
			if (flag6)
			{
				base.abstractSpear.stuckInWallCycles = Random.Range(3, 7) * ((this.throwDir.y == 0) ? 1 : -1);
			}
			for (int i = -1; i < 2; i += 2)
			{
				bool flag7 = (base.abstractSpear.stuckInWallCycles >= 0 && !this.room.GetTile(this.stuckInWall.Value + new Vector2(20f * (float)i, 0f)).Solid) || (base.abstractSpear.stuckInWallCycles < 0 && !this.room.GetTile(this.stuckInWall.Value + new Vector2(0f, 20f * (float)i)).Solid);
				if (flag7)
				{
					this.setRotation = new Vector2?((base.abstractSpear.stuckInWallCycles < 0) ? new Vector2(0f, -(float)i) : new Vector2(-(float)i, 0f));
					break;
				}
			}
			bool flag8 = this.setRotation != null;
			if (flag8)
			{
				this.stuckInWall = new Vector2?(this.room.MiddleOfTile(this.stuckInWall.Value) - this.setRotation.Value * 5f);
			}
			this.rotationSpeed = 0f;
		}
		bool flag9 = newMode > Weapon.Mode.Free;
		if (flag9)
		{
			this.spinning = false;
		}
		bool flag10 = newMode != Weapon.Mode.StuckInWall && newMode != Weapon.Mode.StuckInCreature;
		if (flag10)
		{
			this.stuckInWall = default(Vector2?);
		}
		base.ChangeMode(newMode);
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		this.room.PlaySound(SoundID.Slugcat_Throw_Spear, base.firstChunk);
		this.alwaysStickInWalls = false;
	}

	private new void LodgeInCreature(SharedPhysics.CollisionResult result, bool eu)
	{
		this.stuckInObject = result.obj;
		this.ChangeMode(Weapon.Mode.StuckInCreature);
		bool flag = result.chunk != null;
		if (flag)
		{
			this.stuckInChunkIndex = result.chunk.index;
			bool flag2 = this.spearDamageBonus > 0.9f && this.room.GetTile(this.room.GetTilePosition(this.stuckInChunk.pos) + this.throwDir).Terrain == Room.Tile.TerrainType.Solid && this.room.GetTile(this.stuckInChunk.pos).Terrain == Room.Tile.TerrainType.Air;
			if (flag2)
			{
				this.stuckInWall = new Vector2?(this.room.MiddleOfTile(this.stuckInChunk.pos) + this.throwDir.ToVector2() * (10f - this.stuckInChunk.rad));
				this.stuckInChunk.MoveFromOutsideMyUpdate(eu, this.stuckInWall.Value);
				this.stuckRotation = Custom.VecToDeg(this.rotation);
				this.stuckBodyPart = -1;
				this.pinToWallCounter = 300;
			}
			else
			{
				bool flag3 = this.stuckBodyPart == -1;
				if (flag3)
				{
					this.stuckRotation = Custom.Angle(this.throwDir.ToVector2(), this.stuckInChunk.Rotation);
				}
			}
			base.firstChunk.MoveWithOtherObject(eu, this.stuckInChunk, new Vector2(0f, 0f));
			Debug.Log("Add spear to creature chunk " + this.stuckInChunk.index);
			new AbstractPhysicalObject.AbstractSpearStick(this.abstractPhysicalObject, (result.obj as Creature).abstractCreature, this.stuckInChunkIndex, this.stuckBodyPart, this.stuckRotation);
		}
		else
		{
			bool flag4 = result.onAppendagePos != null;
			if (flag4)
			{
				this.stuckInChunkIndex = 0;
				this.stuckInAppendage = result.onAppendagePos;
				this.stuckRotation = Custom.VecToDeg(this.rotation) - Custom.VecToDeg(this.stuckInAppendage.appendage.OnAppendageDirection(this.stuckInAppendage));
				Debug.Log("Add spear to creature Appendage");
				new AbstractPhysicalObject.AbstractSpearAppendageStick(this.abstractPhysicalObject, (result.obj as Creature).abstractCreature, result.onAppendagePos.appendage.appIndex, result.onAppendagePos.prevSegment, result.onAppendagePos.distanceToNext, this.stuckRotation);
			}
		}
		bool beingViewed = this.room.BeingViewed;
		if (beingViewed)
		{
			for (int i = 0; i < 8; i++)
			{
				this.room.AddObject(new WaterDrip(result.collisionPoint, -base.firstChunk.vel * Random.value * 0.5f + Custom.DegToVec(360f * Random.value) * base.firstChunk.vel.magnitude * Random.value * 0.5f, false));
			}
		}
	}

	public new virtual void TryImpaleSmallCreature(Creature smallCrit)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < this.abstractPhysicalObject.stuckObjects.Count; i++)
		{
			bool flag = this.abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.ImpaledOnSpearStick;
			if (flag)
			{
				bool flag2 = (this.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition == num2;
				if (flag2)
				{
					num2++;
				}
				num++;
			}
		}
		bool flag3 = num > 5 || num2 >= 5;
		if (!flag3)
		{
			new AbstractPhysicalObject.ImpaledOnSpearStick(this.abstractPhysicalObject, smallCrit.abstractCreature, 0, num2);
		}
	}

	public override void SetRandomSpin()
	{
		bool flag = this.room != null;
		if (flag)
		{
			this.rotationSpeed = ((Random.value >= 0.5f) ? 1f : -1f) * Mathf.Lerp(50f, 150f, Random.value) * Mathf.Lerp(0.05f, 1f, this.room.gravity);
		}
		this.spinning = true;
	}

	public new void ProvideRotationBodyPart(BodyChunk chunk, BodyPart bodyPart)
	{
		this.stuckBodyPart = bodyPart.bodyPartArrayIndex;
		this.stuckRotation = Custom.Angle(base.firstChunk.vel, (bodyPart.pos - chunk.pos).normalized);
		bodyPart.vel += base.firstChunk.vel;
	}

	public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, PhysicalObject.Appendage appendage)
	{
		base.HitSomethingWithoutStopping(obj, chunk, appendage);
		bool flag = obj is Fly;
		if (flag)
		{
			this.TryImpaleSmallCreature(obj as Creature);
		}
	}

	public new void PulledOutOfStuckObject()
	{
		for (int i = 0; i < this.abstractPhysicalObject.stuckObjects.Count; i++)
		{
			bool flag = this.abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (this.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == this.abstractPhysicalObject;
			if (flag)
			{
				this.abstractPhysicalObject.stuckObjects[i].Deactivate();
				break;
			}
			bool flag2 = this.abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (this.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == this.abstractPhysicalObject;
			if (flag2)
			{
				this.abstractPhysicalObject.stuckObjects[i].Deactivate();
				break;
			}
		}
		this.stuckInObject = null;
		this.stuckInAppendage = null;
		this.stuckInChunkIndex = 0;
	}

	public override void RecreateSticksFromAbstract()
	{
		for (int i = 0; i < this.abstractPhysicalObject.stuckObjects.Count; i++)
		{
			bool flag = this.abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (this.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == this.abstractPhysicalObject && (this.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).LodgedIn.realizedObject != null;
			if (flag)
			{
				AbstractPhysicalObject.AbstractSpearStick abstractSpearStick = this.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick;
				this.stuckInObject = abstractSpearStick.LodgedIn.realizedObject;
				this.stuckInChunkIndex = abstractSpearStick.chunk;
				this.stuckBodyPart = abstractSpearStick.bodyPart;
				this.stuckRotation = abstractSpearStick.angle;
				this.ChangeMode(Weapon.Mode.StuckInCreature);
			}
			else
			{
				bool flag2 = this.abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (this.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == this.abstractPhysicalObject && (this.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).LodgedIn.realizedObject != null;
				if (flag2)
				{
					AbstractPhysicalObject.AbstractSpearAppendageStick abstractSpearAppendageStick = this.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick;
					this.stuckInObject = abstractSpearAppendageStick.LodgedIn.realizedObject;
					this.stuckInAppendage = new PhysicalObject.Appendage.Pos(this.stuckInObject.appendages[abstractSpearAppendageStick.appendage], abstractSpearAppendageStick.prevSeg, abstractSpearAppendageStick.distanceToNext);
					this.stuckRotation = abstractSpearAppendageStick.angle;
					this.ChangeMode(Weapon.Mode.StuckInCreature);
				}
			}
		}
	}
}