using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace YTY.amt
{
  public class ScxFile
  {
    private string fileName;
    private byte[] version;
    private int formatVersion;
    private int lastSave;
    private byte[] instruction;
    private int playerCount;
    private int nextUid;
    private float version2;
    private List<Player> players;
    private byte[] originalFileName;

    public IEnumerable< Player> Players { get; }

    public ScxFile(string fileName)
    {
      this.fileName = fileName;
    }

    public ScxFile(Stream stream)
    {
      using (var br = new BinaryReader(stream))
      {
        version = br.ReadBytes(4);
        br.ReadBytes(4);
        formatVersion = br.ReadInt32();
        lastSave = br.ReadInt32();
        instruction = br.ReadBytes(br.ReadInt32());
        br.ReadBytes(4);
        playerCount = br.ReadInt32();
        if (formatVersion == 3)
        {
          br.ReadBytes(8);
          var c = br.ReadInt32();
          for (var i = 0; i < c; i++)
            br.ReadInt32();
        }
        using (var ds = new DeflateStream(stream, CompressionMode.Decompress))
        {
          using (var dr = new BinaryReader(ds))
          {
            nextUid = dr.ReadInt32();
            version2 = dr.ReadSingle();
            for(var i = 0; i<16;i++)
            {
              players.Add(new Player()
              {
                name = dr.ReadBytes(256)
              });
            }
            for(var i=0;i<16;i++)
            {
              players[i].Name_StringTable = dr.ReadInt32();
            }
            for(var i=0;i<16;i++)
            {
              players[i].isActive = dr.ReadInt32();
              players[i].isHuman = dr.ReadInt32();
              players[i].civilization = dr.ReadInt32();
              dr.ReadInt32();
            }
            dr.ReadBytes(9);
            originalFileName = dr.ReadBytes(dr.ReadInt16());
          }
        }
      }
    }
  }

  public class Player
  {
    internal byte[] name;
    public int Name_StringTable { get; set; }
    internal int isActive;
    internal int isHuman;
    internal int civilization;
    internal byte[] ai;
    internal byte[] aiFile;
    internal byte personality;
    public int Gold { get; set; }
    public int Wood { get; set; }
    public int Food { get; set; }
    public int Stone { get; set; }
    public int Ore { get; set; }
    public int PlayerNumber { get; set; }
    public DiplomacyInt[] Diplomacies { get; }
    internal int alliedVictory;
    public int[] DisabledTechs { get; }
    public int[] DisabledUnits { get; }
    public int[] DisabledBuildings { get; }
    public StartAge StartAge { get; set; }
  }

  public enum DiplomacyInt
  {
    Allied,
    Neutral,
    Enemy = 3
  }

  public enum StartAge
  {
    None=-1,
    Dark,
    Feudal,
    Castle,
    Imperial,
    PostImperial
  }
}
