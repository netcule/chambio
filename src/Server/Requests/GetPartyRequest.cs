using Chambio.Server.Entities;
using MediatR;

namespace Chambio.Server.Requests;

public record GetPartyRequest(int Id) : IRequest<Party>;
