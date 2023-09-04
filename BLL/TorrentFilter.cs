using System;
using System.Collections.Generic;
using BencodeLibrary;
using MODEL;

namespace BLL
{
    public class TorrentFilter:FilterBase
    {
        public override List<RarbgTitle> GetList(string p)
        {
            return Tool.DecodeTorrent(p);
        }
    }
}