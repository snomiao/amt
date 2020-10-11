﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTY.amt
{
  public class FileDto
  {
    public int Id { get; set; }

    public string SourceUri { get; set; }

    public string FileName { get; set; }

    public long Size { get; set; }

    public int Version { get; set; }

    public string Md5 { get; set; }

    public int Status { get; set; }
  }
}
