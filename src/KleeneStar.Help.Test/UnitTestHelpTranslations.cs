using KleeneStar.Help.WebSeed;
using KleeneStar.Model;
using System.Text;
using System.Text.RegularExpressions;

namespace KleeneStar.Help.Test
{
    /// <summary>
    /// Contains unit tests for the language resources of the help: every text of the pages, the
    /// template and the workspace is shipped in English and German, exactly once, and a
    /// translation keeps the inline forms of its text.
    /// </summary>
    /// <remarks>
    /// A missing translation is no error anywhere in the product: the page is written in English
    /// instead. A duplicated key is none either: the first definition wins. So the tests read the
    /// files as lists of entries and print the lines that are missing.
    /// </remarks>
    [Collection("NonParallelTests")]
    public class UnitTestHelpTranslations
    {
        /// <summary>
        /// The languages the help is shipped in.
        /// </summary>
        public static readonly TheoryData<string> Cultures = ["en", "de"];

        /// <summary>
        /// The inline forms a translation keeps: page references, bold runs, code runs and the
        /// placeholders of the fixed texts.
        /// </summary>
        private static readonly Regex Reference = new(@"\[\[[A-Za-z][A-Za-z0-9]*-\d+\]\]", RegexOptions.Compiled);
        private static readonly Regex Placeholder = new(@"\{\d+\}", RegexOptions.Compiled);

        /// <summary>
        /// Verifies that every text has an entry in the language file, and prints the lines the
        /// file lacks.
        /// </summary>
        /// <param name="culture">The language.</param>
        [Theory]
        [MemberData(nameof(Cultures))]
        public void EveryTextIsTranslated(string culture)
        {
            var translations = Read(culture);
            var missing = HelpText.All()
                .Where(x => !translations.ContainsKey(HelpText.LocalKey(x)))
                .Select(x => HelpText.LocalKey(x) + "=" + x)
                .ToList();

            Assert.True(missing.Count == 0, $"{missing.Count} texts lack a translation in '{culture}':\n" + string.Join("\n", missing));
        }

        /// <summary>
        /// Verifies that the English file says what the code says.
        /// </summary>
        [Fact]
        public void TheEnglishFileRepeatsTheCode()
        {
            var english = Read("en");

            foreach (var text in HelpText.All())
            {
                if (english.TryGetValue(HelpText.LocalKey(text), out var value))
                {
                    Assert.Equal(text, value);
                }
            }
        }

        /// <summary>
        /// Verifies that no two texts fold into the same key and that no file defines a key
        /// twice or carries a text the help no longer uses.
        /// </summary>
        /// <param name="culture">The language.</param>
        [Theory]
        [MemberData(nameof(Cultures))]
        public void EveryKeyIsOneTextAndUsed(string culture)
        {
            var texts = HelpText.All();
            var folded = texts.GroupBy(HelpText.LocalKey).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

            Assert.Empty(folded);

            var entries = Entries(culture).Where(x => x.Key.StartsWith(HelpText.Prefix, StringComparison.Ordinal)).ToList();
            var duplicates = entries.GroupBy(x => x.Key).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            var used = texts.Select(HelpText.LocalKey).ToHashSet(StringComparer.Ordinal);
            var stale = entries.Select(x => x.Key).Where(x => !used.Contains(x)).ToList();

            Assert.Empty(duplicates);
            Assert.True(stale.Count == 0, $"stale keys in '{culture}':\n" + string.Join("\n", stale));
        }

        /// <summary>
        /// Verifies that a translation keeps the page references, the bold and code runs and
        /// the placeholders of its text.
        /// </summary>
        /// <param name="culture">The language.</param>
        [Theory]
        [MemberData(nameof(Cultures))]
        public void TranslationsKeepTheInlineForms(string culture)
        {
            var translations = Read(culture);

            foreach (var text in HelpText.All())
            {
                if (!translations.TryGetValue(HelpText.LocalKey(text), out var translated))
                {
                    continue;
                }

                Assert.True(Forms(text).SetEquals(Forms(translated)), $"'{translated}' does not keep the references and placeholders of '{text}'");
                Assert.True(Count(text, "**") == Count(translated, "**"), $"'{translated}' does not keep the bold runs of '{text}'");
                Assert.True(Count(text, "`") == Count(translated, "`"), $"'{translated}' does not keep the code runs of '{text}'");
            }
        }

        /// <summary>
        /// Verifies that a page built in German carries the German texts, including the fixed
        /// texts of the template, and still references its pages.
        /// </summary>
        [Fact]
        public void APageIsBuiltInGerman()
        {
            var translate = HelpText.For(Read("de"));
            var page = HelpSeeder.Pages.Single(x => x.Key == "HELP-3");
            var document = HelpDocument.Build(page, "1.2.3", translate);

            Assert.Contains(System.Text.Json.JsonSerializer.Serialize(translate(page.Summary))[1..^1], document);
            Assert.Contains(System.Text.Json.JsonSerializer.Serialize(translate(page.Sections[0].Heading))[1..^1], document);
            Assert.NotEqual(page.Summary, translate(page.Summary));
            Assert.DoesNotContain("Help page template", document);
            Assert.Equal(HelpDocument.Build(page, "1.2.3"), HelpDocument.Build(page, "1.2.3", HelpText.English));
        }

        /// <summary>
        /// Verifies that the seed writes the help in the language it is given, and that an
        /// untouched page follows when the language changes.
        /// </summary>
        [Fact]
        public async Task SeedWritesAndFollowsTheLanguage()
        {
            var german = HelpText.For(Read("de"));

            await using var db = InMemoryDbContextFactory.Create($"SeedGerman_{Guid.NewGuid()}");

            await KleeneStarDbSeeder.SeedAsync(db);
            Assert.True(await HelpSeeder.SeedAsync(db, german));

            var page = HelpSeeder.Pages[2];

            Assert.Equal(german(HelpSeeder.WorkspaceName), db.Workspaces.Single(x => x.Id == HelpSeeder.WorkspaceId).Name);
            Assert.Equal(german(page.Summary), db.Objects.Single(x => x.Id == page.Id).Summary);

            await HelpSeeder.SeedAsync(db, HelpText.English);

            Assert.Equal(page.Summary, db.Objects.Single(x => x.Id == page.Id).Summary);
            Assert.Equal(HelpDocument.Build(page, HelpSeeder.Version), db.Objects.Single(x => x.Id == page.Id).Description);
        }

        /// <summary>
        /// Returns the references and placeholders of a text.
        /// </summary>
        private static HashSet<string> Forms(string text)
        {
            return [.. Reference.Matches(text).Select(x => x.Value).Concat(Placeholder.Matches(text).Select(x => x.Value))];
        }

        /// <summary>
        /// Counts the occurrences of a marker in a text.
        /// </summary>
        private static int Count(string text, string marker)
        {
            var count = 0;

            for (var i = text.IndexOf(marker, StringComparison.Ordinal); i >= 0; i = text.IndexOf(marker, i + marker.Length, StringComparison.Ordinal))
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// Returns the translations of a language file, the first definition of a key winning.
        /// </summary>
        private static Dictionary<string, string> Read(string culture)
        {
            var translations = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var (key, value) in Entries(culture))
            {
                translations.TryAdd(key, value);
            }

            return translations;
        }

        /// <summary>
        /// Returns the entries of an embedded language file in their order.
        /// </summary>
        private static List<(string Key, string Value)> Entries(string culture)
        {
            var assembly = typeof(HelpSeeder).Assembly;
            var name = assembly.GetManifestResourceNames().Single(x => x.EndsWith(".Internationalization." + culture, StringComparison.Ordinal));

            using var stream = assembly.GetManifestResourceStream(name);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var entries = new List<(string, string)>();

            while (reader.ReadLine() is { } line)
            {
                var trimmed = line.Trim();
                var separator = trimmed.IndexOf('=');

                if (trimmed.Length == 0 || trimmed.StartsWith('#') || separator < 0)
                {
                    continue;
                }

                entries.Add((trimmed[..separator].Trim(), trimmed[(separator + 1)..].Trim()));
            }

            return entries;
        }
    }
}
