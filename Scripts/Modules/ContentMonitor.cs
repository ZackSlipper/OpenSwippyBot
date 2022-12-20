using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using DSharpPlus;
using Emzi0767.Utilities;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace SwippyBot
{
    public class ContentMonitor
    {
        Bot bot;
        ContentScanTerms terms;
        SimilarCharacterTransformations transformations;

        public ContentMonitor(Bot bot)
        {
            this.bot = bot;
            terms = new ContentScanTerms();
            transformations = new SimilarCharacterTransformations();

            bot.Client.MessageCreated += MessageCreated;
            bot.Client.MessageUpdated += MessageUpdated;
        }

        async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            try
            {
                if (e == null || e.Message == null || e.Message.Author.IsBot)
                    return;

                await ScanContent(e.Message);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
        {
            try
            {
                if (e == null || e.Message == null || e.Message.Author == null || e.Message.Author.IsBot)
                    return;

                await ScanContent(e.Message);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        Task ScanContent(DiscordMessage message)
        {
            List<char> baseContent = new List<char>(message.Content.ToLower());
            if (string.IsNullOrWhiteSpace(message.Content) && message.Stickers.Count > 0)
                baseContent = new List<char>(new string(message.Stickers[0].Name.ToLower().Where(x => char.IsLetterOrDigit(x)).ToArray()).Replace("w", "").Replace(Environment.NewLine, ""));

            //Transform text

            //Transformed content
            List<char> transformedContent = new List<char>(baseContent);
            bool foundTerm;
            for (int i = 0; i < transformedContent.Count; i++)
            {
                foreach (CharTransformation trans in transformations.Transformations)
                {
                    //Replace chars
                    if (trans.SimilarChars.Contains(transformedContent[i]))
                        transformedContent[i] = trans.Char;

                    //Replace strings
                    foreach (string transString in trans.SimilarStrings)
                    {
                        foundTerm = true;
                        for (int j = 0; j < transString.Length; j++)
                        {
                            if (i + j >= transformedContent.Count || transformedContent[i + j] != transString[j])
                            {
                                foundTerm = false;
                                break;
                            }
                        }

                        if (foundTerm)
                        {
                            transformedContent.RemoveRange(i, transString.Length);
                            transformedContent.Insert(i, trans.Char);
                        }
                    }
                }
            }

            //No Spaces
            List<char> transNoSpace = RemoveSpaces(new List<char>(transformedContent));

            //Only Letters
            List<char> transOnlyLetters = FilterPunctuation(new List<char>(transformedContent));


            //Scan for terms
            bool found = false;
            foreach (Term term in terms.Terms)
            {
                if (term.Exact)
                    found = ScanForTerm(baseContent, term, message);
                else
                {
                    if (term.FilterPunctuation && term.FilterSpaces)
                        found = ScanForTerm(transOnlyLetters, term, message);
                    else if (!term.FilterPunctuation && !term.FilterSpaces)
                        found = ScanForTerm(transformedContent, term, message);
                    else if (term.FilterPunctuation && !term.FilterSpaces)
                        found = ScanForTerm(transOnlyLetters, term, message);
                    else if (!term.FilterPunctuation && term.FilterSpaces)
                        found = ScanForTerm(transNoSpace, term, message);
                }

                if (found)
                    return Task.CompletedTask;
            }
            
            return Task.CompletedTask;
        }

        List<char> RemoveSpaces(List<char> content)
        {
            for (int i = 0; i < content.Count; i++)
            {
                if (char.IsSeparator(content[i]))
                {
                    content.RemoveAt(i);
                    i--;
                }
            }
            return content;
        }

        List<char> FilterPunctuation(List<char> content)
        {
            for (int i = 0; i < content.Count; i++)
            {
                if (!char.IsLetter(content[i]) && !char.IsSeparator(content[i]))
                {
                    content.RemoveAt(i);
                    i--;
                }
            }
            return content;
        }

        bool ScanForTerm(List<char> content, Term term, DiscordMessage message)
        {
            bool foundMatch = false;
            int errors = 0;
            int charSkip = 0;
            char lastChar = ' ';

            for (int i = 0; i < content.Count; i++)
            {
                foundMatch = true;
                errors = 0;
                charSkip = 0;
                lastChar = ' ';

                for (int j = 0; j < term.TermText.Length; j++)
                {
                    if (i + j + errors + charSkip >= content.Count)
                    {
                        foundMatch = false;
                        break;
                    }

                    if (term.RemoveDuplicateChars && content[i + j + errors + charSkip] == lastChar)
                    {
                        j--;
                        charSkip++;
                        continue;
                    }

                    if (content[i + j + errors + charSkip] != term.TermText[j])
                    {
                        errors++;
                        j--;
                        if (errors > term.ErrorTolerance)
                        {
                            foundMatch = false;
                            break;
                        }
                    }
                    else
                        lastChar = content[i + j + errors + charSkip];
                }

                foreach (string exCheck in term.PreExceptionChecks)
                {
                    if (i - exCheck.Length >= 0)
                    {
                        bool found = true;
                        for (int j = 0; j < exCheck.Length; j++)
                        {
                            if (content[i - exCheck.Length + j] != exCheck[j])
                            {
                                found = false;
                                break;
                            }
                        }

                        if (found)
                        {
                            foundMatch = false;
                            break;
                        }
                    }
                }

                //Bot.SystemMessage($"{foundMatch}", "");

                foreach (string exCheck in term.PostExceptionChecks)
                {
                    if (i + term.TermText.Length + exCheck.Length <= content.Count)
                    {
                        bool found = true;
                        for (int j = 0; j < exCheck.Length; j++)
                        {
                            if (content[i + term.TermText.Length + j] != exCheck[j])
                            {
                                found = false;
                                break;
                            }
                        }

                        if (found)
                        {
                            foundMatch = false;
                            break;
                        }
                    }
                }

                if (foundMatch)
                {
                    term.InvokeAction(message);
                    return true;
                }
            }
            return false;
        }
    }
}