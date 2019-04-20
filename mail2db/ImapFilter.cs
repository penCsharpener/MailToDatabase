using MailKit;
using MailKit.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace penCsharpener.Mail2DB {
    public class ImapFilter {

        private SearchQuery search;
        private bool _defaultAnd;

        public ImapFilter(bool defaultAND = true) {
            _defaultAnd = defaultAND;
        }

        private void AndOrOr(SearchQuery query) {
            if (search == null) {
                search = query;
                return;
            }
            if (_defaultAnd) {
                SearchQuery.And(search, query);
            } else {
                SearchQuery.Or(search, query);
            }
        }

        public ImapFilter BodyContains(string bodyText) {
            AndOrOr(SearchQuery.BodyContains(bodyText));
            return this;
        }

        public ImapFilter SubjectContains(string subjectText) {
            AndOrOr(SearchQuery.SubjectContains(subjectText));
            return this;
        }

        public ImapFilter NotSeen() {
            AndOrOr(SearchQuery.NotSeen);
            return this;
        }

        public ImapFilter Seen() {
            AndOrOr(SearchQuery.Seen);
            return this;
        }

        public ImapFilter NotAnswered() {
            AndOrOr(SearchQuery.NotAnswered);
            return this;
        }

        public ImapFilter Recent() {
            AndOrOr(SearchQuery.Recent);
            return this;
        }

        public ImapFilter ToContains(string text) {
            AndOrOr(SearchQuery.ToContains(text));
            return this;
        }

        public ImapFilter CcContains(string text) {
            AndOrOr(SearchQuery.CcContains(text));
            return this;
        }

        public ImapFilter FromContains(string text) {
            AndOrOr(SearchQuery.FromContains(text));
            return this;
        }

        public ImapFilter Uids(IList<uint> Uids) {
            AndOrOr(SearchQuery.Uids(Uids.Select(x => new UniqueId(x)).ToList()));
            return this;
        }

        public ImapFilter Uids(IList<UniqueId> Uids) {
            AndOrOr(SearchQuery.Uids(Uids));
            return this;
        }

        public ImapFilter YoungerThan(int seconds) {
            AndOrOr(SearchQuery.YoungerThan(seconds));
            return this;
        }

        public ImapFilter OlderThan(int seconds) {
            AndOrOr(SearchQuery.OlderThan(seconds));
            return this;
        }

        public ImapFilter SentBetween(DateTime from, DateTime to) {
            AndOrOr(SearchQuery.And(SearchQuery.SentAfter(from), SearchQuery.SentBefore(to)));
            return this;
        }

        public ImapFilter DeliveredBetween(DateTime from, DateTime to) {
            AndOrOr(SearchQuery.And(SearchQuery.DeliveredAfter(from), SearchQuery.DeliveredBefore(to)));
            return this;
        }

        public ImapFilter And(ImapFilter imapFilter) {
            search.And(imapFilter.ToSearchQuery());
            return this;
        }

        public ImapFilter Or(ImapFilter imapFilter) {
            search.Or(imapFilter.ToSearchQuery());
            return this;
        }

        public SearchQuery ToSearchQuery() {
            return search;
        }
    }
}
