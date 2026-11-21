using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using WebExpress.WebCore.Internationalization;

namespace KleeneStar.Help.WebSeed
{
    /// <summary>
    /// Turns the English text a help page is written in into the internationalization key it is
    /// shipped as, and resolves it in a language.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The help is written in English in the code and shipped as keys derived from the text, the
    /// way gettext uses the source string as its message id (the convention of the templates
    /// plugin): the key is the text in lower case with everything but letters and digits folded
    /// into underscores, under <c>text.</c>; a text too long for a readable key keeps its first
    /// words and gets a hash of the whole appended. The English file carries the text as its own
    /// translation, the German file its translation; the tests hold code and files in step.
    /// </para>
    /// <para>
    /// A translation keeps the inline forms of its text: <c>**bold**</c>, <c>`code`</c> and the
    /// page references <c>[[HELP-8]]</c>. A text without a translation is used as written.
    /// </para>
    /// </remarks>
    public static class HelpText
    {
        /// <summary>
        /// The prefix of every derived key, inside the plugin's namespace.
        /// </summary>
        public const string Prefix = "text.";

        /// <summary>
        /// The longest slug kept as it is; a longer one is shortened and hashed.
        /// </summary>
        private const int MaxSlug = 48;

        /// <summary>
        /// The language the texts are written in.
        /// </summary>
        public static Func<string, string> English { get; } = text => text;

        /// <summary>
        /// Returns the fully qualified key of a text.
        /// </summary>
        /// <param name="text">The English text.</param>
        /// <returns>The key, e.g. <c>kleenestar.help:text.first_steps</c>.</returns>
        public static string Key(string text) => KleeneStarPlugin.Id + ":" + LocalKey(text);

        /// <summary>
        /// Returns the key of a text inside the plugin's language files.
        /// </summary>
        /// <param name="text">The English text.</param>
        /// <returns>The key, e.g. <c>text.first_steps</c>.</returns>
        public static string LocalKey(string text) => Prefix + Slug(text ?? string.Empty);

        /// <summary>
        /// Returns the translator of a culture: the text in that language where the language
        /// files carry it, else the text as written.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>The translator.</returns>
        public static Func<string, string> For(CultureInfo culture)
        {
            if (culture is null)
            {
                return English;
            }

            return text =>
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }

                var key = Key(text);
                var translated = I18N.Translate(culture, key);

                return string.IsNullOrWhiteSpace(translated) || translated == key || translated == LocalKey(text)
                    ? text
                    : translated;
            };
        }

        /// <summary>
        /// Returns the translator over a set of translations keyed like the language files.
        /// </summary>
        /// <param name="translations">The translations by local key.</param>
        /// <returns>The translator.</returns>
        public static Func<string, string> For(IReadOnlyDictionary<string, string> translations)
        {
            ArgumentNullException.ThrowIfNull(translations);

            return text => !string.IsNullOrWhiteSpace(text) && translations.TryGetValue(LocalKey(text), out var translated) && !string.IsNullOrWhiteSpace(translated)
                ? translated
                : text;
        }

        /// <summary>
        /// Returns every text the help is shipped with: the texts of the pages and the fixed
        /// texts of the template and the workspace.
        /// </summary>
        /// <returns>The English texts, each once.</returns>
        public static IReadOnlyList<string> All()
        {
            return [.. HelpSeeder.Pages.SelectMany(x => x.Texts())
                .Concat(HelpDocument.FixedTexts)
                .Concat(HelpSeeder.FixedTexts)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)];
        }

        /// <summary>
        /// Folds a text into the key segment.
        /// </summary>
        private static string Slug(string text)
        {
            var builder = new StringBuilder(text.Length);
            var underscore = false;

            foreach (var c in text.ToLowerInvariant())
            {
                if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
                {
                    builder.Append(c);
                    underscore = false;
                }
                else if (!underscore && builder.Length > 0)
                {
                    builder.Append('_');
                    underscore = true;
                }
            }

            var slug = builder.ToString().TrimEnd('_');

            if (slug.Length == 0)
            {
                return "x" + Hash(text);
            }

            return slug.Length <= MaxSlug
                ? slug
                : slug[..40].TrimEnd('_') + "_" + Hash(text);
        }

        /// <summary>
        /// Returns a stable hash of a text (FNV-1a over its UTF-8 bytes), as eight hex digits.
        /// </summary>
        private static string Hash(string text)
        {
            var hash = 2166136261u;

            foreach (var b in Encoding.UTF8.GetBytes(text))
            {
                hash = (hash ^ b) * 16777619u;
            }

            return hash.ToString("x8");
        }
    }
}
