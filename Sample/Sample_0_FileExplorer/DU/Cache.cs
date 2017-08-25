using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileExplorer
{
    public class Cache
    {
        public long TotalSize { get; set; }

        public IBigFileHelper FileHelper { get; set; }

        public List<Block> BlockList { get; private set; }

        public Cache()
        {
            TotalSize = 0L;
            BlockList = new List<Block>();
        }

        public void Add(long position, byte[] data, int size)
        {
            Block block = new Block();
            block.Position = position;
            block.Data = data;
            block.Size = size;
            BlockList.Add(block);
            TotalSize += size;
        }

        public void Flush()
        {
            BlockList.Sort(new BlockComparer());
            foreach (Block block in BlockList)
            {
                FileHelper.Update(block.Position, block.Data, 0, block.Size);
            }
        }

        public void Reset()
        {
            TotalSize = 0;
            BlockList.Clear();
        }

        public class Block
        {
            public long Position { get; set; }

            public int Size { get; set; }

            public byte[] Data { get; set; }
        }

        public class BlockComparer : IComparer<Block>
        {
            public int Compare(Block x, Block y)
            {
                long xpos = x == null ? 0L : x.Position;
                long ypos = y == null ? 0L : y.Position;
                return xpos < ypos ? -1 : (xpos > ypos ? 1 : 0);
            }
        }
    }
}
