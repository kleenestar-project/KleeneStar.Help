using KleeneStar.Help.WebSeed;
using System.Text.Json;
using System.Xml.Linq;
using WebExpress.WebUI.WebEditor;

namespace KleeneStar.Help.Test
{
    /// <summary>
    /// Contains unit tests for the help page template (<see cref="HelpDocument"/>) and the
    /// illustrations the pages show.
    /// </summary>
    public class UnitTestHelpDocument
    {
        /// <summary>
        /// Returns the pages as test data.
        /// </summary>
        public static TheoryData<string> PageKeys => new(HelpSeeder.Pages.Select(x => x.Key));

        /// <summary>
        /// Verifies that every page is a document of the editor and follows the template: the
        /// instruction text naming the template and help version first, then the lead with the
        /// illustration, a horizontal rule, the table of contents and one heading 1 per section,
        /// closed by a rule and the closing line.
        /// </summary>
        /// <param name="key">The key of the page.</param>
        [Theory]
        [MemberData(nameof(PageKeys))]
        public void EveryPageFollowsTheTemplate(string key)
        {
            var page = HelpSeeder.Pages.Single(x => x.Key == key);
            var json = HelpDocument.Build(page, HelpSeeder.Version);

            Assert.True(EditorState.IsState(json), "the page is a document of the editor");

            using var document = JsonDocument.Parse(json);
            var rows = document.RootElement.GetProperty("doc").GetProperty("children").EnumerateArray().ToList();

            // two rows: the instruction text alone, then the page
            Assert.Equal(2, rows.Count);
            Assert.All(rows, x => Assert.Equal("row", x.GetProperty("type").GetString()));

            var instruction = Assert.Single(rows[0].GetProperty("children").EnumerateArray()).GetProperty("children")[0].GetProperty("children")[0];
            Assert.Equal("atom", instruction.GetProperty("type").GetString());
            Assert.Equal("instruction", instruction.GetProperty("attrs").GetProperty("kind").GetString());
            var text = instruction.GetProperty("attrs").GetProperty("text").GetString();
            Assert.Contains(HelpDocument.TemplateVersion, text);
            Assert.Contains(HelpSeeder.Version, text);

            var blocks = Assert.Single(rows[1].GetProperty("children").EnumerateArray()).GetProperty("children").EnumerateArray().ToList();
            string Type(JsonElement x) => x.GetProperty("type").GetString();

            // lead, role label, rule, table of contents, illustration
            Assert.Equal("p", Type(blocks[0]));
            var label = blocks[1].GetProperty("children").EnumerateArray().Single(x => Type(x) == "addon").GetProperty("attrs");
            Assert.Equal("status", label.GetProperty("name").GetString());
            Assert.Equal("info", label.GetProperty("data").GetProperty("tone").GetString());
            Assert.Equal("hr", Type(blocks[2]));
            Assert.Equal("toc", blocks[3].GetProperty("attrs").GetProperty("name").GetString());
            var image = blocks[4].GetProperty("children")[0];
            Assert.Equal("image", Type(image));
            Assert.Equal(HelpDocument.IllustrationUri(page.Illustration), image.GetProperty("attrs").GetProperty("src").GetString());
            Assert.Equal(HelpDocument.IllustrationWidth, image.GetProperty("attrs").GetProperty("width").GetString());

            // one heading 1 per section, no other heading level
            Assert.Equal(page.Sections.Count, blocks.Count(x => Type(x) == "h1"));
            Assert.DoesNotContain(blocks, x => Type(x) is "h2" or "h3");
            Assert.Equal("h1", Type(blocks[5]));

            // the related pages, then the closing rule and line with the release
            var related = blocks[^3].GetProperty("attrs");
            Assert.Equal("ks-related", related.GetProperty("name").GetString());
            Assert.Equal(HelpDocument.RelatedPages.ToString(), related.GetProperty("data").GetProperty("max").GetString());
            Assert.Equal("hr", Type(blocks[^2]));
            Assert.Contains("Version " + HelpDocument.Release(HelpSeeder.Version), blocks[^1].GetRawText());
            Assert.DoesNotContain(page.Key, blocks[^1].GetRawText());

            // the instruction is for authors: it is not in the reading text
            Assert.DoesNotContain("This instruction is shown while editing only", EditorState.ToPlainText(json));
            Assert.DoesNotContain("**", EditorState.ToPlainText(json));
        }

        /// <summary>
        /// Verifies that every page reference names a page of the help.
        /// </summary>
        [Fact]
        public void EveryReferenceNamesAHelpPage()
        {
            var keys = HelpSeeder.Pages.Select(x => x.Key).ToHashSet();

            foreach (var page in HelpSeeder.Pages)
            {
                foreach (var reference in HelpDocument.References(page))
                {
                    Assert.True(keys.Contains(reference), $"{page.Key} references {reference}, which is no help page");
                }
            }
        }

        /// <summary>
        /// Verifies the shape of the set: ids and numbers unique, a home page, chapters as roots,
        /// every topic below a chapter and labelled.
        /// </summary>
        [Fact]
        public void PagesFormATree()
        {
            var pages = HelpSeeder.Pages;

            Assert.Equal(pages.Count, pages.Select(x => x.Id).Distinct().Count());
            Assert.Equal(pages.Count, pages.Select(x => x.Number).Distinct().Count());
            Assert.Single(pages, x => x.Role == HelpPageRole.Home);

            var chapters = pages.Where(x => x.Role == HelpPageRole.Chapter).Select(x => x.Id).ToHashSet();

            Assert.All(pages.Where(x => x.Role != HelpPageRole.Topic), x => Assert.Null(x.ParentId));
            Assert.All(pages.Where(x => x.Role == HelpPageRole.Topic), x => Assert.Contains(x.ParentId.Value, chapters));
            Assert.All(pages.Where(x => x.Role == HelpPageRole.Topic), x => Assert.NotNull(x.Label));

            // a chapter is written before its pages, so the parent exists when a page is added
            for (var i = 0; i < pages.Count; i++)
            {
                if (pages[i].ParentId is { } parent)
                {
                    Assert.True(pages.Take(i).Any(x => x.Id == parent), $"{pages[i].Key} comes before its chapter");
                }
            }
        }

        /// <summary>
        /// Verifies that every illustration a page or the workspace names is embedded in the
        /// plugin, mounted under <c>assets/img</c>, and is well-formed SVG of the shared canvas.
        /// </summary>
        [Fact]
        public void EveryIllustrationIsEmbedded()
        {
            var assembly = typeof(HelpSeeder).Assembly;
            var resources = assembly.GetManifestResourceNames().ToHashSet();
            var names = HelpSeeder.Pages.Select(x => x.Illustration).Concat(["workspace", "class"]).Distinct();

            foreach (var name in names)
            {
                var resource = $"KleeneStar.Help.Assets.img\\{name}.svg";

                Assert.True(resources.Contains(resource), $"missing embedded illustration {resource}");

                using var stream = assembly.GetManifestResourceStream(resource);
                var svg = XDocument.Load(stream).Root;

                Assert.Equal("svg", svg.Name.LocalName);
                Assert.NotNull(svg.Attribute("viewBox"));
            }

            foreach (var page in HelpSeeder.Pages)
            {
                using var stream = assembly.GetManifestResourceStream($"KleeneStar.Help.Assets.img\\{page.Illustration}.svg");
                Assert.Equal("0 0 480 300", XDocument.Load(stream).Root.Attribute("viewBox").Value);
            }
        }

        /// <summary>
        /// Verifies that the inline forms of a text become marks and references.
        /// </summary>
        [Theory]
        [InlineData("0.0.1-alpha", "0.0.1")]
        [InlineData("1.2.3+abc", "1.2.3")]
        [InlineData("2.0.0", "2.0.0")]
        public void TheClosingLineNamesTheRelease(string version, string release)
        {
            Assert.Equal(release, HelpDocument.Release(version));
        }

        [Fact]
        public void TextUnderstandsBoldCodeAndReferences()
        {
            var nodes = HelpDocument.Text("Press **Create**, type `{{` or read [[HELP-8]].").ToList();
            var json = string.Join(",", nodes.Select(x => x.ToJsonString()));

            Assert.Contains("\"bold\":true", json);
            Assert.Contains("\"code\":true", json);
            Assert.Contains("\"name\":\"ks-object\"", json);
            Assert.Contains("\"key\":\"HELP-8\"", json);
            Assert.DoesNotContain("**", json);
        }
    }
}
