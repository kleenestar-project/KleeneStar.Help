using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace KleeneStar.Help.WebSeed
{
    /// <summary>
    /// Renders a <see cref="HelpPage"/> into the document format of the framework editor (the
    /// versioned JSON <c>{"version":1,"doc":…}</c>), following the help page template.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The template, top to bottom:
    /// </para>
    /// <list type="number">
    /// <item>a row of its own holding the instruction text (an editor atom of kind
    /// <c>instruction</c>) naming the template version, the help version and the rules of the
    /// template; the editor shows it to the author, the reading view leaves it out;</item>
    /// <item>a row holding the page itself, in this order: the lead; the label of the role (the
    /// framework's <c>status</c> add-on); a horizontal rule; the table of contents (the
    /// framework's <c>toc</c> add-on, built from the headings of the document, linked in the
    /// reading view); the illustration; one heading 1 per section with its paragraphs, lists and
    /// notices; the related pages (KleeneStar's <c>ks-related</c> add-on, the
    /// <see cref="RelatedPages"/> most relevant pages); a horizontal rule and the closing line
    /// naming the help version.</item>
    /// </list>
    /// <para>
    /// The document is the editor's own format rather than markup, so the editor opens a seeded
    /// page as it opens every page it saved, and the server-side readers (previews, search text)
    /// consume it through <c>EditorState</c>. Node ids are left to the editor, which mints them
    /// when it validates the document.
    /// </para>
    /// </remarks>
    public static class HelpDocument
    {
        /// <summary>
        /// The version of the help page template. Raised when the shape of the template changes.
        /// </summary>
        public const string TemplateVersion = "1.2";

        /// <summary>
        /// The number of related pages the closing list of a page shows; an author changes it per
        /// page in the properties of the add-on.
        /// </summary>
        public const int RelatedPages = 5;

        /// <summary>
        /// The width the illustration is shown at.
        /// </summary>
        public const string IllustrationWidth = "600px";

        /// <summary>
        /// The closing line, <c>{0}</c> being the help version without its prerelease suffix.
        /// </summary>
        public const string ClosingLine = "KleeneStar Help · Version {0}";

        /// <summary>
        /// The instruction text, <c>{0}</c> being the template version, <c>{1}</c> the help
        /// version and <c>{2}</c> the key.
        /// </summary>
        public const string InstructionText = "Help page template {0} · KleeneStar Help {1} · {2}. "
            + "Keep the order of the template: lead, label, horizontal rule, table of contents, illustration, "
            + "one heading 1 per topic, related pages, horizontal rule, closing line. Illustrations come from kleenestar.help/assets/img. "
            + "This instruction is shown while editing only.";

        /// <summary>
        /// The labels of the roles, as the version row shows them.
        /// </summary>
        private static readonly IReadOnlyDictionary<HelpPageRole, string> RoleLabels = new Dictionary<HelpPageRole, string>
        {
            [HelpPageRole.Home] = "Overview",
            [HelpPageRole.Chapter] = "Chapter",
            [HelpPageRole.Topic] = "Guide"
        };

        /// <summary>
        /// Returns the fixed texts of the template, shipped as internationalization keys like
        /// the texts of the pages.
        /// </summary>
        public static IEnumerable<string> FixedTexts => [ClosingLine, InstructionText, .. RoleLabels.Values];

        /// <summary>
        /// The inline forms of a text: <c>**bold**</c>, <c>`code`</c>, <c>[[KEY-1]]</c>.
        /// </summary>
        private static readonly Regex Inline = new(@"\*\*(?<bold>.+?)\*\*|`(?<code>[^`]+)`|\[\[(?<key>[A-Za-z][A-Za-z0-9]*-\d+)\]\]", RegexOptions.Compiled);

        /// <summary>
        /// Renders the page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="version">The version of the help the page belongs to.</param>
        /// <param name="translate">The translator of the language the page is written in, or
        /// <c>null</c> for English.</param>
        /// <returns>The serialized editor document.</returns>
        public static string Build(HelpPage page, string version, Func<string, string> translate = null)
        {
            ArgumentNullException.ThrowIfNull(page);

            translate ??= HelpText.English;

            var key = page.Key;
            page = page.Localize(translate);

            var content = new List<JsonNode>
            {
                Block("p", Text(page.Lead)),
                Block("p", Plain(" "), Status(translate(RoleLabels[page.Role]), "info")),
                Node("hr", null),
                Addon("toc", new JsonObject()),
                Block("p", Image(page))
            };

            foreach (var section in page.Sections)
            {
                content.Add(Block("h1", Text(section.Heading)));

                foreach (var block in section.Blocks)
                {
                    content.Add(Render(block));
                }
            }

            content.Add(Addon("ks-related", new JsonObject { ["max"] = RelatedPages.ToString(CultureInfo.InvariantCulture) }));
            content.Add(Node("hr", null));
            content.Add(Block("p", Marked(Format(translate, ClosingLine, Release(version)), "italic")));

            var blocks = new JsonArray
            {
                Node("row", null,
                    Node("region", new JsonObject { ["weight"] = 1 },
                        Block("p", Instruction(Format(translate, InstructionText, TemplateVersion, version, key), version)))),
                Node("row", null,
                    Node("region", new JsonObject { ["weight"] = 3 }, content))
            };

            var document = new JsonObject
            {
                ["version"] = 1,
                ["doc"] = new JsonObject
                {
                    ["type"] = "doc",
                    ["children"] = blocks
                }
            };

            return document.ToJsonString();
        }

        /// <summary>
        /// Returns a version without its prerelease suffix and build metadata, as the closing
        /// line names it.
        /// </summary>
        /// <param name="version">The version, for example <c>0.0.1-alpha</c>.</param>
        /// <returns>The release, for example <c>0.0.1</c>.</returns>
        public static string Release(string version)
        {
            var end = (version ?? string.Empty).IndexOfAny(['-', '+']);

            return end >= 0 ? version[..end] : version ?? string.Empty;
        }

        /// <summary>
        /// Formats a fixed text of the template in the language of the page.
        /// </summary>
        /// <param name="translate">The translator.</param>
        /// <param name="text">The English text with its placeholders.</param>
        /// <param name="args">The values of the placeholders.</param>
        /// <returns>The text.</returns>
        private static string Format(Func<string, string> translate, string text, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, translate(text), args);
        }

        /// <summary>
        /// Returns the instruction atom of the page.
        /// </summary>
        /// <param name="text">The instruction text.</param>
        /// <param name="version">The help version.</param>
        /// <returns>The instruction atom.</returns>
        private static JsonObject Instruction(string text, string version)
        {
            return new JsonObject
            {
                ["type"] = "atom",
                ["attrs"] = new JsonObject
                {
                    ["kind"] = "instruction",
                    ["text"] = text,
                    ["value"] = version,
                    ["format"] = ""
                }
            };
        }

        /// <summary>
        /// Returns the illustration of the page as an image node.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>The image node.</returns>
        private static JsonObject Image(HelpPage page)
        {
            return new JsonObject
            {
                ["type"] = "image",
                ["attrs"] = new JsonObject
                {
                    ["src"] = IllustrationUri(page.Illustration),
                    ["alt"] = page.Summary,
                    ["width"] = IllustrationWidth,
                    ["align"] = "inline"
                }
            };
        }

        /// <summary>
        /// Returns the address a document names an illustration by.
        /// </summary>
        /// <param name="name">The file name without extension.</param>
        /// <returns>The address.</returns>
        public static string IllustrationUri(string name) => KleeneStarPlugin.AssetRoot + "img/" + name + ".svg";

        /// <summary>
        /// Renders one block of a section.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns>The node.</returns>
        private static JsonObject Render(HelpBlock block) => block switch
        {
            HelpParagraph paragraph => Block("p", Text(paragraph.Text)),
            HelpSteps steps => List("ol", steps.Items),
            HelpPoints points => List("ul", points.Items),
            HelpNotice notice => Notice(notice),
            HelpChildren => Addon("ks-children", new JsonObject()),
            HelpPageList => Addon("ks-list", new JsonObject
            {
                ["workspace"] = HelpSeeder.WorkspaceKey,
                ["class"] = HelpSeeder.ClassName,
                ["kind"] = "document",
                ["state"] = "all",
                ["mine"] = "any",
                ["wql"] = "",
                ["max"] = "50"
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(block), block, "Unknown help block.")
        };

        /// <summary>
        /// Renders a list.
        /// </summary>
        /// <param name="type">The list type, <c>ol</c> or <c>ul</c>.</param>
        /// <param name="items">The items.</param>
        /// <returns>The list node.</returns>
        private static JsonObject List(string type, IEnumerable<string> items)
        {
            var list = Node(type, null);
            var children = (JsonArray)list["children"];

            foreach (var item in items)
            {
                children.Add(Node("li", null, Block("p", Text(item))));
            }

            return list;
        }

        /// <summary>
        /// Renders a notice as the framework's callout container.
        /// </summary>
        /// <param name="notice">The notice.</param>
        /// <returns>The add-on node.</returns>
        private static JsonObject Notice(HelpNotice notice)
        {
            var addon = Addon("callout", new JsonObject
            {
                ["tone"] = notice.Tone,
                ["title"] = notice.Title
            }, container: true);

            ((JsonArray)addon["children"]).Add(Block("p", Text(notice.Text)));

            return addon;
        }

        /// <summary>
        /// Returns an add-on node.
        /// </summary>
        /// <param name="name">The registered name of the add-on.</param>
        /// <param name="data">The properties.</param>
        /// <param name="inline">Whether the add-on is inline.</param>
        /// <param name="container">Whether the add-on holds content of its own.</param>
        /// <returns>The node.</returns>
        private static JsonObject Addon(string name, JsonObject data, bool inline = false, bool container = false)
        {
            return new JsonObject
            {
                ["type"] = "addon",
                ["attrs"] = new JsonObject
                {
                    ["name"] = name,
                    ["data"] = data,
                    ["inline"] = inline,
                    ["container"] = container
                },
                ["children"] = new JsonArray()
            };
        }

        /// <summary>
        /// Returns the framework's inline status label.
        /// </summary>
        /// <param name="text">The text of the label.</param>
        /// <param name="tone">The tone.</param>
        /// <returns>The node.</returns>
        private static JsonObject Status(string text, string tone)
        {
            return Addon("status", new JsonObject { ["text"] = text, ["tone"] = tone }, inline: true);
        }

        /// <summary>
        /// Returns a block whose children are inline nodes.
        /// </summary>
        /// <param name="type">The block type.</param>
        /// <param name="children">The inline nodes.</param>
        /// <returns>The node.</returns>
        private static JsonObject Block(string type, params IEnumerable<JsonNode> children) => Node(type, null, children);

        /// <summary>
        /// Returns a node.
        /// </summary>
        /// <param name="type">The node type.</param>
        /// <param name="attrs">The attributes, or <c>null</c>.</param>
        /// <param name="children">The children.</param>
        /// <returns>The node.</returns>
        private static JsonObject Node(string type, JsonObject attrs, params IEnumerable<JsonNode> children)
        {
            var array = new JsonArray();

            foreach (var child in children)
            {
                array.Add(child);
            }

            return new JsonObject
            {
                ["type"] = type,
                ["attrs"] = attrs ?? new JsonObject(),
                ["children"] = array
            };
        }

        /// <summary>
        /// Returns a text node without marks.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The node.</returns>
        private static JsonObject Plain(string text) => Marked(text);

        /// <summary>
        /// Returns a text node with the supplied marks.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="marks">The marks set to true.</param>
        /// <returns>The node.</returns>
        private static JsonObject Marked(string text, params string[] marks)
        {
            var set = new JsonObject();

            foreach (var mark in marks)
            {
                set[mark] = true;
            }

            return new JsonObject
            {
                ["type"] = "text",
                ["text"] = text,
                ["marks"] = set
            };
        }

        /// <summary>
        /// Splits a text into its inline nodes: plain runs, bold runs, code runs and references
        /// to other pages by key.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The nodes.</returns>
        public static IEnumerable<JsonNode> Text(string text)
        {
            var nodes = new List<JsonNode>();
            var position = 0;

            foreach (Match match in Inline.Matches(text ?? string.Empty))
            {
                if (match.Index > position)
                {
                    nodes.Add(Plain(text[position..match.Index]));
                }

                if (match.Groups["bold"].Success)
                {
                    nodes.Add(Marked(match.Groups["bold"].Value, "bold"));
                }
                else if (match.Groups["code"].Success)
                {
                    nodes.Add(Marked(match.Groups["code"].Value, "code"));
                }
                else
                {
                    nodes.Add(Addon("ks-object", new JsonObject { ["key"] = match.Groups["key"].Value }, inline: true));
                }

                position = match.Index + match.Length;
            }

            if (position < (text ?? string.Empty).Length)
            {
                nodes.Add(Plain(text[position..]));
            }

            return nodes;
        }

        /// <summary>
        /// Returns the keys a page references through <c>[[KEY-1]]</c>.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>The keys.</returns>
        public static IEnumerable<string> References(HelpPage page)
        {
            var texts = new List<string> { page.Lead };

            foreach (var section in page.Sections)
            {
                texts.Add(section.Heading);

                foreach (var block in section.Blocks)
                {
                    switch (block)
                    {
                        case HelpParagraph paragraph: texts.Add(paragraph.Text); break;
                        case HelpSteps steps: texts.AddRange(steps.Items); break;
                        case HelpPoints points: texts.AddRange(points.Items); break;
                        case HelpNotice notice: texts.Add(notice.Text); break;
                    }
                }
            }

            return texts
                .SelectMany(x => Inline.Matches(x ?? string.Empty))
                .Where(x => x.Groups["key"].Success)
                .Select(x => x.Groups["key"].Value)
                .Distinct();
        }
    }
}
