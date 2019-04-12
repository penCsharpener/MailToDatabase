using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB {
    public static class MailkitExtensions {
        public static async Task<byte[]> ToBytes(this MimePart mimePart) {
            using (var ms = new MemoryStream()) {
                await mimePart.Content.DecodeToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}
