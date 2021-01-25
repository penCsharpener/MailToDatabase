using MailToDatabase.ImeReader.Models.Validation;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MailToDatabase.ImeReader.Models.Configuration
{
    public class AppSettings
    {
        public string ImeFilePath { get; private set; }
        public string InputDirectoryPath { get; private set; }
        public string InputPath { get; set; }
        public string OutputFolderPath { get; set; }
        public InputModes InputMode { get; private set; }


        public void ParseCommandlineParameters(IConfiguration configuration)
        {
            if (!string.IsNullOrWhiteSpace(configuration["InputPath"]))
            {
                InputPath = configuration["InputPath"];
                if (File.Exists(InputPath))
                {
                    ImeFilePath = InputPath;
                    InputMode = InputModes.InputFile;
                }
                else if (Directory.Exists(InputPath))
                {
                    InputDirectoryPath = InputPath;
                    InputMode = InputModes.InputDirectory;
                }
            }
            else
            {
                Ensure.ThatNotNull(InputPath, nameof(InputPath));
            }

            if (!string.IsNullOrWhiteSpace(configuration["OutputFolderPath"]))
            {
                OutputFolderPath = configuration["OutputFolderPath"];
            }
            else
            {
                Ensure.ThatNotNull(OutputFolderPath, nameof(OutputFolderPath));
            }
        }
    }
}
