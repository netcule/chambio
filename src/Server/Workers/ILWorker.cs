using System.Text.Json;
using Chambio.Server.Abstractions;
using Chambio.Server.Entities;
using Chambio.Server.Enums;
using Chambio.Server.Helpers;
using Chambio.Server.Persistence;
using Chambio.Server.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Chambio.Server.Workers;

public class ILWorker : Worker
{
    readonly ChambioContext _context;

    readonly WikiService _wiki;

    readonly HttpClient _http;

    public ILWorker(ChambioContext context, WikiService wiki,
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
            .FirstOrDefaultAsync(c => c.Key == "IL", cancellationToken);

        const string flagUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d4/Flag_of_Israel.svg/256px-Flag_of_Israel.svg.png";
        const string symbolUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8f/Emblem_of_Israel.svg/256px-Emblem_of_Israel.svg.png";

        if (country is null)
        {
            country = new("IL", CountryNames.IL, LegislatureType.Unicameral,
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

        const string knessetKey = "כנסת";
        const string knessetName = "Knesset";

        Chamber? knesset = country.Chambers!
            .FirstOrDefault(c => c.Key == knessetKey);

        if (knesset is null)
        {
            knesset = new(knessetKey, knessetName, null);

            country.Chambers!.Add(knesset);
        }

        await _context.SaveChangesAsync(cancellationToken);

        Dictionary<string, string?> membersQuery = new()
        {
            { "$filter", "IsCurrent eq true and PositionID eq 54" },
            { "$expand", "KNS_Person" },
            { "$select", "KNS_Person/LastName,KNS_Person/FirstName,KNS_Person/GenderID,FactionName" }
        };

        string membersUrl = QueryHelpers
            .AddQueryString("KNS_PersonToPosition", membersQuery);

        IEnumerable<JsonElement> memberElements = Enumerable
            .Empty<JsonElement>();

        while (true)
        {
            Stream membersStream = await _http
                .GetStreamAsync(membersUrl, cancellationToken);

            JsonDocument membersDocument = await JsonDocument
                .ParseAsync(membersStream, default, cancellationToken);

            try
            {
                memberElements = memberElements
                    .Concat(membersDocument.RootElement
                        .GetProperty("value")
                        .EnumerateArray());
            }
            catch (IndexOutOfRangeException) { }

            bool nextLinkExists = membersDocument.RootElement
                .TryGetProperty("odata.nextLink", out JsonElement nextLink);

            if (!nextLinkExists)
                break;

            membersUrl = nextLink.GetString()!;
        }

        HashSet<Party> updatedParties = new();
        HashSet<Member> updatedMembers = new();

        foreach (JsonElement memberElement in memberElements)
        {
            try
            {
                string partyKey = memberElement
                    .GetProperty("FactionName")
                    .GetString()!;

                Party? party = null;

                if (!partyKey.Contains("ח\"כ יחיד"))
                {
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
                        await _wiki.FillPartyAsync(party, country.Key, "he",
                            cancellationToken, "סיעות בכנסת");

                    await _context.SaveChangesAsync(cancellationToken);
                }

                JsonElement memberInfoElement = memberElement
                    .GetProperty("KNS_Person");

                string memberKey = memberInfoElement
                    .GetProperty("FirstName")
                    .GetString() + ' ' + memberInfoElement
                    .GetProperty("LastName")
                    .GetString();

                Gender? memberGender = null;

                try
                {
                    memberGender = memberInfoElement
                        .GetProperty("GenderID")
                        .GetInt32() switch
                        {
                            251 => Gender.Male,
                            250 => Gender.Female,
                            _ => null
                        };
                }
                catch (KeyNotFoundException) { }

                Member? member = country.Members!
                    .FirstOrDefault(m => m.Key == memberKey);

                if (member is null)
                {
                    member = new(memberKey, memberKey, memberGender)
                    {
                        Party = party,
                        Chamber = knesset
                    };

                    country.Members!.Add(member);
                }
                else
                {
                    member.Gender = memberGender;
                    member.Party = party;
                    member.Chamber = knesset;
                }

                if (updatedMembers.Add(member))
                    await _wiki.FillMemberAsync(member, country.Key, "he",
                        cancellationToken, "רשימת חברי הכנסת");

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (KeyNotFoundException) { }
        }

        country.Members!.RemoveAll(m => !updatedMembers.Contains(m));

        country.Parties!.RemoveAll(p => !updatedParties.Contains(p));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
