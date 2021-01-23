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

using MailKit;
using MailKit.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace penCsharpener.Mail2DB
{
    public class ImapFilter
    {

        private SearchQuery search;
        private bool _defaultAnd;
        private ImapFilterOperators _operator;
        public ImapFilterOperators Operator
        {
            get => _operator;
            set
            {
                _defaultAnd = _operator == ImapFilterOperators.And;
                _operator = value;
            }
        }


        public ImapFilter(bool defaultAND = true)
        {
            _defaultAnd = defaultAND;
        }

        private void AndOrOr(SearchQuery query)
        {
            if (search == null)
            {
                search = query;
                return;
            }
            if (_defaultAnd)
            {
                search = SearchQuery.And(search, query);
            }
            else
            {
                search = SearchQuery.Or(search, query);
            }
        }

        public ImapFilter BodyContains(string bodyText)
        {
            AndOrOr(SearchQuery.BodyContains(bodyText));
            return this;
        }

        public ImapFilter SubjectContains(string subjectText)
        {
            AndOrOr(SearchQuery.SubjectContains(subjectText));
            return this;
        }

        public ImapFilter NotSeen()
        {
            AndOrOr(SearchQuery.NotSeen);
            return this;
        }

        public ImapFilter Seen()
        {
            AndOrOr(SearchQuery.Seen);
            return this;
        }

        public ImapFilter NotAnswered()
        {
            AndOrOr(SearchQuery.NotAnswered);
            return this;
        }

        public ImapFilter Recent()
        {
            AndOrOr(SearchQuery.Recent);
            return this;
        }

        public ImapFilter ToContains(string text)
        {
            AndOrOr(SearchQuery.ToContains(text));
            return this;
        }

        public ImapFilter CcContains(string text)
        {
            AndOrOr(SearchQuery.CcContains(text));
            return this;
        }

        public ImapFilter FromContains(string text)
        {
            AndOrOr(SearchQuery.FromContains(text));
            return this;
        }

        public ImapFilter Uids(IList<uint> Uids)
        {
            AndOrOr(SearchQuery.Uids(Uids.Select(x => new UniqueId(x)).ToList()));
            return this;
        }

        public ImapFilter Uids(IList<UniqueId> Uids)
        {
            AndOrOr(SearchQuery.Uids(Uids));
            return this;
        }

        public ImapFilter YoungerThan(int seconds)
        {
            AndOrOr(SearchQuery.YoungerThan(seconds));
            return this;
        }

        public ImapFilter OlderThan(int seconds)
        {
            AndOrOr(SearchQuery.OlderThan(seconds));
            return this;
        }

        public ImapFilter SentSince(DateTime since)
        {
            AndOrOr(SearchQuery.SentSince(since));
            return this;
        }

        public ImapFilter SentBefore(DateTime before)
        {
            AndOrOr(SearchQuery.SentBefore(before));
            return this;
        }

        public ImapFilter SentBetween(DateTime from, DateTime to)
        {
            AndOrOr(SearchQuery.And(SearchQuery.SentSince(from), SearchQuery.SentBefore(to)));
            return this;
        }

        public ImapFilter DeliveredBetween(DateTime from, DateTime to)
        {
            AndOrOr(SearchQuery.And(SearchQuery.DeliveredAfter(from), SearchQuery.DeliveredBefore(to)));
            return this;
        }

        public ImapFilter DeliveredAfter(DateTime after)
        {
            AndOrOr(SearchQuery.DeliveredAfter(after));
            return this;
        }

        public ImapFilter DeliveredBefore(DateTime before)
        {
            AndOrOr(SearchQuery.DeliveredBefore(before));
            return this;
        }

        public ImapFilter MessageContains(string text)
        {
            AndOrOr(SearchQuery.MessageContains(text));
            return this;
        }

        internal int LimitResults { get; set; }
        public ImapFilter Limit(int limit)
        {
            LimitResults = limit;
            return this;
        }

        public ImapFilter And(ImapFilter imapFilter)
        {
            search.And(imapFilter.ToSearchQuery());
            return this;
        }

        public ImapFilter Or(ImapFilter imapFilter)
        {
            search.Or(imapFilter.ToSearchQuery());
            return this;
        }

        public SearchQuery ToSearchQuery()
        {
            return search ?? SearchQuery.All;
        }

        public override string ToString()
        {
            return search.ToString();
        }
    }
}
