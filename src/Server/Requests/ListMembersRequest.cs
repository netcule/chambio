using Chambio.Server.Entities;
using MediatR;

namespace Chambio.Server.Requests;

public record ListMembersRequest(string Name) : IRequest<List<Member>>;
