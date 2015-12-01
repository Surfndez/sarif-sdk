// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Converters
{
    [TestClass]
    public class FortifyConverterTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FortifyConverter_Convert_NullInput()
        {
            new FortifyConverter().Convert(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FortifyConverter_Convert_NullOutput()
        {
            using (var input = new MemoryStream())
            {
                new FortifyConverter().Convert(input, null);
            }
        }

        private struct Builder
        {
            public string RuleId;
            public string InstanceId;
            public string Category;
            public string Kingdom;
            public string Abstract;
            public string AbstractCustom;
            public string Priority;
            public FortifyPathElement PrimaryOrSink;
            public FortifyPathElement Source;
            public ImmutableArray<int> CweIds;

            public FortifyIssue ToImmutable()
            {
                return new FortifyIssue
                    (
                        ruleId: this.RuleId,
                        iid: this.InstanceId,
                        category: this.Category,
                        kingdom: this.Kingdom,
                        abs: this.Abstract,
                        abstractCustom: this.AbstractCustom,
                        priority: this.Priority,
                        primaryOrSink: this.PrimaryOrSink,
                        source: this.Source,
                        cweIds: this.CweIds
                    );
            }
        }

        private static readonly FortifyPathElement s_dummyPathElement =
            new FortifyPathElement("filePath", 1729, "int Foo::Bar(string const&) const&&");
        private static readonly FortifyPathElement s_dummyPathSourceElement =
            new FortifyPathElement("sourceFilePath", 42, null);

        private static Builder GetBasicBuilder()
        {
            return new Builder
            {
                Category = "cat",
                Kingdom = "king",
                PrimaryOrSink = FortifyConverterTests.s_dummyPathElement
            };
        }

        private static FortifyIssue GetBasicIssue()
        {
            return FortifyConverterTests.GetBasicBuilder().ToImmutable();
        }

        [TestMethod]
        public void FortifyConverter_Convert_RuleIdIsKingdomAndCategory()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(FortifyConverterTests.GetBasicIssue());
            Assert.AreEqual("cat", result.RuleId);
        }

        [TestMethod]
        public void FortifyConverter_Convert_ToolFingerprintIsIid()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.InstanceId = "a";
            Result resultA = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.AreEqual("a", resultA.ToolFingerprint);

            builder.InstanceId = null; // IID is optional
            Result resultNull = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.IsNull(resultNull.ToolFingerprint);
        }

        [TestMethod]
        public void FortifyConverter_Convert_ShortMessageIsUnset()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(FortifyConverterTests.GetBasicIssue());
            Assert.IsNull(result.ShortMessage);
        }

        [TestMethod]
        public void FortifyConverter_Convert_FullMessageFallsBackToCategoryIfNoAbstractPresent()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(FortifyConverterTests.GetBasicIssue());
            result.FullMessage.Should().Contain("cat");
        }

        [TestMethod]
        public void FortifyConverter_Convert_FullMessageUsesAbstractIfPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.Abstract = "Some abstract message";
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.AreEqual("Some abstract message", result.FullMessage);
        }

        [TestMethod]
        public void FortifyConverter_Convert_FullMessageUsesAbstractCustomIfPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.AbstractCustom = "Some abstract custom message";
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.AreEqual("Some abstract custom message", result.FullMessage);
        }

        [TestMethod]
        public void FortifyConverter_Convert_ConcatenatesAbstractsIfBothPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.Abstract = "Some abstract message";
            builder.AbstractCustom = "Some abstract custom message";
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.AreEqual("Some abstract message" + Environment.NewLine + "Some abstract custom message",
                result.FullMessage);
        }

        [TestMethod]
        public void FortifyConverter_Convert_KingdomIsInProperties()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(FortifyConverterTests.GetBasicIssue());
            result.Properties.Should().Equal(new Dictionary<string, string>
            {
                {"kingdom", "king" }
            });
        }

        [TestMethod]
        public void FortifyConverter_Convert_FillsInPriorityIfFriorityPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.IsTrue(result.Properties == null || !result.Properties.ContainsKey("priority"),
                "Priority was set to a null value.");

            builder.Priority = "HIGH";
            result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.AreEqual("HIGH", result.Properties["priority"]);
        }

        [TestMethod]
        public void FortifyConverter_Convert_FillsInCweIfPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.IsTrue(result.Properties == null || !result.Properties.ContainsKey("cwe"),
                "CWE was filled in when no CWEs were present.");

            builder.CweIds = ImmutableArray.Create(24, 42, 1729);
            result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.AreEqual("24, 42, 1729", result.Properties["cwe"]);
        }

        [TestMethod]
        public void FortifyConverter_Convert_FillsInFortifyRuleIdIfPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.IsTrue(result.Properties == null || !result.Properties.ContainsKey("fortifyRuleId"),
                "Fortify RuleID was filled in when no ruleId was present.");

            builder.RuleId = "abc";
            result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.AreEqual("abc", result.Properties["fortifyRuleId"]);
        }

        [TestMethod]
        public void FortifyConverter_Convert_UsesPrimaryAsMainLocation()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.Source = FortifyConverterTests.s_dummyPathSourceElement;
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.AreEqual(1, result.Locations.Count);
            Assert.AreEqual("filePath", result.Locations[0].IssueFile[0].Uri.ToString());
            Assert.AreEqual(new Region { StartLine = 1729 }, result.Locations[0].IssueFile[0].Region);
        }

        [TestMethod]
        public void FortifyConverter_Convert_DoesNotFillInExecutionFlowWhenOnlyPrimaryIsPresent()
        {
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(GetBasicIssue());
            Assert.IsNull(result.ExecutionFlows);
        }

        [TestMethod]
        public void FortifyConverter_Convert_FillsInExecutionFlowWhenSourceIsPresent()
        {
            Builder builder = FortifyConverterTests.GetBasicBuilder();
            builder.Source = FortifyConverterTests.s_dummyPathSourceElement;
            Result result = FortifyConverter.ConvertFortifyIssueToSarifIssue(builder.ToImmutable());
            Assert.AreEqual(1, result.ExecutionFlows.Count);
            IList<AnnotatedCodeLocation> flow = result.ExecutionFlows[0];
            Assert.AreEqual("sourceFilePath", flow[0].PhysicalLocations[0][0].Uri.ToString());
            Assert.AreEqual(new Region { StartLine = 42 }, flow[0].PhysicalLocations[0][0].Region);
            Assert.AreEqual("filePath", flow[1].PhysicalLocations[0][0].Uri.ToString());
            Assert.AreEqual(new Region { StartLine = 1729 }, flow[1].PhysicalLocations[0][0].Region);
        }
    }
}