using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace ZippyBackup
{
    /// <summary>
    /// Computes the CRC-32 of a given buffer.  An example of usage:
    /// 
    /// <code>
    /// UInt32 polynomial = 0xedb88320;
    /// UInt32 seed = 0xffffffff;
    ///
    /// Crc32 crc32 = new Crc32(polynomial, seed);
    /// UInt32 CRC32Hash;
    /// 
    /// using (FileStream fs = File.Open("c:\\myfile.txt", FileMode.Open))
    ///     CRC32Hash = crc32.Compute(fs);
    /// 
    /// Console.WriteLine("CRC-32 is {0}", CRC32Hash);
    /// </code>
    /// </summary>
    public class Crc32 : HashAlgorithm
    {
        public const UInt32 DefaultPolynomial = 0xedb88320;
        public const UInt32 DefaultSeed = 0xffffffff;

        private UInt32 hash;
        private UInt32 seed;
        private UInt32[] table;
        private static UInt32[] defaultTable;

        public Crc32()
        {
            table = InitializeTable(DefaultPolynomial);
            seed = DefaultSeed;
            Initialize();
        }

        public Crc32(UInt32 polynomial, UInt32 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = seed;
            Initialize();
        }

        public override void Initialize()
        {
            hash = seed;
        }

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            hash = CalculateHash(table, hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
            this.HashValue = hashBuffer;
            return hashBuffer;
        }

        public override int HashSize
        {
            get { return 32; }
        }

#if false       // Inefficient methods
        public static UInt32 Compute(byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(Stream s)
        {
            byte[] buffer = new byte[s.Length];
            for (int ii = 0; ; ii++)
            {
                int ch = s.ReadByte();
                if (ch < 0) break;
                buffer[ii] = (byte)ch;
            }
            return Compute(buffer);
        }
#endif

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            UInt32[] createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
        {
            UInt32 crc = seed;
            for (int i = start; i < size; i++)
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        public UInt32 Compute(byte[] buffer, int length)
        {
            return ~CalculateHash(table, seed, buffer, 0, length);
        }

        public UInt32 Compute(Stream s)
        {
            byte[] buffer = new byte [4096];
            UInt32 LastResult = seed;
            for (;;)
            {
                int length = s.Read(buffer, 0, 4096);
                if (length == 0) return ~LastResult;
                LastResult = CalculateHash(table, LastResult, buffer, 0, length);
            }
        }

        private byte[] UInt32ToBigEndianBytes(UInt32 x)
        {
            return new byte[] {
			    (byte)((x >> 24) & 0xff),
			    (byte)((x >> 16) & 0xff),
			    (byte)((x >> 8) & 0xff),
			    (byte)(x & 0xff)
		    };
        }
    }
}
