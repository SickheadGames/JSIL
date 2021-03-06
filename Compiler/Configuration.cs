﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler {
    [Serializable]
    public class Configuration : Translator.Configuration {
        [Serializable]
        public sealed class SolutionBuildConfiguration {
            public string Configuration;
            public string Platform;
            public string Target;
            public string LogVerbosity;

            public readonly List<string> ExtraOutputs = new List<string>();

            public void MergeInto (SolutionBuildConfiguration result) {
                if (Configuration != null)
                    result.Configuration = Configuration;

                if (Platform != null)
                    result.Platform = Platform;

                if (Target != null)
                    result.Target = Target;

                if (LogVerbosity != null)
                    result.LogVerbosity = LogVerbosity;

                result.ExtraOutputs.AddRange(ExtraOutputs);
            }
        }

        public string[] ContributingPaths = new string[0];
        public string Path;

        public readonly SolutionBuildConfiguration SolutionBuilder = new SolutionBuildConfiguration();

        public bool? AutoLoadConfigFiles;
        public bool? UseLocalProxies;
        public bool? ReuseTypeInfoAcrossAssemblies;
        public string OutputDirectory;
        public string FileOutputDirectory;
        public string Profile;
        public Dictionary<string, object> ProfileSettings = new Dictionary<string, object>();
        public Dictionary<string, string> CustomVariables = new Dictionary<string, string>();

        public void MergeInto (Configuration result) {
            base.MergeInto(result);

            if (AutoLoadConfigFiles.HasValue)
                result.AutoLoadConfigFiles = AutoLoadConfigFiles;
            if (UseLocalProxies.HasValue)
                result.UseLocalProxies = UseLocalProxies;
            if (ReuseTypeInfoAcrossAssemblies.HasValue)
                result.ReuseTypeInfoAcrossAssemblies = ReuseTypeInfoAcrossAssemblies;
            if (OutputDirectory != null)
                result.OutputDirectory = OutputDirectory;
            if (FileOutputDirectory != null)
                result.FileOutputDirectory = FileOutputDirectory;
            if (Profile != null)
                result.Profile = Profile;
            if (Path != null)
                result.Path = Path;

            foreach (var kvp in ProfileSettings)
                result.ProfileSettings[kvp.Key] = kvp.Value;

            foreach (var kvp in CustomVariables)
                result.CustomVariables[kvp.Key] = kvp.Value;

            SolutionBuilder.MergeInto(result.SolutionBuilder);

            result.ContributingPaths = result.ContributingPaths.Concat(ContributingPaths).ToArray();
        }

        public Configuration Clone () {
            var result = new Configuration();
            MergeInto(result);
            return result;
        }

        private Func<string> BindCustomVariable (string key) {
            return () => this.CustomVariables[key];
        }

        public VariableSet ApplyTo (VariableSet variables) {
            var result = variables.Clone();

            foreach (var kvp in CustomVariables)
                result[kvp.Key] = BindCustomVariable(kvp.Key);

            result["CurrentDirectory"] = () => Environment.CurrentDirectory;
            result["ConfigDirectory"] = () => Path;
            result["OutputDirectory"] = () => OutputDirectory;
            result["FileOutputDirectory"] = () => FileOutputDirectory;
            result["Profile"] = () => Profile;

            result["Configuration"] = () => SolutionBuilder.Configuration;
            result["Platform"] = () => SolutionBuilder.Platform;
            result["Target"] = () => SolutionBuilder.Target;

            return result;
        }
    }
}
