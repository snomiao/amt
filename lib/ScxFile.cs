﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace YTY
{
  public class ScxFile
  {
    public string FileName { get; set; }
    public byte[] Version { get; set; }
    public int FormatVersion { get; set; }
    public int LastSave { get; set; }
    public byte[] Instruction { get; set; }
    public int PlayerCount { get; set; }
    public int NextUid { get; set; }
    public float Version2 { get; set; }
    public List<Player> Players { get; set; }
    public byte[] OriginalFileName { get; set; }
    public List<int> StringTableInfos { get; set; }
    public List<byte[]> StringInfos { get; set; }
    public int HasBitmap { get; set; }
    public int BitmapX { get; set; }
    public int BitmapY { get; set; }
    public BITMAPDIB Bitmap { get; set; }
    public long Conquest { get; set; }
    public long Relics { get; set; }
    public long Explored { get; set; }
    public int AllMustMeet { get; set; }
    public int Mode { get; set; }
    public int Score { get; set; }
    public int Time { get; set; }
    public byte LockTeams { get; set; }
    public byte PlayerChooseTeams { get; set; }
    public byte RandomStartPoints { get; set; }
    public byte MaxTeams { get; set; }
    public int AllTechs { get; set; }
    public int CameraX { get; set; }
    public int CameraY { get; set; }
    public MapType MapType { get; set; }
    public int MapX { get; set; }
    public int MapY { get; set; }
    public Terrain[,] Map { get; set; }
    public List<Resource> Resources { get; }
    public List<List<Unit>> Units { get; }
    public List<PlayerMisc> PlayerMiscs { get; }
    public List<Trigger> Triggers { get; }
    public List<int> TriggersOrder { get; }
    public int HasAiFile { get; set; }
    public Dictionary<byte[], byte[]> AiFiles { get; }

    private List<int> unknownInt32s;

    private ScxFile()
    {
      Players = new List<Player>(16);
      StringTableInfos = new List<int>(6);
      Resources = new List<Resource>(8);
      Units = Enumerable.Range(0, 8).Select(i => new List<Unit>()).ToList();
      PlayerMiscs = new List<PlayerMisc>(8);
    }

    public ScxFile(string fileName) : this()
    {
      FileName = fileName;
    }

    public ScxFile(Stream stream) : this()
    {
      using (var br = new BinaryReader(stream))
      {
        Version = br.ReadBytes(4);
        br.ReadBytes(4);
        FormatVersion = br.ReadInt32();
        LastSave = br.ReadInt32();
        Instruction = br.ReadBytes(br.ReadInt32());
        br.ReadBytes(4);
        PlayerCount = br.ReadInt32();
        if (FormatVersion == 3)
        {
          br.ReadBytes(8);
          var unknownCount = br.ReadInt32();
          unknownInt32s = new List<int>(unknownCount);
          for (var i = 0; i < unknownCount; i++)
            unknownInt32s.Add(br.ReadInt32());
        }
        using (var ds = new DeflateStream(stream, CompressionMode.Decompress))
        {
          using (var dr = new BinaryReader(ds))
          {
            NextUid = dr.ReadInt32();
            Version2 = dr.ReadSingle();
            for (var i = 0; i < 16; i++)
            {
              Players.Add(new Player()
              {
                Name = dr.ReadBytes(256)
              });
            }
            for (var i = 0; i < 16; i++)
            {
              Players[i].Name_StringTable = dr.ReadInt32();
            }
            for (var i = 0; i < 16; i++)
            {
              Players[i].IsActive = dr.ReadInt32();
              Players[i].IsHuman = dr.ReadInt32();
              Players[i].Civilization = dr.ReadInt32();
              dr.ReadInt32();
            }
            dr.ReadBytes(9);
            OriginalFileName = dr.ReadBytes(dr.ReadInt16());
            for (var i = 0; i < 5; i++)
            {
              StringTableInfos.Add(dr.ReadInt32());
            }
            if (GetVersion() >= ScxVersion.Version122)
              StringTableInfos.Add(dr.ReadInt32());
            StringInfos = new List<byte[]>(10);
            for (var i = 0; i < 9; i++)
            {
              StringInfos.Add(dr.ReadBytes(dr.ReadInt16()));
            }
            if (GetVersion() >= ScxVersion.Version122)
              StringInfos.Add(dr.ReadBytes(dr.ReadInt16()));
            HasBitmap = dr.ReadInt32();
            BitmapX = dr.ReadInt32();
            BitmapY = dr.ReadInt32();
            dr.ReadBytes(2);
            if (BitmapX > 0 && BitmapY > 0)
            {
              Bitmap = new BITMAPDIB()
              {
                Size = dr.ReadInt32(),
                Width = dr.ReadInt32(),
                Height = dr.ReadInt32(),
                Planes = dr.ReadInt32(),
                BitCount = dr.ReadInt32(),
                Compression = dr.ReadInt32(),
                SizeImage = dr.ReadInt32(),
                XPelsPerMeter = dr.ReadInt32(),
                YPelsPerMeter = dr.ReadInt32(),
                ClrUsed = dr.ReadInt32(),
                ClrImportant = dr.ReadInt32(),
                Colors = new List<RGB>(256)
              };
              for (var i = 0; i < 256; i++)
              {
                Bitmap.Colors.Add(new RGB()
                {
                  Red = dr.ReadByte(),
                  Green = dr.ReadByte(),
                  Blue = dr.ReadByte(),
                });
                dr.ReadByte();
              }
              Bitmap.ImageData = dr.ReadBytes(((BitmapX - 1) / 4 + 1) * 4 * BitmapY);
            }
            for (var i = 0; i < 32; i++)
            {
              dr.ReadBytes(dr.ReadInt16());
            }
            for (var i = 0; i < 16; i++)
            {
              Players[i].Ai = dr.ReadBytes(dr.ReadInt16());
            }
            for (var i = 0; i < 16; i++)
            {
              dr.ReadBytes(8);
              Players[i].AiFile = dr.ReadBytes(dr.ReadInt32());
            }
            for (var i = 0; i < 16; i++)
            {
              Players[i].Personality = dr.ReadByte();
            }
            for (var i = 0; i < 16; i++)
            {
              Players[i].Gold = dr.ReadInt32();
              Players[i].Wood = dr.ReadInt32();
              Players[i].Food = dr.ReadInt32();
              Players[i].Stone = dr.ReadInt32();
              Players[i].Ore = dr.ReadInt32();
              dr.ReadInt32();
              if (GetVersion() >= ScxVersion.Version124)
                Players[i].PlayerNumber = dr.ReadInt32();
            }
            dr.ReadInt32();
            Conquest = dr.ReadInt64();
            Relics = dr.ReadInt64();
            Explored = dr.ReadInt64();
            AllMustMeet = dr.ReadInt32();
            Mode = dr.ReadInt32();
            Score = dr.ReadInt32();
            Time = dr.ReadInt32();
            for (var i = 0; i < 16; i++)
              for (var j = 0; j < 16; i++)
                Players[i].Diplomacies.Add((DiplomacyInt)dr.ReadInt32());
            dr.ReadBytes(11524);
            for (var i = 0; i < 16; i++)
              Players[i].AlliedVictory = dr.ReadInt32();
            if (GetVersion() >= ScxVersion.Version123)
            {
              LockTeams = dr.ReadByte();
              PlayerChooseTeams = dr.ReadByte();
              RandomStartPoints = dr.ReadByte();
              MaxTeams = dr.ReadByte();
            }
            for (var i = 0; i < 16; i++)
              dr.ReadInt32();
            for (var i = 0; i < 16; i++)
              for (var j = 0; j < 30; j++)
                Players[i].DisabledTechs.Add(dr.ReadInt32());
            for (var i = 0; i < 16; i++)
              dr.ReadInt32();
            for (var i = 0; i < 16; i++)
              for (var j = 0; j < 30; j++)
                Players[i].DisabledUnits.Add(dr.ReadInt32());
            for (var i = 0; i < 16; i++)
              dr.ReadInt32();
            for (var i = 0; i < 16; i++)
            {
              var countJ = GetVersion() >= ScxVersion.Version126 ? 30 : 20;
              for (var j = 0; j < countJ; j++)
                Players[i].DisabledBuildings.Add(dr.ReadInt32());
            }
            dr.ReadBytes(8);
            AllTechs = dr.ReadInt32();
            for (var i = 0; i < 16; i++)
            {
              Players[i].StartAge = (StartAge)dr.ReadInt32();
              if (GetVersion() >= ScxVersion.Version126)
                Players[i].StartAge -= 2;
            }
            dr.ReadInt32();
            CameraX = dr.ReadInt32();
            CameraY = dr.ReadInt32();
            if (GetVersion() >= ScxVersion.Version122)
              MapType = (MapType)dr.ReadInt32();
            if (GetVersion() >= ScxVersion.Version124)
              dr.ReadBytes(16);
            MapX = dr.ReadInt32();
            MapY = dr.ReadInt32();
            Map = new Terrain[MapX, MapY];
            for (var i = 0; i < MapX; i++)
            {
              for (var j = 0; j < MapY; j++)
              {
                Map[i, j].Id = dr.ReadByte();
                Map[i, j].Elevation = dr.ReadInt16();
              }
            }
            dr.ReadBytes(4);
            for (var i = 0; i < 8; i++)
            {
              Resources.Add(new Resource()
              {
                Food = dr.ReadSingle(),
                Wood = dr.ReadSingle(),
                Gold = dr.ReadSingle(),
                Stone = dr.ReadSingle(),
                Ore = dr.ReadSingle(),
                PopulationLimit = dr.ReadSingle()
              });
            }
            for (var i = 0; i < 8; i++)
            {
              var unitsCount = dr.ReadInt32();
              for (var j = 0; j < unitsCount; j++)
              {
                Units[i].Add(new Unit()
                {
                  PosX = dr.ReadSingle(),
                  PosY = dr.ReadSingle(),
                  PosZ = dr.ReadSingle(),
                  Id = dr.ReadInt32(),
                  UnitClass = dr.ReadInt16(),
                  State = dr.ReadByte(),
                  Rotation = dr.ReadSingle(),
                  Frame = dr.ReadInt16(),
                  Garrison = dr.ReadInt32()
                });
              }
            }
            dr.ReadInt32();
            for (var i = 0; i < 8; i++)
            {
              PlayerMiscs[i].Name = dr.ReadBytes(dr.ReadInt16());
              PlayerMiscs[i].CameraX = dr.ReadSingle();
              PlayerMiscs[i].CameraY = dr.ReadSingle();
              dr.ReadInt32();
              PlayerMiscs[i].AlliedVictory = dr.ReadByte();
              dr.ReadBytes(2);
              for (var j = 0; j < 9; j++)
                PlayerMiscs[i].Diplomacies.Add((DiplomacyByte)dr.ReadByte());
              for (var j = 0; j < 9; j++)
                PlayerMiscs[i].Diplomacies2.Add((Diplomacy2)dr.ReadInt32());
              PlayerMiscs[i].Color = (PlayerColor)dr.ReadInt32();
              dr.ReadBytes((dr.ReadSingle() == 2.0f ? 8 : 0) + dr.ReadInt16() * 44 + 11);
            }
            var someDouble = dr.ReadDouble();
            if (someDouble == 1.6d)
              dr.ReadByte();
            var numTriggers = dr.ReadInt32();
            Triggers = new List<Trigger>(numTriggers);
            for (var i = 0; i < numTriggers; i++)
            {
              Triggers.Add(new Trigger()
              {
                IsEnabled = dr.ReadInt32(),
                IsLooping = dr.ReadInt32()
              });
              dr.ReadByte();
              Triggers[i].IsObjective = dr.ReadByte();
              Triggers[i].DiscriptionOrder = dr.ReadInt32();
              if (someDouble == 1.6d)
                dr.ReadInt32();
              Triggers[i].Discription = dr.ReadBytes(dr.ReadInt32());
              Triggers[i].Name = dr.ReadBytes(dr.ReadInt32());
              var numEffects = dr.ReadInt32();
              Triggers[i].Effects = new List<Effect>(numEffects);
              for (var j = 0; j < numEffects; j++)
              {
                Triggers[i].Effects.Add(new Effect() { Type = (EffectType)dr.ReadInt32() });
                var numFields = dr.ReadInt32();
                for (var k = 0; k < numFields; k++)
                {
                  Triggers[i].Effects[j].Fields.Add(dr.ReadInt32());
                }
                Triggers[i].Effects[j].Text = dr.ReadBytes(dr.ReadInt32());
                Triggers[i].Effects[j].SoundFile = dr.ReadBytes(dr.ReadInt32());
                Triggers[i].Effects[j].UnitIDs = new List<int>(Triggers[i].Effects[j].GetField(EffectField.NumSelected));
                for (var k = 0; k < Triggers[i].Effects[j].GetField(EffectField.NumSelected); k++)
                {
                  Triggers[i].Effects[j].UnitIDs.Add(dr.ReadInt32());
                }
              }
              Triggers[i].EffectsOrder = new List<int>(numEffects);
              for (var j = 0; j < numEffects; j++)
              {
                Triggers[i].EffectsOrder.Add(dr.ReadInt32());
              }
              var numConditions = dr.ReadInt32();
              Triggers[i].Conditions = new List<Condition>(numConditions);
              for (var j = 0; j < numConditions; j++)
              {
                Triggers[i].Conditions.Add(new Condition() { Type = (ConditionType)dr.ReadInt32() });
                var numFields = dr.ReadInt32();
                for (var k = 0; k < numFields; k++)
                {
                  Triggers[i].Conditions[j].Fields.Add(dr.ReadInt32());
                }
              }
              Triggers[i].ConditionsOrder = new List<int>(numConditions);
              for (var j = 0; j < numConditions; j++)
              {
                Triggers[i].ConditionsOrder.Add(dr.ReadInt32());
              }
            }
            TriggersOrder = new List<int>(numTriggers);
            for (var i = 0; i < numTriggers; i++)
            {
              TriggersOrder.Add(dr.ReadInt32());
            }
            HasAiFile = dr.ReadInt32();
            if (dr.ReadInt32() == 1)
              dr.ReadBytes(396);
            if (HasAiFile == 1)
            {
              var numAiFiles = dr.ReadInt32();
              AiFiles = new Dictionary<byte[], byte[]>(numAiFiles);
              for (var i = 0; i < numAiFiles; i++)
              {
                AiFiles.Add(dr.ReadBytes(dr.ReadInt32()), dr.ReadBytes(dr.ReadInt32()));
              }
            }
          }
        }
      }
    }

    public MemoryStream GetStream()
    {
      var ret = new MemoryStream();
      using (var bw1 = new BinaryWriter(ret, Encoding.Default, true))
      {
        bw1.Write(Version);
        bw1.Write(Instruction.Length + 20);
        bw1.Write(FormatVersion);
        bw1.Write(LastSave);
        bw1.Write(Instruction);
        bw1.Write(0);
        bw1.Write(PlayerCount);
        if (FormatVersion == 3)
        {
          bw1.Write(1000);
          bw1.Write(1);
          bw1.Write(unknownInt32s.Count);
          foreach (var u in unknownInt32s)
            bw1.Write(u);
        }
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms))
        using (var cs = new DeflateStream(ret, CompressionMode.Compress))
        {
          bw.Write(NextUid);
          bw.Write(Version2);
          foreach (var p in Players)
            bw.Write(ZeroAppend(p.Name, 256));
          foreach (var p in Players)
            bw.Write(p.Name_StringTable);
          foreach(var p in Players)
          {
            bw.Write(p.IsActive);
            bw.Write(p.IsHuman);
            bw.Write(p.Civilization);
            bw.Write(4);
          }
          bw.Write(1);
          bw.Write((byte)0);
          bw.Write(-1.0f);
          bw.Write((short)OriginalFileName.Length);
          bw.Write(OriginalFileName);
          foreach (var s in StringTableInfos)
            bw.Write(s);
          foreach(var s in StringInfos)
          {
            bw.Write((short)s.Length);
            bw.Write(s);
          }
          bw.Write(HasBitmap);
          bw.Write(BitmapX);
          bw.Write(BitmapY);
          bw.Write(1s);
        }
      }
      return ret;
    }

    private ScxVersion GetVersion()
    {
      if (Version[2] == 0x31)
        return ScxVersion.Version118;
      else if (Version2 < 1.2201f)
        return ScxVersion.Version122;
      else if (Version2 < 1.2301f)
        return ScxVersion.Version123;
      else if (Version2 < 1.2401f)
        return ScxVersion.Version124;
      else
        return ScxVersion.Version126;
    }

    private static byte[] ZeroAppend(byte[] input,int totalLength)
    {
      return input.Concat(new byte[totalLength - input.Length]).ToArray();
    }
  }

  public class Player
  {
    public byte[] Name { get; set; }
    public int Name_StringTable { get; set; }
    public int IsActive { get; set; }
    public int IsHuman { get; set; }
    public int Civilization { get; set; }
    public byte[] Ai { get; set; }
    public byte[] AiFile { get; set; }
    public byte Personality { get; set; }
    public int Gold { get; set; }
    public int Wood { get; set; }
    public int Food { get; set; }
    public int Stone { get; set; }
    public int Ore { get; set; }
    public int PlayerNumber { get; set; }
    public List<DiplomacyInt> Diplomacies { get; }
    public int AlliedVictory { get; set; }
    public List<int> DisabledTechs { get; }
    public List<int> DisabledUnits { get; }
    public List<int> DisabledBuildings { get; }
    public StartAge StartAge { get; set; }

    public Player()
    {
      Diplomacies = new List<DiplomacyInt>(16);
      DisabledTechs = new List<int>(30);
      DisabledUnits = new List<int>(30);
      DisabledBuildings = new List<int>(30);
    }
  }

  public class BITMAPDIB
  {
    public int Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Planes { get; set; }
    public int BitCount { get; set; }
    public int Compression { get; set; }
    public int SizeImage { get; set; }
    public int XPelsPerMeter { get; set; }
    public int YPelsPerMeter { get; set; }
    public int ClrUsed { get; set; }
    public int ClrImportant { get; set; }
    public List<RGB> Colors { get; set; }
    public byte[] ImageData { get; set; }
  }

  public class RGB
  {
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
  }

  public class Terrain
  {
    public byte Id { get; set; }
    public short Elevation { get; set; }
  }

  public class Resource
  {
    public float Food { get; set; }
    public float Wood { get; set; }
    public float Gold { get; set; }
    public float Stone { get; set; }
    public float Ore { get; set; }
    public float PopulationLimit { get; set; }
  }

  public class Unit
  {
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public int Id { get; set; }
    public short UnitClass { get; set; }
    public byte State { get; set; }
    public float Rotation { get; set; }
    public short Frame { get; set; }
    public int Garrison { get; set; }
  }

  public class PlayerMisc
  {
    public byte[] Name { get; set; }
    public float CameraX { get; set; }
    public float CameraY { get; set; }
    public byte AlliedVictory { get; set; }
    public List<DiplomacyByte> Diplomacies { get; }
    public List<Diplomacy2> Diplomacies2 { get; }
    public PlayerColor Color { get; set; }

    public PlayerMisc()
    {
      Diplomacies = new List<DiplomacyByte>(9);
      Diplomacies2 = new List<Diplomacy2>(9);
    }
  }

  public class Trigger
  {
    public int IsEnabled { get; set; }
    public int IsLooping { get; set; }
    public byte IsObjective { get; set; }
    public int DiscriptionOrder { get; set; }
    public byte[] Discription { get; set; }
    public byte[] Name { get; set; }
    public List<Effect> Effects { get; set; }
    public List<int> EffectsOrder { get; set; }
    public List<Condition> Conditions { get; set; }
    public List<int> ConditionsOrder { get; set; }

    public Trigger()
    {

    }
  }

  public class Effect
  {
    public EffectType Type { get; set; }
    public List<int> Fields { get; }
    public byte[] Text { get; set; }
    public byte[] SoundFile { get; set; }
    public List<int> UnitIDs { get; set; }

    public Effect()
    {
      Fields = new List<int>(23);
    }

    public int GetField(EffectField field)
    {
      return Fields[(int)field];
    }

    public void SetField(EffectField field, int value)
    {
      Fields[(int)field] = value;
    }
  }

  public class Condition
  {
    public ConditionType Type { get; set; }
    public List<int> Fields { get; }

    public Condition()
    {
      Fields = new List<int>(16);
    }

    public int GetField(EffectField field)
    {
      return Fields[(int)field];
    }

    public void SetField(EffectField field, int value)
    {
      Fields[(int)field] = value;
    }
  }

  public enum DiplomacyInt : int
  {
    Allied,
    Neutral,
    Enemy = 3
  }

  public enum DiplomacyByte : byte
  {
    Allied,
    Newtral,
    Enemy = 3
  }

  public enum Diplomacy2
  {
    Gaia,
    Self,
    Allied,
    Neutral,
    Enemy
  }

  public enum StartAge
  {
    None = -1,
    Dark,
    Feudal,
    Castle,
    Imperial,
    PostImperial
  }

  public enum ScxVersion
  {
    Version118,
    Version122,
    Version123,
    Version124,
    Version126,
    Unknown
  }

  public enum MapType
  {
    Arabia = 9,
    Archipelago,
    Baltic,
    BlackForest,
    Coastal,
    Continental,
    CraterLake,
    Fortress,
    GoldRush,
    Highland,
    Islands,
    Mediterranean,
    Migration,
    Rivers,
    TeamIslands,
    Scandinavia = 0x19,
    Yucatan = 0x1B,
    SaltMarsh,
    KingOfTheHill = 0x1E,
    Oasis,
    Nomad = 0x21
  }

  public enum PlayerColor
  {
    Blue,
    Red,
    Green,
    Yellow,
    Cyan,
    Purple,
    Gray,
    Orange
  }

  public enum EffectType
  {
    ChangeDiplomacy = 1,
    ResearchTechnology,
    SendChat,
    PlaySound,
    SendTribute,
    UnlockGate,
    LockGate,
    ActivateTrigger,
    DeactivateTrigger,
    AIScriptGoal,
    CreateObject,
    TaskObject,
    DeclareVictory,
    KillObject,
    RemoveObject,
    ChangeView,
    Unload,
    ChangeOwnership,
    Patrol,
    DisplayInstructions,
    ClearInstructions,
    FreezeUnit,
    UseAdvancedButtons,
    DamageObject,
    PlaceFoundation,
    ChangeObjectName,
    ChangeObjectHP,
    ChangeObjectAttack,
    StopUnit,
    SnapView,
    EnableTech = 32,
    DisableTech,
    EnableUnit,
    DisableUnit,
    FlashObjects
  }

  public enum ConditionType
  {
    BringObjectToArea,
    BringObjectToObject,
    OwnObjects,
    OwnFewerObjects,
    ObjectsInArea,
    DestroyObject,
    CaptureObject,
    AccumulateAttribute,
    ResearchTechnology,
    Timer,
    ObjectSelected,
    AiSignal,
    PlayerDefeated,
    ObjectHasTarget,
    ObjectVisible,
    ObjectNotVisible,
    ResearchingTechnology,
    UnitsGarrisoned,
    DifficultyLevel,
    OwnFewerFoundations,
    SelectedObjectsInArea,
    PoweredObjectsInArea,
    UnitsQueuedPastPopCap
  }

  public enum EffectField
  {
    AIGoal,
    Amount,
    Resource,
    Diplomacy,
    NumSelected,
    LocationUnit,
    UnitID,
    PlayerSource,
    PlayerTarget,
    Technology,
    StringTableID,
    Unknown,
    DisplayTime,
    Trigger,
    LocationX,
    LocationY,
    AreaSouthWestX,
    AreaSouthWestY,
    AreaNorthEastX,
    AreaNorthEastY,
    UnitGroup,
    UnitType,
    InstructionPanel
  }
}
