using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace SwippyBot
{
    public class ContentScanTerms
    {
        public List<Term> Terms { get; } = new List<Term>()
        {
            new Term("anus", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new []{ "ur" }),
            new Term("arse", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new []{ "s" }, new []{ "t" }),
            new Term("ass", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new []{ "r", "w", "s", "b", "l", "t", "m", "g", "z", "y", "n", "k", "c", "d", "f", "p", "e"}, new []{ "o", "a", "b", "c", "d", "k", "r", "p", "i", "u"}),
            new Term("axwound", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("bampot", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("boob", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("bastard", false, true, true, 2, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("bitch", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("blowjob", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("bollocks", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("ballocks", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("bollox", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("carpetmuncher", false, true, true, 2, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("chinc", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("chink", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("choad", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("chode", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new [] {"a"}, new [] {"r", "w"}),
            new Term("clit", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            //new Term("cock", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("cuck", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("coochie", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("coochy", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("cooter", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new [] {"s"}),
            new Term("cum", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), null, new[]{ "in", "b" }),
            new Term("cunnie", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("cunnilingus", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("cunt", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("dago", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            //new Term("damn", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("deggo", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("dick", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("dike", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("dildo", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("dipshit", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("doochbag", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("douche", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("dyke", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("fag", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("fellatio", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("feltch", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("fuck", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("fucc", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("gooch", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("gook", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("goopchute", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("gringo", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("guido", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("handjob", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("heeb", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("hell", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new []{ "s" }, new []{ "o", "p", "d", "m", "b", "f", "k", "q", "u" }),
            new Term("honkey", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("jagoff", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("jackoff", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("jizz", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("jigaboo", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("kike", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("kooch", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("kootch", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("kunt", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("kyke", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("mick", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("milf", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("minge", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("negro", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("niga", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new[]{ "shena" }, new []{ "ns" }),
            new Term("nigger", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("niggy", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("niglet", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("nignog", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("nutsack", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("panooch", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("penis", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("piss", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            //new Term("poon", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("prick", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            //new Term("pedo", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("punanny", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("punta", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("pussy", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("pussie", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("queef", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("shit", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("scrote", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("schlong", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("shiz", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("skank", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("skeet", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("slut", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("stfu", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("stafu", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("smeg", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("splooge", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), null, new [] { "ma" }),
            new Term("tard", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new []{ "s" }, new []{ "ew" }),
            new Term("testicle", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("tits", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("titty", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("tittie", false, true, false, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            //new Term("twat", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new []{ "s" }),
            new Term("vagina", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("vajayjay", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("vjayjay", false, true, true, 1, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            //new Term("wank", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new []{  "b", "f", "h", "t", "c", "d" }, new []{ "ie", "y" }), //"Rank" ins witw speak
            new Term("whore", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            //new Term("wtf", false, true, false, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new []{ "u" }, new []{ "u" }),

            new Term("blet", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult(), new []{ "dou" }, new []{ "h" }),
            new Term("suka", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("pydar", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
            new Term("debil", false, true, true, 0, (DiscordMessage message, string term) => Bot.ContentFilter.Filter(message, term).GetAwaiter().GetResult()),
        };
    }

    public class Term
    {
        public delegate void FoundAction(DiscordMessage message, string term);

        public string TermText { get; }
        public bool Exact { get; }
        public bool FilterSpaces { get; }
        public bool FilterPunctuation { get; } //Also filters spaces
        public bool RemoveDuplicateChars { get; }
        public uint ErrorTolerance { get; }
        public string[] PreExceptionChecks { get; }
        public string[] PostExceptionChecks { get; }

        FoundAction action;

        public Term(string termText, bool filterSpaces, bool filterPunctuation, bool removeDuplicateChars, uint errorTolerance, FoundAction action, string[] preExceptionChecks = null, string[] postExceptionChecks = null)
        {
            TermText = termText;
            FilterSpaces = filterSpaces;
            FilterPunctuation = filterPunctuation;
            RemoveDuplicateChars = removeDuplicateChars;
            ErrorTolerance = errorTolerance;
            PreExceptionChecks = preExceptionChecks == null ? new string[0] : preExceptionChecks;
            PostExceptionChecks = postExceptionChecks == null ? new string[0] : postExceptionChecks;
            this.action = action;

            if (PreExceptionChecks.Length > 0 || PostExceptionChecks.Length > 0) //Without this exception checks break
                ErrorTolerance = 0;
        }

        public void InvokeAction(DiscordMessage message)
        {
            try
            {
                action?.Invoke(message, TermText);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}