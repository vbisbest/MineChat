using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;
using System.Collections.Generic;
using System.Text;

namespace MineChatAPI
{
    public sealed class PKCS1Signature
    {
        // Not really related to this class.
        public static byte[] CreateSecretKey(int length = 16)
        {
            var generator = new CipherKeyGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(), length * 8));

            return generator.GenerateKey();
        }

        private readonly AsymmetricKeyParameter _publicKey;
        public PKCS1Signature(byte[] publicKey)
        {
            _publicKey = PublicKeyFactory.CreateKey(publicKey);
        }

        public byte[] SignData(byte[] data)
        {
            var eng = new Pkcs1Encoding(new RsaEngine());
            eng.Init(true, _publicKey);
            return eng.ProcessBlock(data, 0, data.Length);
        }

        public static string GetServerHash(string serverID, byte[] publicKey, byte[] secretKey)
        {
            List<byte> hashList = new List<byte>();
            hashList.AddRange(Encoding.GetEncoding("iso-8859-1").GetBytes(serverID));
            hashList.AddRange(secretKey);
            hashList.AddRange(publicKey);

            byte[] hash = Digest(hashList.ToArray());

            bool negative = (hash[0] & 0x80) == 0x80;
            if (negative) { hash = TwosComplementLittleEndian(hash); }
            string result = GetHexString(hash).TrimStart('0');
            if (negative) { result = "-" + result; }

            return result;
        }


        private static byte[] Digest(byte[] tohash)
        {
            Org.BouncyCastle.Crypto.Digests.Sha1Digest sha1 = new Org.BouncyCastle.Crypto.Digests.Sha1Digest();
            sha1.BlockUpdate(tohash, 0, tohash.Length);

            int finalLength = 0;

            int l = sha1.GetDigestSize();
            byte[] final = new byte[l];
            sha1.DoFinal(final, finalLength);

            return final;
        }

        private static string GetHexString(byte[] p)
        {
            string result = string.Empty;
            for (int i = 0; i < p.Length; i++)
                result += p[i].ToString("x2");
            return result;
        }

        private static byte[] TwosComplementLittleEndian(byte[] p)
        {
            int i;
            bool carry = true;
            for (i = p.Length - 1; i >= 0; i--)
            {
                p[i] = (byte)~p[i];
                if (carry)
                {
                    carry = p[i] == 0xFF;
                    p[i]++;
                }
            }
            return p;
        }
    }
}