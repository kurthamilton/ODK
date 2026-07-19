using ODK.Core.DataTypes;

namespace ODK.Core.Chapters;

public static class ChapterPropertyExtensions
{
    /// <summary>
    /// Returns the required chapter properties that the member has not satisfied.
    /// </summary>
    /// <param name="valuesByPropertyId">The member's answers, keyed by <see cref="ChapterProperty"/> id.</param>
    /// <param name="forApplication">
    /// When false, application-only properties are ignored (they are only required when applying to join).
    /// </param>
    public static IReadOnlyCollection<ChapterProperty> GetMissingRequired(
        this IEnumerable<ChapterProperty> chapterProperties,
        IReadOnlyDictionary<Guid, string?> valuesByPropertyId,
        bool forApplication)
    {
        var missing = new List<ChapterProperty>();

        foreach (var chapterProperty in chapterProperties.Where(x => x.Required))
        {
            if (chapterProperty.ApplicationOnly && !forApplication)
            {
                continue;
            }

            valuesByPropertyId.TryGetValue(chapterProperty.Id, out var value);

            var satisfied = chapterProperty.DataType == DataType.Checkbox
                ? string.Equals(value, "true", StringComparison.InvariantCultureIgnoreCase)
                : !string.IsNullOrWhiteSpace(value);

            if (!satisfied)
            {
                missing.Add(chapterProperty);
            }
        }

        return missing;
    }
}
