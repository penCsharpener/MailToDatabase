using MailToDatabase.Sqlite.Services.Abstractions;
using System;
using System.Security.Cryptography;

namespace MailToDatabase.Sqlite.Services
{
    public class HashProvider : IHashProvider
    {
        public string ToSha1(byte[] bytes)
        {
            using var hash = SHA1.Create();
            var sha1 = hash.ComputeHash(bytes);
            var hex = BitConverter.ToString(sha1).Replace("-", "");

            return hex;
        }

        public string ToSha256(byte[] bytes)
        {
            using var hash = SHA256.Create();
            var sha256 = hash.ComputeHash(bytes);
            var hex = BitConverter.ToString(sha256).Replace("-", "");

            return hex;
        }
    }
}
