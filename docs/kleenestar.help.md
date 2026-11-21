![KleeneStar](https://raw.githubusercontent.com/kleenestar-project/.github/main/docs/assets/img/banner.png)

# KleeneStar Help

The plugin `KleeneStar.Help` carries the help of KleeneStar: the workspace **KleeneStar Help** (key `HELP`) with a home page, three chapters and the help pages below them, and the illustrations those pages show. It contributes no page, no endpoint and no manager. A help page is an ordinary document of the installation: searchable, versioned, commented and edited like every other one.

The plugin names the core application (`[Application<KleeneStarApplication>]`), because the framework refuses a plugin without one. The host references it in every configuration, `Release` included: the help is product content, not demonstration data.

## What is seeded

`WebSeed/HelpSeed` is an `IDatabaseSeed` (order 200, after the demo data at 100) that delegates to `HelpSeeder.SeedAsync`. It runs on every start and writes what the store does not carry yet, in the language of the installation (`WebExpress:Culture`, see *Languages*):

| Part | Content |
|------|---------|
| Workspace | `HELP`, *KleeneStar Help* (German *KleeneStar-Hilfe*), public, fixed id `HelpSeeder.WorkspaceId` |
| Class | *Help page*, object type document, renderer prose, the three standard forms without fields |
| Permissions | While the workspace carries no grant: administrators `workspace_admin_policy`, signed-in users `workspace_view_policy`. The help is curated: administrators write, everybody signed in reads. |
| Pages | `HelpSeeder.Pages`: home page `HELP-1`, chapters *First Steps* `HELP-2`, *Guides* `HELP-7`, *Frequently asked questions* `HELP-16`, each followed by its topics |
| Home page | `Workspace.HomeId` names `HELP-1` while the workspace names no home page or one that is gone |
| Labels | Every topic carries one reserved landing label: `First Steps`, `Help` or `FAQ`; the start page and the header's help menu list the help through them (`LandingLabel`) |

A store whose own workspace already carries the key `HELP` keeps it; the help stays out.

### Keys, refresh and deletion

Each page has a fixed id and a fixed number. It is seeded under `HELP-{Number}` while that key is free (also as a key alias); otherwise under the next free number of the workspace, and a reference to the original key then names another object.

A page nobody changed since it was seeded (no commit, no pending draft) is rewritten from the content of the plugin on start, so a newer plugin version and a changed installation language reach the pages it wrote. A page somebody published, or is writing, belongs to its authors and is left as it is. A deleted page returns on the next start; a page the organization does not want is archived, which takes it off the start page and the help menu (`LandingLabel` reads active objects only).

## The page template

`WebSeed/HelpDocument.Build` renders a `HelpPage` into the editor's own document format (`{"version":1,"doc":…}`), so the editor opens a seeded page as one it saved itself, and the server side readers (`EditorState`, `ProseText`) read it for previews and search. Top to bottom:

The template (version 1.2) follows the page `HELP-3` as it was laid out in the editor: two rows, each of one region.

1. **Instruction row**: a row of its own holding the editor atom of kind `instruction`, naming the template version (`HelpDocument.TemplateVersion`), the help version (`HelpSeeder.Version`, the plugin's informational version without build metadata), the key and the rules of the template. The editor shows it to the author; the reading view and the plain text leave it out.
2. **Page row**, top to bottom:
   - the **lead**;
   - the **role label**, a `status` label in the tone `info`: *Overview*, *Chapter* or *Guide*;
   - a **horizontal rule**;
   - the **table of contents**, the framework's `toc` add-on, built from the headings of the document and linked in the reading view;
   - the **illustration**, 600 pixels wide (`HelpDocument.IllustrationWidth`), by its relative address;
   - the **sections**: one heading 1 per `HelpSection`, with paragraphs, numbered steps, points and notices (the framework's `callout` container). A chapter lists its pages with KleeneStar's `ks-children` add-on, the home page lists every page of the workspace with `ks-list`;
   - the **related pages**: KleeneStar's `ks-related` add-on with `HelpDocument.RelatedPages` (5) entries, the pages the page references, the pages referencing it, the chapter and the pages beside it, the pages sharing its label; an author changes the number in the properties of the add-on;
   - a **horizontal rule** and the **closing line** *KleeneStar Help · Version 0.0.1*, naming the release without its prerelease suffix (`HelpDocument.Release`).

Text takes the colour of the theme: a document carries no text colour, so it reads in the light and the dark scheme alike. The fixed texts of the template (instruction text, role labels, closing line) are translated like the texts of the pages.

Texts understand three inline forms: `**bold**`, `` `code` `` and `[[HELP-8]]`, which becomes a live `ks-object` reference to another page.

A new help page written by hand keeps the same order. The illustrations are referenced by address, `/kleenestar/kleenestar.help/assets/img/<name>.svg` (`HelpDocument.IllustrationUri`).

## Illustrations

`Assets/img` holds one illustration per page (480 by 300, transparent) and the two square icons of the workspace and the class. They are embedded with `%(RecursiveDir)` in the logical name, so the asset manager mounts them under the plugin id: `/kleenestar/kleenestar.help/assets/img/…`. A document cannot carry an inline SVG or a `data:` address (the editor's sanitizer removes both), so the pages name the files.

All illustrations come from `tools/illustrations.mjs`, which draws every scene from one vocabulary in the colours of KleeneStar: the orange `#FF6600` of the logo, the dark `#212529` of the application header, the class icon blue `#4586DB`, the green `#198754` of the create button and the primary blue `#0D6EFD`. Every scene stands on a pale backdrop carrying the KleeneStar mark (the path of `data/icons/kleenestar.svg`) as a watermark; application windows carry the KleeneStar header (logo tile, word mark, create button, avatar); records are drawn the way the lists show them (class tile, key such as `SD-42`, summary, state); an orange shirt carries the mark. The workspace and class icons follow the core icon style: a solid 150 by 150 tile with a white pictogram. A changed or new scene is generated with `node tools/illustrations.mjs` from the repository root; the output is checked in.

## Content

The pages are written in English in the code. Every statement is checked against the behaviour of the core: button labels, dialog steps, permissions, security levels, WQL operators. A change of that behaviour is a reason to revise the page that describes it and to raise the plugin version, which reaches every untouched page on the next start.

## Languages

Every text of the help is shipped as an internationalization key derived from its English text (`WebSeed/HelpText`, the convention of the templates plugin: `kleenestar.help:text.<slug>`, a long text shortened and hashed). `Internationalization/en` repeats the code, `Internationalization/de` translates it; a translation keeps the inline forms of its text (`**bold**`, `` `code` ``, `[[HELP-n]]`, the placeholders `{0}` of the fixed texts), and button labels follow the language files of the core. Translated are the summaries, leads, headings, paragraphs, steps, points and notices of the pages, the fixed texts of the template, the workspace name and the descriptions of workspace and class. The class name *Help page* and the landing labels `First Steps`, `Help` and `FAQ` stay as written: the page list of the home page names the class by its name, and the start page finds the help by the reserved labels.

The help is data, so it is written in one language: `HelpSeed` writes it in the culture of the installation (`WebExpress:Culture`, `HelpText.For(culture)`), and a text without a translation is written in English. Changing the culture rewrites the untouched pages on the next start; a page somebody published stays in the language it was written in. A new text needs a line in both files; `UnitTestHelpTranslations` prints the missing lines.

## Tests

`KleeneStar.Help.Test`: `UnitTestHelpSeeder` (discovery, workspace, class, pages, tree, home page, labels, grants, idempotence, refresh of untouched pages only, a foreign `HELP` workspace, a taken key), `UnitTestHelpTranslations` (every text in both files, the English file repeats the code, no folded, duplicate or stale key, inline forms kept, a page built and a store seeded in German, an untouched page following the language) and `UnitTestHelpDocument` (template order per page, valid editor document, references, tree shape, embedded illustrations, inline forms). Run the xUnit v3 binary `KleeneStar.Help.Test.exe`.
