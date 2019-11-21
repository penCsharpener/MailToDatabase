﻿/*
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

using MailKit;
using System;

namespace penCsharpener.Mail2DB {
    public class ImapMessage {

        public byte[] MimeMessageBytes { get; set; }
        public uint UId { get; set; }
        public string MailFolder { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string BodyPlainText { get; set; }
        public bool IsHTML { get; set; }
        public bool HasAttachments { get; set; }
        public string MessageTextId { get; set; }
        public string InReplyToId { get; set; }
        public DateTime ReceivedAtUTC { get; set; }
        public DateTime ReceivedAtLocal { get; set; }
        public MailContact From { get; set; }
        public MailContact[] Cc { get; set; }
        public MailContact[] To { get; set; }
        public ImapAttachment[] Attachments { get; set; }

    }
}
