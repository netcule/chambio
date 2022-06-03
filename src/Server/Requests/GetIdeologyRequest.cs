using Chambio.Server.Entities;
using MediatR;

namespace Chambio.Server.Requests;

public record GetIdeologyRequest(int Id) : IRequest<Ideology>;
