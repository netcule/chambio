using System.Text.Json;
using Chambio.Server.Entities;
using Chambio.Server.Options;
using Chambio.Server.Persistence;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Chambio.Server.Services;

public class WikiService
{
    readonly WikiOptions _options;

    readonly ChambioContext _context;

    readonly HttpClient _http;

    public WikiService(IOptions<WikiOptions> options, ChambioContext context, HttpClient http)
    {
        _options = options.Value;
        _context = context;
        _http = http;
    }

    public async Task FillPartyAsync(Party party, string countryKey,
        string languageCode, CancellationToken cancellationToken,
        string? searchCategory = null, string? nationalPageTitle = null,
        string? internationalPageTitle = null)
    {
        nationalPageTitle ??= _options.NationalReferrals?
            .GetValueOrDefault($"{countryKey}/{party.Key}");

        Dictionary<string, string?> nationalPageQuery = new()
        {
            { "action", "query" },
            { "prop", "langlinks|categories" },
            { "lllang", "en" },
            { "clshow", "!hidden" },
            { "cllimit", "500" },
            { "format", "json" },
            { "formatversion", "2" }
        };

        if (nationalPageTitle is null)
        {
            nationalPageQuery["generator"] = "search";

            nationalPageQuery["gsrsearch"] = party.Key +
                (searchCategory is null ?
                    string.Empty : $" incategory:\"{searchCategory}\"");

            nationalPageQuery["gsrlimit"] = "1";
        }
        else
        {
            nationalPageQuery["titles"] = nationalPageTitle;
        }

        JsonDocument nationalPageDocument = await GetDocumentAsync(
            languageCode, nationalPageQuery, cancellationToken);

        JsonElement nationalPageElement;

        try
        {
            nationalPageElement = nationalPageDocument.RootElement
                .GetProperty("query")
                .GetProperty("pages")[0];
        }
        catch (KeyNotFoundException)
        {
            return;
        }

        HashSet<string> nationalCategories = new();
        HashSet<string> internationalCategories = new();

        try
        {
            nationalCategories.UnionWith(nationalPageElement
                .GetProperty("categories")
                .EnumerateArray()
                .Select(e => e
                    .GetProperty("title")
                    .GetString()!));
        }
        catch (KeyNotFoundException) { }

        if (languageCode is "en")
        {
            try
            {
                internationalPageTitle = nationalPageElement
                    .GetProperty("title")
                    .GetString()!;
            }
            catch (KeyNotFoundException) { }

            internationalCategories = nationalCategories;
        }
        else
        {
            IEnumerable<string[]> nationalCategoryChunks = nationalCategories
                .Chunk(10);

            foreach (string[] nationalCategoryChunk in nationalCategoryChunks)
            {
                Dictionary<string, string?> translatedCategoriesQuery = new()
                {
                    { "action", "query" },
                    { "prop", "langlinks" },
                    { "titles", string.Join('|', nationalCategoryChunk) },
                    { "lllang", "en" },
                    { "format", "json" },
                    { "formatversion", "2" }
                };

                JsonDocument translatedCategoriesDocument =
                    await GetDocumentAsync(languageCode, translatedCategoriesQuery,
                        cancellationToken);

                try
                {
                    internationalCategories.UnionWith(
                        translatedCategoriesDocument
                            .RootElement
                                .GetProperty("query")
                                .GetProperty("pages")
                                .EnumerateArray()
                                .Where(e => e
                                    .TryGetProperty("langlinks", out _))
                                .Select(e => e
                                    .GetProperty("langlinks")[0]
                                    .GetProperty("title")
                                    .GetString()!));
                }
                catch (KeyNotFoundException) { }
            }

            try
            {
                internationalPageTitle ??= _options.InternationalReferrals?
                    .GetValueOrDefault($"{countryKey}/{party.Key}") ??
                        nationalPageElement
                            .GetProperty("langlinks")[0]
                            .GetProperty("title")
                            .GetString()!;

                Dictionary<string, string?> internationalPageQuery = new()
                {
                    { "action", "query" },
                    { "prop", "categories" },
                    { "titles", internationalPageTitle },
                    { "clshow", "!hidden" },
                    { "cllimit", "500" },
                    { "format", "json" },
                    { "formatversion", "2" }
                };

                JsonDocument internationalPageDocument =
                    await GetDocumentAsync("en", internationalPageQuery,
                        cancellationToken);

                JsonElement internationalPageElement =
                    internationalPageDocument.RootElement
                        .GetProperty("query")
                        .GetProperty("pages")[0];

                internationalCategories.UnionWith(internationalPageElement
                    .GetProperty("categories")
                    .EnumerateArray()
                    .Select(e => e
                        .GetProperty("title")
                        .GetString()!));
            }
            catch (KeyNotFoundException) { }
        }

        if (internationalPageTitle is not null)
            party.Name = internationalPageTitle.Split(" (")[0];

        if (internationalCategories.Count > 0)
        {
            HashSet<Ideology> updatedIdeologies = new();

            HashSet<string> filteredCategories = FilterCategories(
                internationalCategories);

            foreach (string filteredCategory in filteredCategories)
            {
                Ideology ideology = await _context.Ideologies
                    .FirstOrDefaultAsync(i => i.Name == filteredCategory,
                        cancellationToken) ?? new(filteredCategory);

                if (!party.Ideologies!.Contains(ideology))
                    party.Ideologies!.Add(ideology);

                updatedIdeologies.Add(ideology);
            }

            party.Ideologies!.RemoveAll(i => !updatedIdeologies.Contains(i));
        }
    }

    public async Task FillMemberAsync(Member member, string countryKey,
        string languageCode, CancellationToken cancellationToken,
        string? searchCategory = null, string? nationalPageTitle = null)
    {
        nationalPageTitle ??= _options.NationalReferrals?
            .GetValueOrDefault($"{countryKey}/{member.Key}");

        Dictionary<string, string?> nationalPageQuery = new()
        {
            { "action", "query" },
            { "prop", "langlinks" },
            { "lllang", "en" },
            { "format", "json" },
            { "formatversion", "2" }
        };

        if (nationalPageTitle is null)
        {
            nationalPageQuery["generator"] = "search";

            nationalPageQuery["gsrsearch"] = member.Key +
                (searchCategory is null ?
                    string.Empty : $" incategory:\"{searchCategory}\"");

            nationalPageQuery["gsrlimit"] = "1";
        }
        else
        {
            nationalPageQuery["titles"] = nationalPageTitle;
        }

        JsonDocument nationalPageDocument = await GetDocumentAsync(
            languageCode, nationalPageQuery, cancellationToken);

        JsonElement nationalPageElement;

        try
        {
            nationalPageElement = nationalPageDocument.RootElement
                .GetProperty("query")
                .GetProperty("pages")[0];
        }
        catch (KeyNotFoundException)
        {
            return;
        }

        if (languageCode is "en")
        {
            try
            {
                member.Name = nationalPageElement
                    .GetProperty("title")
                    .GetString()!
                    .Split('(')[0];
            }
            catch (KeyNotFoundException) { }

            return;
        }

        try
        {
            member.Name = nationalPageElement
                .GetProperty("langlinks")[0]
                .GetProperty("title")
                .GetString()!
                .Split('(')[0];
        }
        catch (KeyNotFoundException) { }
    }

    async Task<JsonDocument> GetDocumentAsync(string languageCode,
        Dictionary<string, string?> query, CancellationToken cancellationToken)
    {
        string url = $"https://{languageCode}.wikipedia.org/w/api.php";

        Stream stream = await _http.GetStreamAsync(QueryHelpers
            .AddQueryString(url, query), cancellationToken);

        return await JsonDocument.ParseAsync(stream, default,
            cancellationToken);
    }

    HashSet<string> FilterCategories(IEnumerable<string> categories) =>
        categories
            .Where(c =>
                c.Contains("parties") ||
                c.Contains("organizations"))
            .Select(c => FilterCategory(c))
            .Where(c => !_options.ExcludedCategories?.Contains(c) ?? true)
            .ToHashSet();

    string FilterCategory(string category) =>
        string.Join(' ', category
            .Split(':')[1]
            .Split(' ')
            .TakeWhile(c =>
                c != "political" &&
                c != "parties" &&
                c != "organizations"));
}
