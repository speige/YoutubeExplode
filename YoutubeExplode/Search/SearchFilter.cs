using System;
using System.Collections.Generic;

namespace YoutubeExplode.Search;

/// <summary>
/// Filter applied to a YouTube search query.
/// </summary>
public readonly record struct SearchFilter(
    SearchFilterType? Type = null,
    SearchFilterDuration? Duration = null,
    SearchFilterUploadDate? UploadDate = null,
    SearchFilterFeatures Features = SearchFilterFeatures.None,
    SearchFilterSortBy SortBy = SearchFilterSortBy.Relevance
)
{
    /// <summary>
    /// No filter applied.
    /// </summary>
    public static SearchFilter None { get; } = new();

    /// <summary>
    /// Only search for videos.
    /// </summary>
    public static SearchFilter Video { get; } = new(SearchFilterType.Video);

    /// <summary>
    /// Only search for playlists.
    /// </summary>
    public static SearchFilter Playlist { get; } = new(SearchFilterType.Playlist);

    /// <summary>
    /// Only search for channels.
    /// </summary>
    public static SearchFilter Channel { get; } = new(SearchFilterType.Channel);

    private void WriteVarint(List<byte> buffer, ulong value)
    {
        while (value >= 0x80)
        {
            buffer.Add((byte)(value | 0x80));
            value >>= 7;
        }
        buffer.Add((byte)value);
    }

    private void WriteTag(List<byte> buffer, int fieldNumber, int wireType)
    {
        WriteVarint(buffer, (ulong)((fieldNumber << 3) | wireType));
    }

    private void WriteVarintField(List<byte> buffer, int fieldNumber, ulong value)
    {
        WriteTag(buffer, fieldNumber, 0);
        WriteVarint(buffer, value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var topBuffer = new List<byte>();

        // Field 1: Sort By
        if (SortBy != SearchFilterSortBy.Relevance)
        {
            WriteVarintField(topBuffer, 1, (ulong)SortBy);
        }

        // Field 2: Filters
        var filterBuffer = new List<byte>();
        if (UploadDate is not null)
            WriteVarintField(filterBuffer, 1, (ulong)UploadDate);
        if (Type is not null)
            WriteVarintField(filterBuffer, 2, (ulong)Type);
        if (Duration is not null)
            WriteVarintField(filterBuffer, 3, (ulong)Duration);

        if (Features.HasFlag(SearchFilterFeatures.HD))
            WriteVarintField(filterBuffer, 4, 1);
        if (Features.HasFlag(SearchFilterFeatures.Subtitles))
            WriteVarintField(filterBuffer, 5, 1);
        if (Features.HasFlag(SearchFilterFeatures.CreativeCommons))
            WriteVarintField(filterBuffer, 6, 1);
        if (Features.HasFlag(SearchFilterFeatures.ThreeD))
            WriteVarintField(filterBuffer, 7, 1);
        if (Features.HasFlag(SearchFilterFeatures.Live))
            WriteVarintField(filterBuffer, 8, 1);
        if (Features.HasFlag(SearchFilterFeatures.VR180))
            WriteVarintField(filterBuffer, 9, 1);
        if (Features.HasFlag(SearchFilterFeatures.Location))
            WriteVarintField(filterBuffer, 10, 1);
        if (Features.HasFlag(SearchFilterFeatures.Purchased))
            WriteVarintField(filterBuffer, 11, 1);
        if (Features.HasFlag(SearchFilterFeatures.FourK))
            WriteVarintField(filterBuffer, 14, 1);
        if (Features.HasFlag(SearchFilterFeatures.HDR))
            WriteVarintField(filterBuffer, 15, 1);
        if (Features.HasFlag(SearchFilterFeatures.ThreeSixty))
            WriteVarintField(filterBuffer, 16, 1);

        if (filterBuffer.Count > 0)
        {
            WriteTag(topBuffer, 2, 2);
            WriteVarint(topBuffer, (ulong)filterBuffer.Count);
            topBuffer.AddRange(filterBuffer);
        }

        if (topBuffer.Count == 0)
            return "";

        return Convert.ToBase64String(topBuffer.ToArray());
    }
}

/// <summary>
/// Search result type filter.
/// </summary>
public enum SearchFilterType
{
    /// <summary>
    /// Only search for videos.
    /// </summary>
    Video = 1,

    /// <summary>
    /// Only search for channels.
    /// </summary>
    Channel = 2,

    /// <summary>
    /// Only search for playlists.
    /// </summary>
    Playlist = 3,

    /// <summary>
    /// Only search for movies.
    /// </summary>
    Movie = 4,

    /// <summary>
    /// Only search for shorts.
    /// </summary>
    Shorts = 5,
}

/// <summary>
/// Search result duration filter.
/// </summary>
public enum SearchFilterDuration
{
    /// <summary>
    /// Short duration (under 4 minutes).
    /// </summary>
    Short = 1,

    /// <summary>
    /// Long duration (over 20 minutes).
    /// </summary>
    Long = 2,

    /// <summary>
    /// Medium duration (4 to 20 minutes).
    /// </summary>
    Medium = 3,
}

/// <summary>
/// Search result upload date filter.
/// </summary>
public enum SearchFilterUploadDate
{
    /// <summary>
    /// Uploaded today.
    /// </summary>
    Today = 2,

    /// <summary>
    /// Uploaded this week.
    /// </summary>
    ThisWeek = 3,

    /// <summary>
    /// Uploaded this month.
    /// </summary>
    ThisMonth = 4,

    /// <summary>
    /// Uploaded this year.
    /// </summary>
    ThisYear = 5,
}

/// <summary>
/// Search result features filter.
/// </summary>
[Flags]
public enum SearchFilterFeatures
{
    /// <summary>
    /// No features.
    /// </summary>
    None = 0,

    /// <summary>
    /// Live videos.
    /// </summary>
    Live = 1 << 0,

    /// <summary>
    /// 4K videos.
    /// </summary>
    FourK = 1 << 1,

    /// <summary>
    /// HD videos.
    /// </summary>
    HD = 1 << 2,

    /// <summary>
    /// Subtitles/CC.
    /// </summary>
    Subtitles = 1 << 3,

    /// <summary>
    /// Creative Commons.
    /// </summary>
    CreativeCommons = 1 << 4,

    /// <summary>
    /// 360° videos.
    /// </summary>
    ThreeSixty = 1 << 5,

    /// <summary>
    /// VR180 videos.
    /// </summary>
    VR180 = 1 << 6,

    /// <summary>
    /// 3D videos.
    /// </summary>
    ThreeD = 1 << 7,

    /// <summary>
    /// HDR videos.
    /// </summary>
    HDR = 1 << 8,

    /// <summary>
    /// Location.
    /// </summary>
    Location = 1 << 9,

    /// <summary>
    /// Purchased.
    /// </summary>
    Purchased = 1 << 10,
}

/// <summary>
/// Search result sort order.
/// </summary>
public enum SearchFilterSortBy
{
    /// <summary>
    /// Relevance.
    /// </summary>
    Relevance = 0,

    /// <summary>
    /// Upload date.
    /// </summary>
    UploadDate = 2,

    /// <summary>
    /// View count (popularity).
    /// </summary>
    Popularity = 3,
}
