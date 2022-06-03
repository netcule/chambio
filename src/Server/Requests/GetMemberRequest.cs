using Chambio.Server.Entities;
using MediatR;

namespace Chambio.Server.Requests;

public record GetMemberRequest(int Id) : IRequest<Member>;
