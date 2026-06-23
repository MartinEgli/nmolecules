namespace NMolecules.Bricks
{
    public enum BrickElementKind { Unknown = 0, Assembly = 1, Namespace = 2, Type = 3, Member = 4, Attribute = 5, DependencyRegistration = 6, ExternalReference = 7 }
    public enum BrickElementOrigin { Unknown = 0, Source = 1, Generated = 2, External = 3, Metadata = 4 }
    public enum BrickElementSource { Unknown = 0, Code = 1, Configuration = 2, Convention = 3, Inference = 4, Import = 5 }
    public enum BrickScope { Type = 0, Member = 1, Namespace = 2, Assembly = 3, Global = 4 }
    public enum BrickDependencyLayer { Static = 0, Visibility = 1, Runtime = 2, Configuration = 3 }
    public enum BrickDependencyStrength { Direct = 0, Indirect = 1, Inferred = 2 }
    public enum BrickEvidenceLevel { CompilerConfirmed = 0, AnalyzerInferred = 1, ConfigurationDeclared = 2, RuntimeInferred = 3, Unknown = 4 }
    public enum BrickDecision { Allow = 0, Deny = 1, Require = 2 }
    public enum BrickPermissionDefault { Allow = 0, Deny = 1 }
    public enum BrickEnforcementMode { Disabled = 0, Document = 1, Analyze = 2, Enforce = 3 }
    public enum BrickSeverity { Info = 0, Warning = 1, Error = 2 }
    public enum BrickViolationKind { DependencyRule = 0, RequiredDependency = 1, RoleResolution = 2, RoleCombination = 3, PolicyConfiguration = 4, Baseline = 5, Suppression = 6 }
    public enum BrickViolationState { Active = 0, Suppressed = 1, Baselined = 2, ExpiredSuppression = 3, ExpiredBaseline = 4 }
    public enum BrickPolicyImportMode { Import = 0, Extend = 1, Override = 2, Disable = 3, Narrow = 4 }
}
