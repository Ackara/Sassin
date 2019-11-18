using Microsoft.Build.Framework;
using System.IO;

namespace Acklann.Sassin.MSBuild
{
    public class CompileSassTask : ITask
    {
        public CompileSassTask()
        {
            Suffix = string.Empty;
            Minify = GenerateSourceMaps = AddSourceComments = true;
        }

        [Required]
        public string ProjectDirectory { get; set; }

        public bool Minify { get; set; }
        public string Suffix { get; set; }
        public string OutputDirectory { get; set; }

        public string SourceMapDirectory { get; set; }
        public bool GenerateSourceMaps { get; set; }
        public bool AddSourceComments { get; set; }

        public bool Execute()
        {
            if (!Directory.Exists(ProjectDirectory)) throw new DirectoryNotFoundException($"Could not find directory at '{ProjectDirectory}'.");

            NodeJS.Install();

            var options = new CompilerOptions
            {
                Minify = Minify,
                Suffix = Suffix,
                OutputDirectory = OutputDirectory,
                AddSourceComments = AddSourceComments,
                GenerateSourceMaps = GenerateSourceMaps,
                SourceMapDirectory = SourceMapDirectory
            };

            foreach (string sassFile in SassCompiler.FindFiles(ProjectDirectory))
            {
                CompilerResult result = SassCompiler.Compile(sassFile, options);

                if (result.Success) LogMessage(result);

                foreach (CompilerError err in result.Errors) LogCompilerError(err);
            }

            return true;
        }

        private void LogMessage(CompilerResult result)
        {
        }

        private void LogMessage(string format, MessageImportance level = MessageImportance.Normal)
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                format,
                string.Empty,
                nameof(CompileSassTask),
                level));
        }

        private void LogCompilerError(CompilerError error)
        {
            switch (error.Severity)
            {
                case ErrorSeverity.Error:
                    BuildEngine.LogErrorEvent(new BuildErrorEventArgs(
                        string.Empty,
                        $"{error.StatusCode}",
                        error.File,
                        error.Line,
                        error.Column,
                        0, 0,
                        error.Message,
                        string.Empty,
                        nameof(CompileSassTask)));
                    break;

                case ErrorSeverity.Warning:
                    BuildEngine.LogWarningEvent(new BuildWarningEventArgs(
                        string.Empty,
                        $"{error.StatusCode}",
                        error.File,
                        error.Line,
                        error.Column,
                        0, 0,
                        error.Message,
                        string.Empty,
                        nameof(CompileSassTask)));
                    break;
            }
        }

        #region ITask

        public ITaskHost HostObject { get; set; }

        public IBuildEngine BuildEngine { get; set; }

        #endregion ITask
    }
}