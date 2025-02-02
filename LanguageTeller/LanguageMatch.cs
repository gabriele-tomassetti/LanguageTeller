using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageTeller
{    
    /// <summary>
    /// Represents a single language match.
    /// </summary>
    public struct LanguageMatch
    {
        /// <summary>
        /// The percentage of the input text which matches this language
        /// </summary>
        public float Percentage { get; private set; }

        /// <summary>
        /// Language probability
        /// </summary>
        public float Probability { get; private set; }
        
        /// <summary>
        /// Language label
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// A language has been found
        /// </summary>
        public bool Found { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LanguageMatch(float probability, string language)
        {
            Probability = probability;
            Language = language;
            Percentage = 1.0f;
            Found = true;
        }

        /// <summary>
        /// Constructor for when no match has been found
        /// </summary>
        public LanguageMatch(bool found)
        {
            Probability = 0.0f;
            Language = "";
            Percentage = 0.0f;
            Found = found;
        }

        /// <summary>
        /// Constructor for when multiple matches are returned
        /// </summary>
        public LanguageMatch(float probability, string language, float percentage)
        {
            Probability = probability;
            Language = language;
            Percentage = percentage;
            Found = true;
        }
    }
}
