using System.Collections.Generic;

namespace SwippyBot
{
    public class SimilarCharacterTransformations
    {
        public List<CharTransformation> Transformations { get; } = new List<CharTransformation>()
        {
            new CharTransformation('a',
                new HashSet<char>{ '@', 'ą', 'Д' },
                new string[]{ "/\\\\", "/-\\\\", "^" }),

            new CharTransformation('b',
                new HashSet<char>{ 'ß' },
                new string[]{ "|3", "!3", "(3", "/3", ")3", "🅱️" }),

            new CharTransformation('c',
                new HashSet<char>{ '¢', '©' }, //'č', '[', '¢', '©', '<', '('
                new string[]{  }),

            new CharTransformation('d',
                new HashSet<char>{ ')' },
                new string[]{ "|)", "[)" }),

            new CharTransformation('e',
                new HashSet<char>{ 'ę', 'ė', '3', '£', '€' },
                new string[]{  }),

            new CharTransformation('f',
                new HashSet<char>{ 'ƒ' },
                new string[]{ /*"ph"*/ }), //Detects "Alphagatorz" as "fag"

            new CharTransformation('g',
                new HashSet<char>{ '6', '9' },
                new string[]{ }),

            new CharTransformation('h',
                new HashSet<char>{ '#' },
                new string[]{ "|-|", "/-/", "\\\\-\\\\" }),

            new CharTransformation('i',
                new HashSet<char>{ 'į', '1', '|', '!' },
                new string[]{ }),

            new CharTransformation('j',
                new HashSet<char>{  },
                new string[]{  }),

            new CharTransformation('k',
                new HashSet<char>{  },
                new string[]{ "|<", "|c" }),

            new CharTransformation('l',
                new HashSet<char>{  },
                new string[]{ "|_" }),

            new CharTransformation('m',
                new HashSet<char>{ },
                new string[]{ "/\\\\/\\\\", "/V\\\\", "|\\\\/|", "Ⓜ️" }),

            new CharTransformation('n',
                new HashSet<char>{  },
                new string[]{ "|\\\\|", "/\\\\/" }),

             new CharTransformation('o',
                new HashSet<char>{ '○', 'o', '0', 'ø', 'ö', 'ò', 'ó', 'ô', 'õ', 'ō', 'ő', 'œ', 'Ø', 'º', '➰', '⚪', '⚫', '⭕' },
                new string[]{ "()", "[]", "{}", "<>", "oh", ":o:", ":zero:", ":curly_loop:", ":jack_o_lantern:", ":no_mobile_phones:", "🟤", "🟢", "🟠", "🟡", "🟣", "🔴", "🔵", "🅾️", "🎃", "📵" }),

            new CharTransformation('p',
                new HashSet<char>{  },
                new string[]{ "🅿️", "🇵" }),

            new CharTransformation('q',
                new HashSet<char>{  },
                new string[]{  }),

            new CharTransformation('r',
                new HashSet<char>{ '®', 'Я' },
                new string[]{  }),

            new CharTransformation('s',
                new HashSet<char>{ 'š', '$', '§' },
                new string[]{  }),

            new CharTransformation('t',
                new HashSet<char>{ '†' },
                new string[]{  }),

            new CharTransformation('u',
                new HashSet<char>{ 'ų', 'ū' },
                new string[]{ "|_|", "(_)" }),

            new CharTransformation('v',
                new HashSet<char>{  },
                new string[]{ "\\/" }),

            new CharTransformation('w',
                new HashSet<char>{  },
                new string[]{ "\\\\/\\\\/" }),

            new CharTransformation('x',
                new HashSet<char>{ '×', '%' },
                new string[]{ "><", ")(", "}{" }),

            new CharTransformation('y',
                new HashSet<char>{ '¥' },
                new string[]{  }),

            new CharTransformation('z',
                new HashSet<char>{ 'ž', '2' },
                new string[]{  })
        };
    }

    public class CharTransformation
    {
        public char Char { get; }
        public HashSet<char> SimilarChars { get; }
        public string[] SimilarStrings { get; }

        public CharTransformation(char character, HashSet<char> similarCharacters, string[] similarStrings)
        {
            Char = character;
            SimilarChars = similarCharacters;
            SimilarStrings = similarStrings;
        }
    }
}