using System;
using System.Collections.Generic;
using System.Linq;

namespace KleeneStar.Help.WebSeed
{
    /// <summary>
    /// The role a help page plays in the page tree of the help workspace.
    /// </summary>
    public enum HelpPageRole
    {
        /// <summary>
        /// The home page of the workspace: it opens the document overview and stands outside the
        /// page tree.
        /// </summary>
        Home,

        /// <summary>
        /// A chapter: a root of the page tree that lists the pages below it.
        /// </summary>
        Chapter,

        /// <summary>
        /// A topic: one guide, one step or one question, below its chapter.
        /// </summary>
        Topic
    }

    /// <summary>
    /// One help page as the plugin ships it: identity, place in the tree, label and content.
    /// </summary>
    /// <remarks>
    /// The id and the number are fixed, so a page is recognized on every start and the pages can
    /// reference each other by key (<c>HELP-{Number}</c>). The content is written in English and
    /// rendered into the editor's document format by <see cref="HelpDocument"/>.
    /// </remarks>
    /// <param name="Id">The fixed id of the object.</param>
    /// <param name="Number">The number of the key, <c>HELP-{Number}</c>.</param>
    /// <param name="Role">The role of the page in the tree.</param>
    /// <param name="ParentId">The id of the chapter a topic belongs to; <c>null</c> for a root.</param>
    /// <param name="Label">The reserved landing label (<c>Help</c>, <c>FAQ</c>, <c>First Steps</c>), or <c>null</c>.</param>
    /// <param name="Summary">The title of the page.</param>
    /// <param name="Illustration">The file name of the illustration under <c>Assets/img</c>, without extension.</param>
    /// <param name="Lead">The introduction shown beside the illustration.</param>
    /// <param name="Sections">The sections, one heading 1 each.</param>
    public sealed record HelpPage
    (
        Guid Id,
        int Number,
        HelpPageRole Role,
        Guid? ParentId,
        string Label,
        string Summary,
        string Illustration,
        string Lead,
        IReadOnlyList<HelpSection> Sections
    )
    {
        /// <summary>
        /// Returns the object key the page is seeded under.
        /// </summary>
        public string Key => HelpSeeder.WorkspaceKey + "-" + Number;

        /// <summary>
        /// Returns every text of the page that is shipped as an internationalization key: the
        /// summary, the lead, the headings and the texts of the blocks. The landing label is a
        /// reserved name and stays as written.
        /// </summary>
        /// <returns>The English texts.</returns>
        public IEnumerable<string> Texts()
        {
            yield return Summary;
            yield return Lead;

            foreach (var section in Sections)
            {
                yield return section.Heading;

                foreach (var text in section.Blocks.SelectMany(x => x.Texts()))
                {
                    yield return text;
                }
            }
        }

        /// <summary>
        /// Returns the page in another language.
        /// </summary>
        /// <param name="translate">The translator of the language.</param>
        /// <returns>The page with every text translated.</returns>
        public HelpPage Localize(Func<string, string> translate)
        {
            ArgumentNullException.ThrowIfNull(translate);

            return this with
            {
                Summary = translate(Summary),
                Lead = translate(Lead),
                Sections = [.. Sections.Select(x => new HelpSection(translate(x.Heading), [.. x.Blocks.Select(b => b.Localize(translate))]))]
            };
        }
    }

    /// <summary>
    /// One section of a help page: a heading 1 and the blocks beneath it.
    /// </summary>
    /// <param name="Heading">The heading.</param>
    /// <param name="Blocks">The blocks of the section.</param>
    public sealed record HelpSection(string Heading, params HelpBlock[] Blocks);

    /// <summary>
    /// A block of a help section.
    /// </summary>
    /// <remarks>
    /// Text in a block understands three inline forms: <c>**bold**</c>, <c>`code`</c> and
    /// <c>[[HELP-8]]</c>, a live reference to another page by key.
    /// </remarks>
    public abstract record HelpBlock
    {
        /// <summary>
        /// Returns the texts of the block that are shipped as internationalization keys.
        /// </summary>
        /// <returns>The English texts.</returns>
        public virtual IEnumerable<string> Texts() => [];

        /// <summary>
        /// Returns the block in another language.
        /// </summary>
        /// <param name="translate">The translator of the language.</param>
        /// <returns>The translated block.</returns>
        public virtual HelpBlock Localize(Func<string, string> translate) => this;
    }

    /// <summary>
    /// A paragraph.
    /// </summary>
    /// <param name="Text">The text.</param>
    public sealed record HelpParagraph(string Text) : HelpBlock
    {
        /// <inheritdoc/>
        public override IEnumerable<string> Texts() => [Text];

        /// <inheritdoc/>
        public override HelpBlock Localize(Func<string, string> translate) => new HelpParagraph(translate(Text));
    }

    /// <summary>
    /// A numbered list of steps.
    /// </summary>
    /// <param name="Items">The steps in order.</param>
    public sealed record HelpSteps(params string[] Items) : HelpBlock
    {
        /// <inheritdoc/>
        public override IEnumerable<string> Texts() => Items;

        /// <inheritdoc/>
        public override HelpBlock Localize(Func<string, string> translate) => new HelpSteps([.. Items.Select(translate)]);
    }

    /// <summary>
    /// A list of points.
    /// </summary>
    /// <param name="Items">The points.</param>
    public sealed record HelpPoints(params string[] Items) : HelpBlock
    {
        /// <inheritdoc/>
        public override IEnumerable<string> Texts() => Items;

        /// <inheritdoc/>
        public override HelpBlock Localize(Func<string, string> translate) => new HelpPoints([.. Items.Select(translate)]);
    }

    /// <summary>
    /// A notice box (the framework's <c>callout</c> add-on).
    /// </summary>
    /// <param name="Tone">The tone: <c>info</c>, <c>success</c> or <c>warning</c>.</param>
    /// <param name="Title">The title of the box.</param>
    /// <param name="Text">The text in the box.</param>
    public sealed record HelpNotice(string Tone, string Title, string Text) : HelpBlock
    {
        /// <inheritdoc/>
        public override IEnumerable<string> Texts() => [Title, Text];

        /// <inheritdoc/>
        public override HelpBlock Localize(Func<string, string> translate) => new HelpNotice(Tone, translate(Title), translate(Text));
    }

    /// <summary>
    /// The pages directly below the page (KleeneStar's <c>ks-children</c> add-on).
    /// </summary>
    public sealed record HelpChildren : HelpBlock;

    /// <summary>
    /// A live list of every page of the help workspace (KleeneStar's <c>ks-list</c> add-on).
    /// </summary>
    public sealed record HelpPageList : HelpBlock;
}
