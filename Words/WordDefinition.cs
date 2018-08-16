using RussianWordScraper.Util;
using System;
using System.Collections.Generic;

namespace Words
{
    public class WordDefinition
    {
        public WordDefinition()
        {
            Tags = new List<string>();
            WordForms = new List<WordForm>();
            Translations = new List<string>();
            RelatedWords = new List<RelatedWord>();
        }
        public List<WordForm> WordForms { get; set; }
        public string WordType { get; set; }
        public int Rank { get; set; }
        public WordForm Word { get; set; }
        public List<string> Translations { get; set; }
        public List<RelatedWord> RelatedWords { get; set; }
        public List<string> Tags { get; set; }

        public int Score { get; set; }
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(WordType))
                    return $"{Word.Word}";
                else
                    return $"{Word.Word}-{WordType}";
            }
        }

        public void AddWordForm(string word, string description)
        {

            if (word.Contains(","))
            {
                word = word.Split(",")[0];
            }
            var wordForm = new WordForm
            {
                Word = word.Trim().RemoveStressMarks(),
                StressedWord = word.Trim()
            };
        }
    }


    public class WordForm
    {
        public WordForm()
        {
            AudioSources = new List<AudioSource>();
            Samples = new List<Sample>();
        }
        public string Word { get; set; }
        public string StressedWord { get; set; }

        public string FormDescription { get; set; }

        public List<AudioSource> AudioSources { get; set; }
        public List<Sample> Samples { get; set; }

        public int Score { get; set; }
        public string Key
        {
            get
            {
                if (!string.IsNullOrEmpty(FormDescription))
                    return $"{Word}-{FormDescription.Replace(" ", "-")}";
                else
                    return Word;
            }
        }
    }

    public class Sample
    {
        public string SampleText { get; set; }
        public string Translation { get; set; }
        public AudioSource AudioSource { get; set; }
        public AudioSource TranslationAudioSource { get; set; }
        public bool Ignore { get; set; }
        public int Score { get; set; }
        public string Key
        {
            get
            {
                if (AudioSource != null && AudioSource.Uri != null)
                    return AudioSource.Key;
                else
                    return Translation.ToLower().Replace(" ", "-");
            }
        }
    }

    public class AudioSource
    {
        public bool Ignore { get; set; }
        public Uri Uri { get; set; }
        public int Score { get; set; }
        public string Key
        {
            get
            {
                if (Uri != null)
                    return System.IO.Path.GetFileNameWithoutExtension(Uri.LocalPath);
                else
                    return null;
            }
        }
    }

    public class RelatedWord
    {
        public string Word { get; set; }
        public string Relationship { get; set; }
        public int Score { get; set; }
    }
}