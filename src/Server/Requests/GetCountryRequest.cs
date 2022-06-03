using Chambio.Server.Entities;
using MediatR;

namespace Chambio.Server.Requests;

public record GetCountryRequest(int Id) : IRequest<Country>;
