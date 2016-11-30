using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace YTY
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
    private List<int> stringTableInfos;
    private List<byte[]> stringInfos;
    private int hasBitmap;
    private int bitmapX;
    private int bitmapY;
    private BITMAPDIB bitmap;

    public List<Player> Players { get; }

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
            players = new List<Player>(16);
            for (var i = 0; i < 16; i++)
            {
              players.Add(new Player()
              {
                name = dr.ReadBytes(256)
              });
            }
            for (var i = 0; i < 16; i++)
            {
              players[i].Name_StringTable = dr.ReadInt32();
            }
            for (var i = 0; i < 16; i++)
            {
              players[i].isActive = dr.ReadInt32();
              players[i].isHuman = dr.ReadInt32();
              players[i].civilization = dr.ReadInt32();
              dr.ReadInt32();
            }
            dr.ReadBytes(9);
            originalFileName = dr.ReadBytes(dr.ReadInt16());
            stringTableInfos = new List<int>(6);
            for(var i =0;i<5;i++)
            {
              stringTableInfos.Add(dr.ReadInt32());
            }
            if (GetVersion() >= ScxVersion.Version122)
              stringTableInfos.Add(dr.ReadInt32());
            stringInfos = new List<byte[]>(10);
            for(var i =0;i<9;i++)
            {
              stringInfos.Add(dr.ReadBytes(dr.ReadInt16()));
            }
            if (GetVersion() >= ScxVersion.Version122)
              stringInfos.Add(dr.ReadBytes(dr.ReadInt16()));
            hasBitmap = dr.ReadInt32();
            bitmapX = dr.ReadInt32();
            bitmapY = dr.ReadInt32();
            dr.ReadBytes(2);
            if(bitmapX>0&&bitmapY>0)
            {
              bitmap = new BITMAPDIB()
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
              for(var i =0;i<256;i++)
              {
                bitmap.Colors.Add(new RGB()
                {
                  Red = dr.ReadByte(),
                  Green = dr.ReadByte(),
                  Blue = dr.ReadByte(),
                });
                dr.ReadByte();
              }
              bitmap.ImageData = dr.ReadBytes(((bitmapX - 1) / 4 + 1) * 4 * bitmapY);
            }
            for(var i= 0;i<32;i++)
            {
              dr.ReadBytes(dr.ReadInt16());
            }
            for(var i = 0;i<16;i++)
            {
              players[i].ai = dr.ReadBytes(dr.ReadInt16());
            }
          }
        }
      }
    }

    private ScxVersion GetVersion()
    {
      if (version[2] == 0x31)
        return ScxVersion.Version118;
      else if (version2 < 1.2201f)
        return ScxVersion.Version122;
      else if (version2 < 1.2301f)
        return ScxVersion.Version123;
      else if (version2 < 1.2401f)
        return ScxVersion.Version124;
      else
        return ScxVersion.Version126;
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

  public  class RGB
  {
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
  }

  public enum DiplomacyInt
  {
    Allied,
    Neutral,
    Enemy = 3
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
}
