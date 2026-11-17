using System.Security.Cryptography;
using System.Text;

namespace Adapter.OutBound.AdapterSql.Settings
{
    public static class Crypt
    {

        public static string decryptDES(string dados, string chave)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dados);
            ArgumentException.ThrowIfNullOrWhiteSpace(chave);

            using var des = DES.Create();

            des.Key = GetKey(chave);
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            using var decryptor = des.CreateDecryptor();

            var buffer = Convert.FromBase64String(dados);

            var decrypted = decryptor.TransformFinalBlock(
                buffer,
                0,
                buffer.Length);

            return Encoding.UTF8.GetString(decrypted);
        }

        private static byte[] GetKey(string chave)
        {
            var key = Encoding.UTF8.GetBytes(chave);

            if (key.Length != 8)
            {
                throw new ArgumentException(
                    "A chave DES deve possuir exatamente 8 bytes em UTF-8.",
                    nameof(chave));
            }

            return key;
        }
    }
}