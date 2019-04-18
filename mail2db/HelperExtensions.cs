using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace penCsharpener.Mail2DB {
    public static class HelperExtensions {
        public static byte[] ToBytes(this string str, Encoding encoding) => encoding.GetBytes(str);

        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        public static string ToSha256(this byte[] bytes) {
            using (var sha1 = new System.Security.Cryptography.SHA256Managed()) {
                var byteHash = sha1.ComputeHash(bytes);
                return byteHash.ToHex();
            }
        }

        public static string ToHex(this byte[] bytes) {
            return string.Concat(bytes.Select(x => x.ToString("X2"))).ToLower();
        }
    }
}
