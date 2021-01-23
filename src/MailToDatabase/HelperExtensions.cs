/*
MIT License

Copyright (c) 2019 Matthias Müller

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Linq;
using System.Text;

namespace MailToDatabase
{
    internal static class HelperExtensions
    {
        internal static byte[] ToBytes(this string str, Encoding encoding)
        {
            return encoding.GetBytes(str);
        }

        internal static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        internal static string ToSha256(this byte[] bytes)
        {
            using (var sha1 = new System.Security.Cryptography.SHA256Managed())
            {
                var byteHash = sha1.ComputeHash(bytes);
                return byteHash.ToHex();
            }
        }

        internal static string ToHex(this byte[] bytes)
        {
            return string.Concat(bytes.Select(x => x.ToString("X2"))).ToLower();
        }
    }
}
