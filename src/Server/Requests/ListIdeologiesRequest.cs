using Chambio.Server.Entities;
using MediatR;

namespace Chambio.Server.Requests;

public record ListIdeologiesRequest(string Name) : IRequest<List<Ideology>>;
