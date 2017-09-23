using System.Security.Cryptography;
using System.Text;

namespace TempMail
{
    internal sealed class Rng
    {
        const int MIN_LENGTH = 6;
        const int MAX_LENGTH = 10;
        const int BUFFER_SIZE = 1024;

        byte[] RandomBuffer;
        int BufferOffset;
        static RNGCryptoServiceProvider rng;

        static Rng() => rng = new RNGCryptoServiceProvider();

        public Rng()
        {
            RandomBuffer = new byte[BUFFER_SIZE];
            BufferOffset = RandomBuffer.Length;
        }

        private byte Next()
        {
            if (BufferOffset >= RandomBuffer.Length)
            {
                rng.GetBytes(RandomBuffer);
                BufferOffset = 0;
            }
            return RandomBuffer[BufferOffset++];
        }

        public int Next(int minValue, int maxValue) => minValue + Next() % (maxValue - minValue);

        public string NextString()
        {
            var sb = new StringBuilder();
            int count = Next(MIN_LENGTH, MAX_LENGTH);

            for (int index = 0; index < count; index++)
            {
                sb.Append((char)Next('a', 'z'));
            }

            return sb.ToString();
        }
    }
}
