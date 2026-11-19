using KleeneStar.Core.WebSeed;
using KleeneStar.Help.WebSeed;
using KleeneStar.Model;
using KleeneStar.Model.Entities;
using Microsoft.EntityFrameworkCore;
using WebExpress.WebCore.WebPlugin;

namespace KleeneStar.Help.Test
{
    /// <summary>
    /// Contains unit tests for the seed of the help workspace (<see cref="HelpSeeder"/>).
    /// </summary>
    [Collection("NonParallelTests")]
    public class UnitTestHelpSeeder
    {
        /// <summary>
        /// Verifies that the core's catalog of database seeds finds <see cref="HelpSeed"/> under
        /// the plugin id, ordered after the demo data.
        /// </summary>
        [Fact]
        public void HelpSeedIsDiscovered()
        {
            using var catalog = new PluginTypeCatalog<IDatabaseSeed>(x => x.Key);

            catalog.Collect(typeof(HelpSeed).Assembly);

            var seed = Assert.Single(catalog.Items);

            Assert.IsType<HelpSeed>(seed);
            Assert.Equal(KleeneStarPlugin.Id, seed.Key);
            Assert.True(seed.Order > 100, "the help runs after the demo data");
        }

        /// <summary>
        /// Verifies that the seed writes the workspace, its prose document class and every page
        /// under its key, chapters as roots, topics below their chapter, the home page as the
        /// home of the workspace.
        /// </summary>
        [Fact]
        public async Task SeedWritesWorkspaceClassAndPages()
        {
            await using var db = await SeedAsync($"SeedWrites_{Guid.NewGuid()}");

            var workspace = db.Workspaces.Single(x => x.Id == HelpSeeder.WorkspaceId);
            var cls = db.Classes.Single(x => x.Id == HelpSeeder.ClassId);

            Assert.Equal(HelpSeeder.WorkspaceKey, workspace.Key);
            Assert.Equal(ObjectKind.Document, cls.Kind);
            Assert.Equal(ObjectRenderer.Prose, cls.Renderer);
            Assert.Equal(3, db.Forms.Count(x => x.ClassId == cls.Id));

            foreach (var page in HelpSeeder.Pages)
            {
                var row = db.Objects.Single(x => x.Id == page.Id);

                Assert.Equal(page.Key, row.Key);
                Assert.Equal(page.Summary, row.Summary);
                Assert.Equal(HelpSeeder.WorkspaceId, row.WorkspaceId);
                Assert.Equal(HelpSeeder.ClassId, row.ClassId);
                Assert.Equal(ObjectKind.Document, row.Kind);
                Assert.Equal(WorkspaceState.Active, row.State);
                Assert.Equal(page.Role == HelpPageRole.Topic ? page.ParentId : null, row.ParentId);
            }

            var home = HelpSeeder.Pages.Single(x => x.Role == HelpPageRole.Home);

            Assert.Equal(home.Id, workspace.HomeId);
            Assert.False(db.Objects.Any(x => x.ParentId == home.Id), "the home page stands outside the page tree");
        }

        /// <summary>
        /// Verifies that every topic carries exactly one reserved landing label and that the three
        /// areas of the start page are filled.
        /// </summary>
        [Fact]
        public async Task SeedLabelsTheTopics()
        {
            await using var db = await SeedAsync($"SeedLabels_{Guid.NewGuid()}");

            var ids = HelpSeeder.Pages.Select(x => x.Id).ToList();
            var tags = db.ObjectTags.Where(x => ids.Contains(x.ObjectId)).ToList();

            foreach (var page in HelpSeeder.Pages)
            {
                var own = tags.Where(x => x.ObjectId == page.Id).ToList();

                if (page.Role == HelpPageRole.Topic)
                {
                    Assert.Equal(page.Label, Assert.Single(own).Name);
                }
                else
                {
                    Assert.Empty(own);
                }
            }

            foreach (var label in new[] { HelpSeeder.LabelHelp, HelpSeeder.LabelFaq, HelpSeeder.LabelFirstSteps })
            {
                Assert.True(tags.Count(x => x.Name == label) >= 3, $"expected several pages labelled {label}");
            }
        }

        /// <summary>
        /// Verifies that the workspace is granted administration to the administrators and
        /// reading to the signed-in users, and that a second run grants nothing more.
        /// </summary>
        [Fact]
        public async Task SeedGrantsAdministrationAndReading()
        {
            await using var db = await SeedAsync($"SeedGrants_{Guid.NewGuid()}");

            await HelpSeeder.SeedAsync(db);

            var grants = db.PermissionAssignments
                .Where(x => x.Scope == WorkspacePermissionDefaults.Scope && x.ScopeId == HelpSeeder.WorkspaceId.ToString())
                .ToList();

            Assert.Equal(2, grants.Count);
            Assert.Contains(grants, x => x.GroupId == Group.AdministratorsId && x.Policy == WorkspacePermissionDefaults.AdminPolicy);
            Assert.Contains(grants, x => x.GroupId == Group.AuthenticatedId && x.Policy == HelpSeeder.ReaderPolicy);
        }

        /// <summary>
        /// Verifies that a second run adds nothing.
        /// </summary>
        [Fact]
        public async Task SeedIsIdempotent()
        {
            await using var db = await SeedAsync($"SeedIdempotent_{Guid.NewGuid()}");

            var counts = (db.Workspaces.Count(), db.Classes.Count(), db.Forms.Count(), db.Objects.Count(), db.ObjectTags.Count(), db.PermissionAssignments.Count());

            await HelpSeeder.SeedAsync(db);

            Assert.Equal(counts, (db.Workspaces.Count(), db.Classes.Count(), db.Forms.Count(), db.Objects.Count(), db.ObjectTags.Count(), db.PermissionAssignments.Count()));
        }

        /// <summary>
        /// Verifies that a page nobody changed is brought up to the content of the plugin, while
        /// a page with a commit belongs to its authors and stays as written.
        /// </summary>
        [Fact]
        public async Task SeedRefreshesUntouchedPagesOnly()
        {
            await using var db = await SeedAsync($"SeedRefresh_{Guid.NewGuid()}");

            var untouched = HelpSeeder.Pages[3];
            var edited = HelpSeeder.Pages[4];

            foreach (var id in new[] { untouched.Id, edited.Id })
            {
                db.Objects.Single(x => x.Id == id).Description = "older text";
            }

            db.Commits.Add(new Commit
            {
                Id = Guid.NewGuid(),
                ObjectId = edited.Id,
                ObjectKey = edited.Key,
                Number = 1,
                Type = CommitType.Updated,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            });

            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            await HelpSeeder.SeedAsync(db);

            Assert.Equal(HelpDocument.Build(untouched, HelpSeeder.Version), db.Objects.AsNoTracking().Single(x => x.Id == untouched.Id).Description);
            Assert.Equal("older text", db.Objects.AsNoTracking().Single(x => x.Id == edited.Id).Description);
        }

        /// <summary>
        /// Verifies that the help stays out of a store whose own workspace already carries the
        /// key <c>HELP</c>.
        /// </summary>
        [Fact]
        public async Task SeedLeavesAForeignHelpWorkspaceAlone()
        {
            await using var db = InMemoryDbContextFactory.Create($"SeedForeign_{Guid.NewGuid()}");

            await KleeneStarDbSeeder.SeedAsync(db);

            db.Workspaces.Add(new Workspace
            {
                Id = Guid.NewGuid(),
                Key = HelpSeeder.WorkspaceKey,
                Name = "Helpdesk",
                State = WorkspaceState.Active,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            Assert.False(await HelpSeeder.SeedAsync(db));
            Assert.False(db.Workspaces.Any(x => x.Id == HelpSeeder.WorkspaceId));
            Assert.False(db.Objects.Any());
        }

        /// <summary>
        /// Verifies that a page whose key another object took is seeded under the next free key.
        /// </summary>
        [Fact]
        public async Task SeedSkipsATakenKey()
        {
            await using var db = InMemoryDbContextFactory.Create($"SeedTakenKey_{Guid.NewGuid()}");

            await KleeneStarDbSeeder.SeedAsync(db);
            await HelpSeeder.SeedAsync(db);

            var page = HelpSeeder.Pages[^1];
            var row = db.Objects.Single(x => x.Id == page.Id);

            db.Objects.Remove(row);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            db.Objects.Add(new Model.Entities.Object
            {
                Id = Guid.NewGuid(),
                Key = page.Key,
                Summary = "Somebody else's page",
                Kind = ObjectKind.Document,
                State = WorkspaceState.Active,
                WorkspaceId = HelpSeeder.WorkspaceId,
                ClassId = HelpSeeder.ClassId,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            await HelpSeeder.SeedAsync(db);

            var reseeded = db.Objects.AsNoTracking().Single(x => x.Id == page.Id);

            Assert.NotEqual(page.Key, reseeded.Key);
            Assert.StartsWith(HelpSeeder.WorkspaceKey + "-", reseeded.Key);
            Assert.Equal(1, db.Objects.Count(x => x.Key == page.Key));
        }

        /// <summary>
        /// Writes the base data and the help into a fresh in-memory store.
        /// </summary>
        /// <param name="name">The name of the store.</param>
        /// <returns>The context.</returns>
        private static async Task<KleeneStarDbContext> SeedAsync(string name)
        {
            var db = InMemoryDbContextFactory.Create(name);

            await KleeneStarDbSeeder.SeedAsync(db);
            Assert.True(await HelpSeeder.SeedAsync(db));

            return db;
        }
    }
}
