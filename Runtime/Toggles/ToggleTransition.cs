
namespace Quirks.UI
{
    /// <summary>
    /// Display settings for when a toggle is activated or deactivated.
    /// </summary>
    public enum ToggleTransition
    {
        /// <summary>
        /// Show / hide the toggle instantly.
        /// </summary>
        Instant,

        /// <summary>
        /// Fade the toggle in / out smoothly.
        /// </summary>
        Fade,

        /// <summary>
        /// Fade the toggle in / out quickly. 
        /// </summary>
        FadeQuickly
    }
}