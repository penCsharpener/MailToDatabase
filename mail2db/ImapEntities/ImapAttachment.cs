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

using penCsharpener.Mail2DB.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB {
    public class ImapAttachment : IWriteAttachment {
        public string Filename { get; set; }
        public long Filesize => FileContent.LongLength;
        public byte[] FileContent { get; set; }
        public string Sha256Hash => FileContent?.ToSha256();
        public string Subfolder { get; protected set; }
        public string FullPath { get; set; }
        public FileInfo FileInfo => GetFileInfo();

        private FileInfo GetFileInfo() {
            if (File.Exists(FullPath)) {
                return new FileInfo(FullPath);
            }
            return null;
        }

        /// <summary>
        /// Writes the byte[] FileContent to the specified path. 
        /// If no filename is set the property Filename will be used.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public async Task WriteFileAsync(string path,
                                   FileSavingOption options = null,
                                   string filename = null,
                                   bool overwrite = false) {

            if (options != null) {
                switch (options.Option) {
                    case FileSavingOptions.SavingTimestamp:
                        Subfolder = DateTime.Now.ToString("yyyy-MM-dd_HHmmss_ff");
                        break;
                    case FileSavingOptions.FirstTwoHashCharacters:
                        Subfolder = Sha256Hash?.Substring(0, 2);
                        break;
                    case FileSavingOptions.EmailAccount:
                        Subfolder = options.EmailAccount;
                        break;
                }
            }

            // check if path is really a dir and not a filepath
            if (File.Exists(path)) {
                // if the path is a filepath just take its directory
                path = Path.GetDirectoryName(path);
            }

            var fullPath = Path.Combine(path, Subfolder);

            // create path if it doesn't exist yet
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
            }

            filename = filename.IsNullOrEmpty() ? Filename : filename;

            var fullFilename = Path.Combine(fullPath, filename);
            if (!File.Exists(fullFilename) || overwrite) {

                using (var fs = new FileStream(fullFilename, FileMode.Create,
                                               FileAccess.Write, FileShare.None,
                                               bufferSize: 4096, useAsync: true)) {
                    await fs.WriteAsync(FileContent, 0, FileContent.Length);
                }
                FullPath = fullFilename;
            }
        }
    }
}
