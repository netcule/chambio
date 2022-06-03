using System.Text.Json;
using Chambio.Server.Abstractions;
using Chambio.Server.Entities;
using Chambio.Server.Enums;
using Chambio.Server.Helpers;
using Chambio.Server.Persistence;
using Chambio.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Workers;

public class USWorker : Worker
{
    readonly ChambioContext _context;

    readonly WikiService _wiki;

    readonly HttpClient _http;

    public USWorker(ChambioContext context, WikiService wiki,
        HttpClient http)
    {
        _context = context;
        _wiki = wiki;
        _http = http;
    }

    public override async Task RunAsync(CancellationToken cancellationToken)
    {
        Country? country = await _context.Countries
            .Include(c => c.Chambers!)
            .Include(c => c.Parties!)
            .ThenInclude(p => p.Ideologies!)
            .Include(c => c.Members!)
            .FirstOrDefaultAsync(c => c.Key == "US", cancellationToken);

        const string flagUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/a/a4/Flag_of_the_United_States.svg/256px-Flag_of_the_United_States.svg.png";
        const string symbolUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5b/Greater_coat_of_arms_of_the_United_States.svg/256px-Greater_coat_of_arms_of_the_United_States.svg.png";

        if (country is null)
        {
            country = new("US", CountryNames.US, LegislatureType.Bicameral,
                flagUrl, symbolUrl)
            {
                Chambers = new(),
                Parties = new(),
                Members = new()
            };

            await _context.Countries.AddAsync(country, cancellationToken);
        }
        else
        {
            country.FlagUrl = flagUrl;
            country.SymbolUrl = symbolUrl;
        }

        const string senateKey = "sen";
        const string senateName = "United States Senate";

        Chamber? senate = country.Chambers!
            .FirstOrDefault(c => c.Key == senateKey);

        if (senate is null)
        {
            senate = new(senateKey, senateName, HouseType.Upper);

            country.Chambers!.Add(senate);
        }

        const string houseKey = "rep";
        const string houseName = "United States House of Representatives";

        Chamber? house = country.Chambers!
            .FirstOrDefault(c => c.Key == houseKey);

        if (house is null)
        {
            house = new(houseKey, houseName, HouseType.Lower);

            country.Chambers!.Add(house);
        }

        await _context.SaveChangesAsync(cancellationToken);

        string membersUrl = "congress-legislators/legislators-current.json";

        Stream membersStream = await _http.GetStreamAsync(membersUrl,
            cancellationToken);

        JsonDocument membersDocument = await JsonDocument.ParseAsync(
            membersStream);

        IEnumerable<JsonElement> memberElements;

        try
        {
            memberElements = membersDocument.RootElement
                .EnumerateArray();
        }
        catch (KeyNotFoundException) { return; }

        HashSet<Party> updatedParties = new();
        HashSet<Member> updatedMembers = new();

        foreach (JsonElement memberElement in memberElements)
        {
            try
            {
                JsonElement memberTermElement = memberElement
                    .GetProperty("terms")
                    .EnumerateArray()
                    .Last(e => e
                        .GetProperty("end")
                        .GetDateTime() >= DateTime.Today);

                string chamberKey = memberTermElement
                    .GetProperty("type")
                    .GetString()!;

                Chamber chamber = country.Chambers!
                    .First(c => c.Key == chamberKey);

                string partyKey = memberTermElement
                    .GetProperty("party")
                    .GetString()! + " Party";

                Party? party = null;

                if (partyKey is not "Independent Party")
                {
                    string? partyWikiTitle = partyKey switch
                    {
                        "Republican Party" => "Republican Party (United States)",
                        "Democrat Party" => "Democratic Party (United States)",
                        _ => null
                    };

                    party = country.Parties!
                        .FirstOrDefault(p => p.Key == partyKey);

                    if (party is null)
                    {
                        party = new(partyKey, partyKey)
                        {
                            Ideologies = new()
                        };

                        country.Parties!.Add(party);
                    }

                    if (updatedParties.Add(party))
                        await _wiki.FillPartyAsync(party, country.Key, "en",
                            cancellationToken, "Political parties in the United States",
                            partyWikiTitle);

                    await _context.SaveChangesAsync(cancellationToken);
                }

                string memberKey = memberElement
                    .GetProperty("name")
                    .GetProperty("official_full")
                    .GetString()!;

                Gender? memberGender = null;

                try
                {
                    memberGender = memberElement
                        .GetProperty("bio")
                        .GetProperty("gender")
                        .GetString()! switch
                        {
                            "M" => Gender.Male,
                            "F" => Gender.Female,
                            _ => null
                        };
                }
                catch (KeyNotFoundException) { }

                Member? member = country.Members!.FirstOrDefault(m =>
                    m.Key == memberKey);

                if (member is null)
                {
                    member = new(memberKey, memberKey, memberGender)
                    {
                        Party = party,
                        Chamber = chamber
                    };

                    country.Members!.Add(member);
                }
                else
                {
                    member.Gender = memberGender;
                    member.Party = party;
                    member.Chamber = chamber;
                }

                string? memberWikiTitle = null;

                try
                {
                    memberWikiTitle = memberElement
                       .GetProperty("id")
                       .GetProperty("wikipedia")
                       .GetString()!;
                }
                catch (KeyNotFoundException) { }

                if (updatedMembers.Add(member))
                    await _wiki.FillMemberAsync(member, country.Key, "en",
                        cancellationToken, null, memberWikiTitle);

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (KeyNotFoundException) { }
        }

        country.Members!.RemoveAll(m => !updatedMembers.Contains(m));

        country.Parties!.RemoveAll(p => !updatedParties.Contains(p));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
