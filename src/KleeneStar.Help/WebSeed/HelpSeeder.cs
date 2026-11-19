using KleeneStar.Core;
using KleeneStar.Model;
using KleeneStar.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebExpress.WebUI.WebIcon;

namespace KleeneStar.Help.WebSeed
{
    /// <summary>
    /// Seeds the help of KleeneStar: the workspace <c>HELP</c>, its document class, the home page,
    /// the chapters and the help pages below them, and the landing labels of the pages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The help is product content, not demonstration data: it is written into every store, a
    /// productive one included, on every start. Every part is guarded by a check on the store,
    /// so a second run adds nothing.
    /// </para>
    /// <para>
    /// A page is recognized by its fixed id. A page nobody changed since it was seeded (no
    /// commit and no pending draft) is brought up to the content of the plugin on start, so a
    /// newer plugin version reaches the pages it wrote. A page somebody published, or is
    /// writing, belongs to its authors and is left as it is. A page that was deleted comes back
    /// on the next start; a page the organization does not want is archived instead.
    /// </para>
    /// </remarks>
    public static partial class HelpSeeder
    {
        /// <summary>
        /// The id of the help workspace.
        /// </summary>
        public static readonly Guid WorkspaceId = Guid.Parse("30A4DC9A-13A0-4A4E-8D1E-E76FF15BAD1F");

        /// <summary>
        /// The id of the document class of the help pages.
        /// </summary>
        public static readonly Guid ClassId = Guid.Parse("1061DBF7-78D2-4780-ABA2-A93C531CFC61");

        /// <summary>
        /// The key of the help workspace, the prefix of the page keys.
        /// </summary>
        public const string WorkspaceKey = "HELP";

        /// <summary>
        /// The name of the help workspace.
        /// </summary>
        public const string WorkspaceName = "KleeneStar Help";

        /// <summary>
        /// The name of the document class of the help pages.
        /// </summary>
        public const string ClassName = "Help page";

        /// <summary>
        /// The description of the workspace.
        /// </summary>
        public const string WorkspaceDescription = "Guides, first steps and answers to frequently asked questions about working with KleeneStar.";

        /// <summary>
        /// The description of the class.
        /// </summary>
        public const string ClassDescription = "Pages of the KleeneStar help: guides, first steps and frequently asked questions.";

        /// <summary>
        /// Returns the fixed texts of the workspace, shipped as internationalization keys like the
        /// texts of the pages. The class name stays as written: the page lists of the home page
        /// name the class by it.
        /// </summary>
        public static IEnumerable<string> FixedTexts => [WorkspaceName, WorkspaceDescription, ClassDescription];

        /// <summary>
        /// The colour of the reserved landing labels the help pages carry.
        /// </summary>
        public const string LabelColor = "#0dcaf0";

        /// <summary>
        /// The policy the signed-in users hold on the help workspace: reading.
        /// </summary>
        /// <remarks>
        /// The help is curated: the administrators write it (the default administrator grant),
        /// everybody signed in reads it. A workspace created from a template grants the signed-in
        /// users the edit policy instead (<see cref="WorkspacePermissionDefaults"/>).
        /// </remarks>
        public const string ReaderPolicy = "workspace_view_policy";

        /// <summary>
        /// Returns the version of the help: the version of this plugin, without build metadata.
        /// </summary>
        public static string Version { get; } = ResolveVersion();

        /// <summary>
        /// Writes the help in the language of the installation (<c>WebExpress:Culture</c>).
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <returns>False when a workspace of the installation already carries the key
        /// <c>HELP</c> and the help stays out, else true.</returns>
        public static Task<bool> SeedAsync(KleeneStarDbContext db)
        {
            return SeedAsync(db, HelpText.For(CoreHub.HttpServerContext?.Culture));
        }

        /// <summary>
        /// Writes the help in a language.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="culture">The culture whose language the help is written in.</param>
        /// <returns>False when a workspace of the installation already carries the key
        /// <c>HELP</c> and the help stays out, else true.</returns>
        public static Task<bool> SeedAsync(KleeneStarDbContext db, CultureInfo culture)
        {
            return SeedAsync(db, HelpText.For(culture));
        }

        /// <summary>
        /// Writes what the store does not carry yet: the workspace, the class, the permissions,
        /// the pages, the home page and the labels. Names, descriptions and pages are written
        /// through the translator; a page nobody changed is rewritten when its text differs, so
        /// the untouched pages follow a change of the language too.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="translate">The translator of the language the help is written in.</param>
        /// <returns>False when a workspace of the installation already carries the key
        /// <c>HELP</c> and the help stays out, else true.</returns>
        public static async Task<bool> SeedAsync(KleeneStarDbContext db, Func<string, string> translate)
        {
            ArgumentNullException.ThrowIfNull(db);

            translate ??= HelpText.English;

            // a workspace of the installation already carries the key: the help stays out rather
            // than taking over somebody else's workspace
            if (!db.Workspaces.Any(x => x.Id == WorkspaceId) && db.Workspaces.Any(x => x.Key == WorkspaceKey))
            {
                return false;
            }

            await Step(db, "workspace", x => SeedWorkspace(x, translate));
            await Step(db, "class", x => SeedClass(x, translate));
            await Step(db, "permissions", SeedPermissions);
            await Step(db, "pages", x => SeedPages(x, translate));
            await Step(db, "home page", SeedHome);
            await Step(db, "labels", SeedLabels);

            return true;
        }

        /// <summary>
        /// Adds the help workspace when it is missing.
        /// </summary>
        /// <param name="db">The database context.</param>
        private static void SeedWorkspace(KleeneStarDbContext db, Func<string, string> translate)
        {
            if (db.Workspaces.Any(x => x.Id == WorkspaceId))
            {
                return;
            }

            db.Workspaces.Add(new Workspace
            {
                Id = WorkspaceId,
                Name = translate(WorkspaceName),
                Key = WorkspaceKey,
                Description = translate(WorkspaceDescription),
                State = WorkspaceState.Active,
                Icon = ImageIcon.FromString(HelpDocument.IllustrationUri("workspace")),
                AccessModifier = WorkspaceAccessModifier.Public,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Adds the document class of the help pages with its three standard forms when it is
        /// missing. The pages are prose, so the forms carry no fields.
        /// </summary>
        /// <param name="db">The database context.</param>
        private static void SeedClass(KleeneStarDbContext db, Func<string, string> translate)
        {
            if (db.Classes.Any(x => x.Id == ClassId))
            {
                return;
            }

            db.Classes.Add(new Class
            {
                Id = ClassId,
                Name = ClassName,
                Description = translate(ClassDescription),
                Icon = ImageIcon.FromString(HelpDocument.IllustrationUri("class")),
                Kind = ObjectKind.Document,
                Renderer = ObjectRenderer.Prose,
                WorkspaceId = WorkspaceId,
                State = ClassState.Active,
                AccessModifier = AccessModifier.Public,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            });

            foreach (var (name, type, icon) in new[]
            {
                ("Create", FormType.Create, "/kleenestar/assets/icons/form/create.svg"),
                ("Edit", FormType.Edit, "/kleenestar/assets/icons/form/edit.svg"),
                ("View", FormType.View, "/kleenestar/assets/icons/form/view.svg")
            })
            {
                var formId = Guid.NewGuid();

                db.Forms.Add(new Form
                {
                    Id = formId,
                    Name = name,
                    Description = "Standard form for the class.",
                    FormType = type,
                    State = FormState.Active,
                    Icon = ImageIcon.FromString(icon),
                    ClassId = ClassId,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                });

                db.FormTabs.Add(new FormTab
                {
                    Id = Guid.NewGuid(),
                    FormId = formId,
                    Name = "General",
                    Position = 0
                });
            }
        }

        /// <summary>
        /// Grants the help workspace its permissions while it carries none: administration to
        /// the administrators, reading to every signed-in user. A workspace somebody administered
        /// already is left as it is.
        /// </summary>
        /// <param name="db">The database context.</param>
        private static void SeedPermissions(KleeneStarDbContext db)
        {
            var scopeId = WorkspaceId.ToString();

            if (db.PermissionAssignments.Any(x => x.Scope == WorkspacePermissionDefaults.Scope && x.ScopeId == scopeId))
            {
                return;
            }

            var groups = db.Groups.Select(x => x.Id).ToHashSet();

            foreach (var (groupId, policy) in new[]
            {
                (Model.Entities.Group.AdministratorsId, WorkspacePermissionDefaults.AdminPolicy),
                (Model.Entities.Group.AuthenticatedId, ReaderPolicy)
            })
            {
                // a grant naming a group the store does not carry could never be read back
                if (!groups.Contains(groupId))
                {
                    continue;
                }

                db.PermissionAssignments.Add(new PermissionAssignment(Guid.NewGuid())
                {
                    Scope = WorkspacePermissionDefaults.Scope,
                    ScopeId = scopeId,
                    GroupId = groupId,
                    Policy = policy,
                    Created = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Adds the pages that are missing and brings the untouched ones up to the content of the
        /// plugin.
        /// </summary>
        /// <param name="db">The database context.</param>
        private static void SeedPages(KleeneStarDbContext db, Func<string, string> translate)
        {
            var workspace = db.Workspaces.AsNoTracking().Single(x => x.Id == WorkspaceId);
            var creatorId = KleeneStarDbSeeder.AdministratorId;
            var creator = db.Identities.AsNoTracking().Any(x => x.Id == creatorId) ? creatorId : (Guid?)null;
            var icon = ImageIcon.FromString(HelpDocument.IllustrationUri("class"));

            var ids = Pages.Select(x => x.Id).ToList();
            var existing = db.Objects.Where(x => ids.Contains(x.Id)).ToDictionary(x => x.Id);
            var touched = db.Commits.AsNoTracking()
                .Where(x => ids.Contains(x.ObjectId))
                .Select(x => x.ObjectId)
                .Concat(db.ObjectDrafts.AsNoTracking().Where(x => ids.Contains(x.ObjectId)).Select(x => x.ObjectId))
                .ToHashSet();

            foreach (var page in Pages)
            {
                var description = HelpDocument.Build(page, Version, translate);
                var summary = translate(page.Summary);

                if (existing.TryGetValue(page.Id, out var current))
                {
                    if (!touched.Contains(page.Id) && (current.Description != description || current.Summary != summary))
                    {
                        current.Summary = summary;
                        current.Description = description;
                        current.Updated = DateTime.UtcNow;
                    }

                    continue;
                }

                // a parent that is not in the store (deleted, or refused below) leaves the page a root
                var parentId = page.ParentId is { } parent && (existing.ContainsKey(parent) || db.Objects.Local.Any(x => x.Id == parent))
                    ? parent
                    : (Guid?)null;

                db.Objects.Add(new Model.Entities.Object
                {
                    Id = page.Id,
                    Key = FreeKey(db, workspace, page.Key),
                    Summary = summary,
                    Description = description,
                    Icon = icon,
                    Kind = ObjectKind.Document,
                    State = WorkspaceState.Active,
                    WorkspaceId = WorkspaceId,
                    ClassId = ClassId,
                    ParentId = page.Role == HelpPageRole.Topic ? parentId : null,
                    CreatorId = creator,
                    UpdaterId = creator,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Makes the seeded home page the home page of the help workspace while the workspace
        /// names none, or names one that is gone.
        /// </summary>
        /// <param name="db">The database context.</param>
        private static void SeedHome(KleeneStarDbContext db)
        {
            var home = Pages.Single(x => x.Role == HelpPageRole.Home);
            var workspace = db.Workspaces.Single(x => x.Id == WorkspaceId);

            if (!db.Objects.Any(x => x.Id == home.Id))
            {
                return;
            }

            if (workspace.HomeId is { } homeId && homeId != Guid.Empty && db.Objects.Any(x => x.Id == homeId))
            {
                return;
            }

            workspace.HomeId = home.Id;
            workspace.Updated = DateTime.UtcNow;
        }

        /// <summary>
        /// Attaches the reserved landing label of each page that carries one: the start page and
        /// the header's help menu list the help through these labels.
        /// </summary>
        /// <param name="db">The database context.</param>
        private static void SeedLabels(KleeneStarDbContext db)
        {
            var labelled = Pages.Where(x => x.Label is not null).ToList();
            var ids = labelled.Select(x => x.Id).ToList();
            var present = db.Objects.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToHashSet();
            var tagged = db.ObjectTags.AsNoTracking()
                .Where(x => ids.Contains(x.ObjectId))
                .Select(x => new { x.ObjectId, x.Name })
                .ToList()
                .Select(x => (x.ObjectId, Name: x.Name.ToLowerInvariant()))
                .ToHashSet();

            foreach (var page in labelled.Where(x => present.Contains(x.Id)))
            {
                // the composite unique index on (ObjectId, Name) makes a second attempt fatal
                if (tagged.Contains((page.Id, page.Label.ToLowerInvariant())))
                {
                    continue;
                }

                db.ObjectTags.Add(new ObjectTag
                {
                    ObjectId = page.Id,
                    Name = page.Label,
                    Color = LabelColor,
                    Created = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Returns the key a new page is seeded under: its own key while that is free in the
        /// workspace (also as an alias of a moved object), otherwise the next free number.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="workspace">The help workspace.</param>
        /// <param name="key">The key of the page.</param>
        /// <returns>The key.</returns>
        private static string FreeKey(KleeneStarDbContext db, Workspace workspace, string key)
        {
            var taken = db.Objects.AsNoTracking().Any(x => x.Key == key)
                || db.Objects.Local.Any(x => x.Key == key)
                || db.ObjectKeyAliases.AsNoTracking().Any(x => x.Key == key);

            if (!taken)
            {
                return key;
            }

            var prefix = workspace.Key;
            var pattern = new Regex($"^{Regex.Escape(prefix)}-(\\d+)$", RegexOptions.IgnoreCase);

            var keys = db.Objects.AsNoTracking()
                .Where(x => x.WorkspaceId == workspace.Id)
                .Select(x => x.Key)
                .ToList()
                .Concat(db.ObjectKeyAliases.AsNoTracking()
                    .Where(x => x.Key.StartsWith(prefix + "-"))
                    .Select(x => x.Key)
                    .ToList())
                .Concat(db.Objects.Local.Where(x => x.WorkspaceId == workspace.Id).Select(x => x.Key))
                .Concat(Pages.Select(x => x.Key));

            var next = keys
                .Select(x => pattern.Match(x ?? string.Empty))
                .Where(m => m.Success)
                .Select(m => int.TryParse(m.Groups[1].Value, out var number) ? number : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;

            return $"{prefix}-{next.ToString(CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Reads the version of the plugin assembly, without the build metadata the SDK appends.
        /// </summary>
        /// <returns>The version.</returns>
        private static string ResolveVersion()
        {
            var assembly = typeof(HelpSeeder).Assembly;
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "0.0.0";

            var plus = version.IndexOf('+');

            return plus >= 0 ? version[..plus] : version;
        }

        /// <summary>
        /// Runs one seeding step and saves its changes.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="name">The name of the step, for the error message.</param>
        /// <param name="seed">The step.</param>
        /// <returns>A task representing the step.</returns>
        /// <exception cref="InvalidOperationException">The step failed; the inner exception names the cause.</exception>
        private static async Task Step(KleeneStarDbContext db, string name, Action<KleeneStarDbContext> seed)
        {
            try
            {
                seed(db);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Seeding the help {name} failed: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        /// <summary>
        /// Returns the pages of the help in seeding order: home page, then each chapter followed
        /// by its pages.
        /// </summary>
        public static IReadOnlyList<HelpPage> Pages => _pages;
    }
}
