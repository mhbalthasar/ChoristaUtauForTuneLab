using SyllableG2PApi.G2P;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SyllableG2PApi.Syllabler.Syllabler;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SyllableG2PApi.Syllabler
{
    public abstract class BaseSyllabler(Func<string,bool> callbackIsSymbolExists)
    {
        public string SingerPath { get; set; } = "";
        protected virtual string GetDictionaryName() => "";

        private readonly string[] wordSeparators = new[] { " ", "_" };
        private readonly string[] wordSeparator = new[] { "  " };
        public bool hasDictionary => dictionaries.ContainsKey(GetType());
        protected IG2p dictionary => dictionaries[GetType()];

        private static Dictionary<Type, IG2p> dictionaries = new Dictionary<Type, IG2p>();
        protected abstract string[] GetVowels();
        protected abstract string[] GetConsonants();
        protected virtual string[] HandleWordNotFound(string word)
        {
            return null;
        }

        protected virtual string DictionaryPreProcess(string originData) { return originData; }

        protected virtual IG2p LoadBaseDictionary()
        {
            var dictionaryText = "";
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var fileName = GetDictionaryName();
                var filePath=Path.Combine(path,"Dicts",fileName);
                if (File.Exists(filePath))
                {
                    dictionaryText = DictionaryPreProcess(File.ReadAllText(filePath));
                }
            }
            var builder = G2pDictionary.NewBuilder();
            var vowels = GetVowels();
            foreach (var vowel in vowels)
            {
                builder.AddSymbol(vowel, true);
            }
            var consonants = GetConsonants();
            foreach (var consonant in consonants)
            {
                builder.AddSymbol(consonant, false);
            }
            builder.AddEntry("a", new string[] { "a" });
            ParseDictionary(dictionaryText, builder);
            return builder.Build();
        }
        protected virtual void ParseDictionary(string dictionaryText, G2pDictionary.Builder builder)
        {
            if (dictionaryText == "") return;
            var replacements = GetDictionaryPhonemesReplacement();
            foreach (var line in dictionaryText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith(";;;"))
                {
                    continue;
                }
                var parts = line.Trim().Split(wordSeparator, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    continue;
                }
                string key = parts[0].ToLowerInvariant();
                var values = GetDictionaryWordPhonemes(parts[1]).Select(
                        n => replacements != null && replacements.ContainsKey(n) ? replacements[n] : n);
                lock (builder)
                {
                    builder.AddEntry(key, values);
                };
            };
        }
        protected abstract Dictionary<string, string> GetDictionaryPhonemesReplacement();
        protected virtual string[] GetDictionaryWordPhonemes(string phonemesString)
        {
            return phonemesString.Split(' ');
        }

        private void ReadDictionary()
        {
            try
            {
                var phonemeSymbols = new Dictionary<string, bool>();
                foreach (var vowel in GetVowels())
                {
                    phonemeSymbols.Add(vowel, true);
                }
                foreach (var consonant in GetConsonants())
                {
                    phonemeSymbols.Add(consonant, false);
                }
                var g2p = new G2pRemapper(
                    LoadBaseDictionary(),
                    phonemeSymbols,
                    GetDictionaryPhonemesReplacement()
                    );
                dictionaries[GetType()] = g2p;
            }
            catch (Exception ex)
            {
                //Log.Error(ex, $"Failed to read dictionary {dictionaryName}");
            }
        }
        protected bool hasLoadableDictionaryFile
        {
            get
            {
                var dictionaryName = GetDictionaryName();
                if (dictionaryName == "")
                {
                    return false;
                }
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var fileName = GetDictionaryName();
                var filePath = Path.Combine(path, "Dicts", fileName);
                if (!File.Exists(filePath))
                {
                    return false;
                }
                return true;
            }
        }
        public void InitDict(bool isAsync=false)
        {
            if (dictionaries.ContainsKey(GetType())) return;
            if (hasLoadableDictionaryFile)
            {
                if (isAsync)
                {
                    Task.Run(() =>
                    {
                        ReadDictionary();
                    });
                }else
                {
                    ReadDictionary();
                }
            }
        }




        string[] GetSymbols(Note note)
        {
            string[] getSymbolsRaw(string lyrics)
            {
                if (lyrics == null)
                {
                    return new string[0];
                }
                else return lyrics.Split(" ");
            }

            if (hasDictionary)
            {
                if (!string.IsNullOrEmpty(note.phoneme))
                {
                    return getSymbolsRaw(note.phoneme);
                }

                var result = new List<string>();
                foreach (var subword in note.lyric.Trim().ToLowerInvariant().Split(wordSeparators, StringSplitOptions.RemoveEmptyEntries))
                {
                    var subResult = dictionary.Query(subword);
                    if (subResult == null)
                    {
                        //Log.Warning($"Subword '{subword}' from word '{note.lyric}' can't be found in the dictionary");
                        subResult = HandleWordNotFound(subword);
                        if (subResult == null)
                        {
                            return null;
                        }
                    }
                    result.AddRange(subResult);
                }
                return result.ToArray();
            }
            else
            {
                return getSymbolsRaw(note.lyric);
            }
        }
        private string[] ApplyExtensions(string[] symbols, Note[] notes)
        {
            var newSymbols = new List<string>();
            var vowelIds = ExtractVowels(symbols);
            if (vowelIds.Count == 0)
            {
                // no syllables or all consonants, the last phoneme will be interpreted as vowel
                vowelIds.Add(symbols.Length - 1);
            }
            var lastVowelI = 0;
            newSymbols.AddRange(symbols.Take(vowelIds[lastVowelI] + 1));
            for (var i = 1; i < notes.Length && lastVowelI + 1 < vowelIds.Count; i++)
            {
                if (!IsSyllableVowelExtensionNote(notes[i]))
                {
                    var prevVowel = vowelIds[lastVowelI];
                    lastVowelI++;
                    var vowel = vowelIds[lastVowelI];
                    newSymbols.AddRange(symbols.Skip(prevVowel + 1).Take(vowel - prevVowel));
                }
                else
                {
                    newSymbols.Add(symbols[vowelIds[lastVowelI]]);
                }
            }
            newSymbols.AddRange(symbols.Skip(vowelIds[lastVowelI] + 1));
            return newSymbols.ToArray();
        }
        protected bool IsSyllableVowelExtensionNote(Note note)
        {
            return note.lyric.StartsWith("+~") || note.lyric.StartsWith("+*") || note.lyric.StartsWith("-*") || note.lyric.StartsWith("-~");
        }
        private List<int> ExtractVowels(string[] symbols)
        {
            var vowelIds = new List<int>();
            var vowels = GetVowels();
            for (var i = 0; i < symbols.Length; i++)
            {
                if (vowels.Contains(symbols[i]))
                {
                    vowelIds.Add(i);
                }
            }
            return vowelIds;
        }
        protected virtual Note[] HandleNotEnoughNotes(Note[] notes, List<int> vowelIds)
        {
            var newNotes = new List<Note>();
            newNotes.AddRange(notes.SkipLast(1));
            var lastNote = notes.Last();
            var position = lastNote.position;
            var notesToSplit = vowelIds.Count - newNotes.Count;
            var duration = lastNote.duration / notesToSplit / 15 * 15;
            for (var i = 0; i < notesToSplit; i++)
            {
                var durationFinal = i != notesToSplit - 1 ? duration : lastNote.duration - duration * (notesToSplit - 1);
                newNotes.Add(new Note()
                {
                    position = position,
                    duration = durationFinal
                });
                position += durationFinal;
            }

            return newNotes.ToArray();
        }
        protected (string[], int[], Note[]) GetSymbolsAndVowels(Note[] notes)
        {
            var mainNote = notes[0];
            var symbols = GetSymbols(mainNote);
            if (symbols == null)
            {
                return (null, null, null);
            }
            if (symbols.Length == 0)
            {
                symbols = new string[] { "" };
            }
            symbols = ApplyExtensions(symbols, notes);
            List<int> vowelIds = ExtractVowels(symbols);
            if (vowelIds.Count == 0)
            {
                // no syllables or all consonants, the last phoneme will be interpreted as vowel
                vowelIds.Add(symbols.Length - 1);
            }
            if (notes.Length < vowelIds.Count)
            {
                notes = HandleNotEnoughNotes(notes, vowelIds);
            }
            return (symbols, vowelIds.ToArray(), notes);
        }
        private const string FORCED_ALIAS_SYMBOL = "?";

        protected virtual Ending? MakeEnding(Note[] inputNotes)
        {
            if (inputNotes == null || inputNotes.Length == 0 || inputNotes[0].lyric.StartsWith(FORCED_ALIAS_SYMBOL))
            {
                return null;
            }

            (var symbols, var vowelIds, var notes) = GetSymbolsAndVowels(inputNotes);
            if (symbols == null || vowelIds == null || notes == null)
            {
                return null;
            }

            return new Ending()
            {
                prevV = symbols[vowelIds.Last()],
                cc = symbols.Skip(vowelIds.Last() + 1).ToArray(),
                duration = notes.Skip(vowelIds.Length - 1).Sum(n => n.duration),
                position = notes.Sum(n => n.duration)
            };
        }

        protected virtual Syllable[]? MakeSyllables(Note[] inputNotes, Ending? prevEnding, out string error)
        {
            (var symbols, var vowelIds, var notes) = GetSymbolsAndVowels(inputNotes);
            if (symbols == null || vowelIds == null || notes == null)
            {
                error = "symbols is null";
                return null;
            }
            var firstVowelId = vowelIds[0];
            if (notes.Length < vowelIds.Length)
            {
                error = $"Not enough extension notes, {vowelIds.Length - notes.Length} more expected";
                return null;
            }

            var syllables = new Syllable[vowelIds.Length];

            // Making the first syllable
            if (prevEnding.HasValue)
            {
                var prevEndingValue = prevEnding.Value;
                var beginningCc = prevEndingValue.cc.ToList();
                beginningCc.AddRange(symbols.Take(firstVowelId));

                // If we had a prev neighbour ending, let's take info from it
                syllables[0] = new Syllable()
                {
                    prevV = prevEndingValue.prevV,
                    cc = beginningCc.ToArray(),
                    v = symbols[firstVowelId],
                    duration = prevEndingValue.duration,
                    position = 0,
                    prevWordConsonantsCount = prevEndingValue.cc.Count()
                };
            }
            else
            {
                // there is only empty space before us
                syllables[0] = new Syllable()
                {
                    prevV = "",
                    cc = symbols.Take(firstVowelId).ToArray(),
                    v = symbols[firstVowelId],
                    duration = -1,
                    position = 0
                };
            }

            // normal syllables after the first one
            var noteI = 1;
            var ccs = new List<string>();
            var position = 0;
            var lastSymbolI = firstVowelId + 1;
            for (; lastSymbolI < symbols.Length & noteI < notes.Length; lastSymbolI++)
            {
                if (!vowelIds.Contains(lastSymbolI))
                {
                    ccs.Add(symbols[lastSymbolI]);
                }
                else
                {
                    position += notes[noteI - 1].duration;
                    syllables[noteI] = new Syllable()
                    {
                        prevV = syllables[noteI - 1].v,
                        cc = ccs.ToArray(),
                        v = symbols[lastSymbolI],
                        duration = notes[noteI - 1].duration,
                        position = position,
                        canAliasBeExtended = true // for all not-first notes is allowed
                    };
                    ccs = new List<string>();
                    noteI++;
                }
            }

            error = "";
            return syllables;
        }

        /// <summary>
        /// returns phoneme symbols, like, VCV, or VC + CV, or -CV, etc
        /// </summary>
        /// <returns>List of phonemes</returns>
        protected abstract List<string> ProcessSyllable(Syllable syllable);

        /// <summary>
        /// phoneme symbols for ending, like, V-, or VC-, or VC+C
        /// </summary>
        protected abstract List<string> ProcessEnding(Ending ending);

        protected bool HasOto(string lyric) => callbackIsSymbolExists(lyric);

        protected bool TryAddPhoneme(List<string> sourcePhonemes, params string[] targetPhonemes)
        {
            foreach (var phoneme in targetPhonemes)
            {
                if (HasOto(phoneme))
                {
                    sourcePhonemes.Add(phoneme);
                    return true;
                }
            }
            return false;
        }

        private Note[] LyricPhonemeAnalysis(List<string>? Lyr)
        {
            if (Lyr == null) return new Note[0];
            var nr = new Note[Lyr.Count];
            for (int i = 0; i < Lyr.Count; i++)
            {
                Match match = Regex.Match(Lyr[i], @"^(.*?)\[(.*?)\]$");
                string sLyric = Lyr[i];
                string sPhoneme = "";
                if (match.Success)
                {
                    sLyric = match.Groups[1].Value;
                    sPhoneme = match.Groups[2].Value;
                }
                nr[i] = new Note()
                {
                    lyric = sLyric,
                    phoneme = sPhoneme,
                    position = 480 * i,
                    duration = 480
                };
            }
            return nr;
        }

        public List<List<string>> SplitSyllable(List<string> currentLyric, string? prevLyric, string? nextLyric, out string error)
        {
            error = "";
            string? prev = prevLyric;
            if (currentLyric.Count == 0) return SplitSyllable("", prev, nextLyric, out error);
            var ret = new List<List<string>>();
            for(int i = 0; i < currentLyric.Count; i++)
            {
                string cur = currentLyric[i];
                string? next = i + 1 < currentLyric.Count ? currentLyric[i+1]:nextLyric;
                ret.AddRange(SplitSyllable(cur, prev, next, out error));
                prev = cur;
            }
            return ret;
        }
        public List<List<string>> SplitSyllable(string currentLyric, string? prevLyric, string? nextLyric, out string error)
        {
            return SplitSyllable(
                [currentLyric],
                prevLyric == null ? null : [prevLyric],
                nextLyric == null ? null : [nextLyric],
                out error
                );
        }
        public virtual List<List<string>> SplitSyllable(List<string>? currentLyrics, List<string>? prevLyrics, List<string>? nextLyrics, out string error)
        {
            bool noCurrentLyric = (currentLyrics == null || currentLyrics.Count == 0 || (currentLyrics.Count == 1 && (currentLyrics[0] == "R" || prevLyrics[0] == "")));
            bool noPrevLyric = (prevLyrics == null || prevLyrics.Count == 0 || (prevLyrics.Count == 1 && (prevLyrics[0] == "R" || prevLyrics[0] == "")));
            bool noNextLyric = (nextLyrics == null || nextLyrics.Count == 0 || (nextLyrics.Count == 1 && (nextLyrics[0] == "R" || nextLyrics[0]=="")));
            if (noPrevLyric && noCurrentLyric) { error = "NoWords!"; return new List<List<string>>(); }//EMPTY;

            error = "";
            if (noCurrentLyric)
            { //TAIL

                var ending = MakeEnding(LyricPhonemeAnalysis(prevLyrics));
                if (ending == null) return new List<List<string>>();
                return new List<List<string>>() { ProcessEnding((Ending)ending) };
            }
            else
            {
                List<List<string>> ret = new List<List<string>>();
                var syl = MakeSyllables(LyricPhonemeAnalysis(currentLyrics), MakeEnding(LyricPhonemeAnalysis(prevLyrics)), out error);
                if (syl == null) return ret;
                foreach (var ly in syl) ret.Add(ProcessSyllable(ly));
                if (noNextLyric)
                {
                    //Add Ending
                    var ending = MakeEnding(LyricPhonemeAnalysis(currentLyrics));
                    if(ending!=null) ret.Add(ProcessEnding((Ending)ending));
                }
                return ret;
            }
        }
    }
}
