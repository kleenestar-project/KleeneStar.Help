using KleeneStar.Core;
using WebExpress.WebCore.WebAttribute;
using WebExpress.WebCore.WebPlugin;

namespace KleeneStar.Help
{
    /// <summary>
    /// Plugin entry point of the KleeneStar help: the workspace <c>HELP</c> with the help pages
    /// of the product and the illustrations they show.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The plugin contributes one database seed (<see cref="WebSeed.HelpSeed"/>), which the core
    /// collects as an <c>IDatabaseSeed</c> and runs from <c>KleeneStarApplication.Run</c>, and the
    /// illustrations under <c>Assets/img</c>. It contributes no page, no endpoint and no manager:
    /// a help page is an ordinary document of the installation, read and edited like every
    /// other one, labelled for the help area of the start page and the header's help menu.
    /// </para>
    /// <para>
    /// It names the core's application, because the framework refuses a plugin without one. The
    /// assets are mounted under the plugin id inside the application's route
    /// (<see cref="AssetRoot"/>).
    /// </para>
    /// </remarks>
    [Name("kleenestar.help:plugin.name")]
    [Description("kleenestar.help:plugin.description")]
    [Application<KleeneStarApplication>()]
    [Dependency("webexpress.webapp")]
    [Dependency("kleenestar.core")]
    public sealed class KleeneStarPlugin : IPlugin
    {
        /// <summary>
        /// The plugin id (the namespace of this class, lower-cased).
        /// </summary>
        public const string Id = "kleenestar.help";

        /// <summary>
        /// The address of the plugin's assets as a stored document references them: the core
        /// application's route followed by the plugin id.
        /// </summary>
        /// <remarks>
        /// A document stores the address of an image as written, and the reading view accepts
        /// relative addresses only (no <c>data:</c> URIs), so the illustrations are served as
        /// assets and named by their path. The seeded icons of the model name the core's route
        /// the same way.
        /// </remarks>
        public const string AssetRoot = "/kleenestar/kleenestar.help/assets/";

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public KleeneStarPlugin()
        {
        }

        /// <summary>
        /// Called when the plugin starts working. The seed is run by the core.
        /// </summary>
        public void Run()
        {
        }

        /// <summary>
        /// Called when the host shuts down. The plugin holds no resources of its own.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
