using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMolecules.Bricks
{
/// <summary>
    /// Provides stable benchmark cases for central Bricks capabilities.
    /// </summary>
    public static class BrickBuiltInBenchmarkCases
    {
        /// <summary>Built-in benchmark case for rule evaluation.</summary>
        public static BrickBenchmarkCase RuleEvaluation => Case(
            "bricks.rule-evaluation",
            "Rule evaluation",
            BrickBenchmarkSubject.RuleEvaluation,
            1000,
            TimeSpan.FromTicks(2500));

        /// <summary>Built-in benchmark case for role resolution.</summary>
        public static BrickBenchmarkCase RoleResolution => Case(
            "bricks.role-resolution",
            "Role resolution",
            BrickBenchmarkSubject.RoleResolution,
            1000,
            TimeSpan.FromTicks(3000));

        /// <summary>Built-in benchmark case for policy composition.</summary>
        public static BrickBenchmarkCase PolicyComposition => Case(
            "bricks.policy-composition",
            "Policy composition",
            BrickBenchmarkSubject.PolicyComposition,
            100,
            TimeSpan.FromTicks(20000));

        /// <summary>Built-in benchmark case for violation projection.</summary>
        public static BrickBenchmarkCase ViolationProjection => Case(
            "bricks.violation-projection",
            "Violation state projection",
            BrickBenchmarkSubject.ViolationProjection,
            1000,
            TimeSpan.FromTicks(5000));

        /// <summary>Built-in benchmark case for runtime dependency evaluation.</summary>
        public static BrickBenchmarkCase RuntimeDependencyEvaluation => Case(
            "bricks.runtime-dependency-evaluation",
            "Runtime dependency evaluation",
            BrickBenchmarkSubject.RuntimeDependencyEvaluation,
            1000,
            TimeSpan.FromTicks(5000));

        /// <summary>Built-in benchmark case for report serialization.</summary>
        public static BrickBenchmarkCase ReportSerialization => Case(
            "bricks.report-serialization",
            "Report serialization",
            BrickBenchmarkSubject.ReportSerialization,
            100,
            TimeSpan.FromTicks(50000));

        /// <summary>All built-in benchmark cases in stable output order.</summary>
        public static IReadOnlyList<BrickBenchmarkCase> All => new[]
        {
            RuleEvaluation,
            RoleResolution,
            PolicyComposition,
            ViolationProjection,
            RuntimeDependencyEvaluation,
            ReportSerialization
        };

        private static BrickBenchmarkCase Case(
            string id,
            string displayName,
            BrickBenchmarkSubject subject,
            int operationCount,
            TimeSpan budget) =>
            new BrickBenchmarkCase(
                id,
                displayName,
                subject,
                operationCount,
                new BrickBenchmarkBudget(budget, "Central Bricks performance budget."));
    }
}
