﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class DoNotUseFriendlyNameAsRuleId : SarifValidationSkimmerBase
    {
        private Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF001_DoNotUseFriendlyNameAsRuleIdDescription
        };

        public override Message FullDescription => _fullDescription;

        public override ResultLevel DefaultLevel => ResultLevel.Warning;

        /// <summary>
        /// SARIF001
        /// </summary>
        public override string Id => RuleId.DoNotUseFriendlyNameAsRuleId;

        public override IEnumerable<string> MessageStringIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF001_Default)
                };
            }
        }

        protected override void Analyze(Rule rule, string rulePointer)
        {
            if (rule.Id != null &&
                rule.Name != null &&
                rule.Name.Text != null &&
                rule.Id.Equals(rule.Name.Text, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(
                    rulePointer,
                    nameof(RuleResources.SARIF001_Default),
                    rule.Id);
            }
        }
    }
}
