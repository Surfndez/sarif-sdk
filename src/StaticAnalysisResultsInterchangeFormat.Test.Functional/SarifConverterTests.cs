﻿// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using Microsoft.SecurityDevelopmentLifecycle.Shared.Test.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Converters
{
    [TestClass]
    [DeploymentItem(SarifConverterTests.TestDirectory, SarifConverterTests.TestDirectory)]
    public class SarifConverterTests
    {
        public const string TestDirectory = "SarifConverterTestData";

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void AndroidStudioConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.AndroidStudio);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void ApiScanConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.ApiScan, "*.csv");
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void ClangAnalyzerConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.ClangAnalyzer);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CppCheckConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.CppCheck);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void FortifyConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.Fortify);
        }

        [TestMethod]
        [TestCategory(TestCategories.OnDemand)]
        public void FxCopConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.FxCop);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void PolicheckConverter_EndToEnd()
        {
            BatchRunConverter(ToolFormat.Policheck);
        }

        private readonly ToolFormatConverter converter = new ToolFormatConverter();

        private void BatchRunConverter(ToolFormat tool)
        {
            BatchRunConverter(tool, "*.xml");
        }

        private void BatchRunConverter(ToolFormat tool, string inputFilter)
        {
            var sb = new StringBuilder();

            string toolName = Enum.GetName(typeof(ToolFormat), tool);
            string testDirectory = SarifConverterTests.TestDirectory + "\\" + toolName;
            string[] testFiles = Directory.GetFiles(testDirectory, inputFilter);

            foreach (string file in testFiles)
            {
                RunConverter(sb, tool, file);
            }

            if (sb.Length == 0)
            {
                // Test passes
                return;
            }

            string rebaselineMessage = "If the actual output is expected, generate new baselines for {0} by executing `UpdateBaselines.ps1 {0}` from a CBT command prompt, or `UpdateBaselines.ps1` to update baselines for all tools.";
            sb.AppendLine(String.Format(CultureInfo.CurrentCulture, rebaselineMessage, toolName));
            Assert.Fail(sb.ToString());
        }

        private void RunConverter(StringBuilder sb, ToolFormat tool, string inputFileName)
        {
            string expectedFileName = inputFileName + ".sarif";
            string generatedFileName = inputFileName + ".actual.sarif";

            try
            {
                this.converter.ConvertToStandardFormat(tool, inputFileName, generatedFileName, ToolFormatConversionOptions.OverwriteExistingOutputFile | ToolFormatConversionOptions.PrettyPrint);
            }
            catch (Exception ex)
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "The converter {0} threw an exception for input \"{1}\".", tool, inputFileName));
                sb.AppendLine(ex.ToString());
                return;
            }

            string expectedSarif = File.ReadAllText(expectedFileName);
            string actualSarif = File.ReadAllText(generatedFileName);

            if (expectedSarif == actualSarif)
            {
                // Test passes
                return;
            }

            string errorMessage = "The output of the {0} converter did not match for input {1}.";
            sb.AppendLine(String.Format(CultureInfo.CurrentCulture, errorMessage, tool, inputFileName));
            sb.AppendLine("Check differences with:");
            sb.AppendLine(GenerateDiffCommand(expectedFileName, generatedFileName));
        }

        private string GenerateDiffCommand(string expected, string actual)
        {
            expected = Path.GetFullPath(expected);
            actual = Path.GetFullPath(actual);

            string beyondCompare = TryFindBeyondCompare();
            if (beyondCompare != null)
            {
                return String.Format(CultureInfo.InvariantCulture, "\"{0}\" \"{1}\" \"{2}\" /title1=Expected /title2=Actual", beyondCompare, expected, actual);
            }

            return String.Format(CultureInfo.InvariantCulture, "tfsodd \"{0}\" \"{1}\"", expected, actual);
        }

        private static string TryFindBeyondCompare()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            for (int idx = 4; idx >= 3; --idx)
            {
                string beyondComparePath = String.Format(CultureInfo.InvariantCulture, "{0}\\Beyond Compare {1}\\BComp.exe", programFiles, idx);
                if (File.Exists(beyondComparePath))
                {
                    return beyondComparePath;
                }
            }

            string beyondCompare2Path = programFiles + "\\Beyond Compare 2\\BC2.exe";
            if (File.Exists(beyondCompare2Path))
            {
                return beyondCompare2Path;
            }

            return null;
        }
    }
}