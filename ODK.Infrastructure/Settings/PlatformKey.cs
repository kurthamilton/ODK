using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

/// <summary>The label a per-platform configuration section is keyed by.</summary>
/// <remarks>
/// <para>
/// Deliberately not <see cref="PlatformType"/>, which is the app's own vocabulary. A configuration key is
/// read where its whole path is spelled out - <c>STRIPE_PLATFORMS_DK_WEBHOOKSECRETV1</c> in Doppler, which is
/// where every per-platform secret is set - so a section labels its platform in two characters.
/// <see cref="PlatformKeyExtensions.ToPlatformType"/> maps a label to the platform it names, and every
/// settings type is mapped in <c>DependencyRegistrar</c> before a service sees it, so a label reaches nothing
/// outside this project.
/// </para>
/// <para>
/// Keys only. The <c>Platform</c> key's <em>value</em> - the platform a deployment serves - is a
/// <see cref="PlatformType"/>, because it is a value a person reads whole rather than a segment of a long
/// key. Crossing the two fails loudly in that direction: a scalar the binder cannot convert throws while it
/// is binding, so a deployment stating <c>GS</c> never starts rather than serving the wrong platform.
/// </para>
/// <para>
/// One vocabulary per section, never both: the binder converts a dictionary key through these names alone
/// and drops a key it cannot convert, so a section keyed by a platform name contributes nothing rather than
/// overriding something.
/// </para>
/// </remarks>
public enum PlatformKey
{
    /// <summary>No platform is named. Reserved, so an unset value is never read as a platform.</summary>
    None,

    /// <summary>Drunken Knitwits.</summary>
    DK,

    /// <summary>Group Squirrel.</summary>
    GS
}
