using System;
using System.Collections.Generic;
using System.Text;

namespace penCsharpener.Mail2DB {
    public enum FileSavingOptions {
        SavingTimestamp,
        FirstTwoHashCharacters,
        EmailAccount
    }

    public class FileSavingOption {

        public FileSavingOption(FileSavingOptions option) {
            Option = option;
        }

        public FileSavingOption(string emailAccount) {
            EmailAccount = emailAccount;
            Option = FileSavingOptions.EmailAccount;
        }

        public string EmailAccount { get; set; }
        public FileSavingOptions Option { get; set; }
    }
}
