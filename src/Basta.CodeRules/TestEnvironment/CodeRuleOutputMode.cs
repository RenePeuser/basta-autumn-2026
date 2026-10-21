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
        public static CodeRuleOutputMode OutputMode { get; set; } = ResolveOutputModeFromEnvironment();

        private static CodeRuleOutputMode ResolveOutputModeFromEnvironment()
        {
            // Priority: CodeRuleSettings__OutputMode > TestSdkSettings__OutputMode > default (Human)
            var codeRuleMode = Environment.GetEnvironmentVariable("CodeRuleSettings__OutputMode");
            if (!string.IsNullOrWhiteSpace(codeRuleMode))
            {
                return ParseOutputMode(codeRuleMode);
            }

            var testSdkMode = Environment.GetEnvironmentVariable("TestSdkSettings__OutputMode");
            if (!string.IsNullOrWhiteSpace(testSdkMode))
            {
                return ParseOutputMode(testSdkMode);
            }

            return CodeRuleOutputMode.Human;
        }

        private static CodeRuleOutputMode ParseOutputMode(string value)
        {
            return string.Equals(value, "ai", StringComparison.OrdinalIgnoreCase)
                ? CodeRuleOutputMode.Ai
                : CodeRuleOutputMode.Human;
        }
    }
}
