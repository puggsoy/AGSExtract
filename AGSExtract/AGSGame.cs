using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace AGSExtractWPF
{
    class AGSGame
    {
        //Static setup stuff
        private const string CLIBEOFSIG = "CLIB\x1\x2\x3\x4SIGE";
        private const string ENCPASS = "My\x01\xde\x04Jibzle";

        public static AGSGame load(FileStream f)
        {
            BinaryReader b = new BinaryReader(f);
            uint clibOff = getClibOff(b);
            b.BaseStream.Seek(clibOff, SeekOrigin.Begin);
            string magic = new string(b.ReadChars(4));

            if (magic != "CLIB")
                throw new Exception("Invalid CLIB header!");

            b.ReadByte(); //unk
            byte version = b.ReadByte();
            b.ReadByte(); //null

            switch(version)
            {
                case 15:
                    //valid
                    break;
                default:
                    throw new Exception("Unsupported version: " + version);
            }

            AGSGame g = new AGSGame(f);

            g.dataFCount = b.ReadUInt32();
            g.dataFNames = new string[g.dataFCount];

            for(int i = 0; i < g.dataFCount; i++)
                g.dataFNames[i] = new string(b.ReadChars(0x14));

            g.fCount = b.ReadUInt32();
            g.fNames = new string[g.fCount];

            for (int i = 0; i < g.fCount; i++)
                g.fNames[i] = decryptText(b.ReadBytes(0x19));

            g.fOffsets = new uint[g.fCount];

            for(int i = 0; i < g.fCount; i++)
                g.fOffsets[i] = b.ReadUInt32();

            g.fLengths = new uint[g.fCount];

            for (int i = 0; i < g.fCount; i++)
                g.fLengths[i] = b.ReadUInt32();

            g.fDataFiles = new byte[g.fCount];

            for(int i = 0; i < g.fCount; i++)
            {
                g.fDataFiles[i] = b.ReadByte();

                if (g.fDataFiles[i] == 0) //not sure why this?
                    g.fOffsets[i] += clibOff;
            }

            return g;
        }

        private static uint getClibOff(BinaryReader b)
        {
            b.BaseStream.Seek(-0x0C, SeekOrigin.End);

            if (new string(b.ReadChars(0x0C)) != CLIBEOFSIG)
                throw new Exception("Invalid AGS game!");

            b.BaseStream.Seek(-0x10, SeekOrigin.End);

            return b.ReadUInt32();
        }

        private static string decryptText(byte[] enc)
        {
            byte[] pass = Encoding.Default.GetBytes(ENCPASS);
            uint adx = 0;
            byte[] dec = new byte[enc.Length];

            int i = 0;
            for(i = 0; i < enc.Length; i++)
            {
                dec[i] = (byte)(enc[i] - pass[adx]);
                if (dec[i] == 0) break;
                adx++;
                if (adx > 10) adx = 0;
            }

            return Encoding.UTF8.GetString(dec).Substring(0, i);
        }

        //End of static

        private FileStream f;
        
        private uint dataFCount;
        private string[] dataFNames;
        private uint fCount;
        public string[] fNames { get; private set; }
        private uint[] fOffsets;
        private uint[] fLengths;
        private byte[] fDataFiles;

        public AGSGame(FileStream f)
        {
            this.f = f;
        }

        public uint count()
        {
            return fCount;
        }

        public byte[] getFile(int index)
        {
            f.Seek(fOffsets[index], SeekOrigin.Begin);
            return new BinaryReader(f).ReadBytes((int)fLengths[index]);
        }

        public byte[] getFile(string name)
        {
            int index = fNames.ToList().IndexOf(name);
            return getFile(index);
        }

        public void close()
        {
            f.Close();
        }
    }
}
