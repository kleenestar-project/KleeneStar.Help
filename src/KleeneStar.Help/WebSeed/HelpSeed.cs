using KleeneStar.Core.WebSeed;
using KleeneStar.Model;
using System.Threading.Tasks;

namespace KleeneStar.Help.WebSeed
{
    /// <summary>
    /// The help workspace as a database seed of the core (<see cref="IDatabaseSeed"/>),
    /// delegating to <see cref="HelpSeeder.SeedAsync"/>.
    /// </summary>
    /// <remarks>
    /// Runs after the demo data (order 100), because the demo seed reads a store that already
    /// carries a workspace of its own as a productive installation and leaves it alone.
    /// </remarks>
    public sealed class HelpSeed : IDatabaseSeed
    {
        /// <inheritdoc/>
        public string Key => KleeneStarPlugin.Id;

        /// <inheritdoc/>
        public int Order => 200;

        /// <inheritdoc/>
        public Task SeedAsync(KleeneStarDbContext db)
        {
            return HelpSeeder.SeedAsync(db);
        }
    }
}
