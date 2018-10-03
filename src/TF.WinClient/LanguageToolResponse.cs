using System.Collections.Generic;

namespace TF.WinClient
{
    public class LanguageToolResponse
    {
        public SoftwareInfo Software { get; set; }
        public LanguageInfo Language { get; set; }

        public List<Match> Matches { get; set; }
    }

    public class SoftwareInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string BuildDate { get; set; }
        public int ApiVersion { get; set; }
        public string Status { get; set; }
        public bool Premium { get; set; }
    }

    public class LanguageInfo
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public DetectedLanguageInfo DetectedLanguage { get; set; }
    }

    public class DetectedLanguageInfo
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class Match
    {
        public string Message { get; set; }
        public string ShortMessage { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public List<Replacement> Replacements { get; set; }
        public ContextInfo Context { get; set; }
        public string Sentence { get; set; }
        public RuleInfo Rule { get; set; }
    }

    public class Replacement
    {
        public string Value { get; set; }
    }

    public class ContextInfo
    {
        public string Text { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
    }

    public class RuleInfo
    {
        public string Id { get; set; }
        public string SubId { get; set; }
        public string Description { get; set; }
        public List<Url> Urls { get; set; }
        public string IssueType { get; set; }
        public CategoryInfo Category { get; set; }
    }

    public class Url
    {
        public string Value { get; set; }
    }

    public class CategoryInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
