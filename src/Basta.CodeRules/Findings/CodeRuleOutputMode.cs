namespace Basta.CodeRules
{
    public enum CodeRuleOutputMode
    {
        /// <summary>Framed console output with tables, for developers.</summary>
        Human,

        /// <summary>Structured JSON, for AI agents.</summary>
        Ai
    }

    public static class CodeRuleSettings
    {
        public const string DocumentationDomain = "https://github.com/draptik/basta-autumn-2026";

        // Hint: This is just for DEMO purposes. In a real project, you would use the SDK's TestSdkSettings.OutputMode instead of this static property.
        /// <summary>
        /// <c>CodeRuleSettings__OutputMode</c> wins, then <c>TestSdkSettings__OutputMode</c> so one variable switches
        /// API tests and CodeRules together. Anything but "ai" (including the SDK's "hybrid") is <see cref="CodeRuleOutputMode.Human"/>.
        /// </summary>
        public static CodeRuleOutputMode OutputMode { get; set; } = CodeRuleOutputMode.Human;
    }
}
