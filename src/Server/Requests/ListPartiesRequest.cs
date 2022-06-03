using Chambio.Server.Entities;
using MediatR;

namespace Chambio.Server.Requests;

public record ListPartiesRequest(string Name) : IRequest<List<Party>>;
