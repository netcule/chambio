using Chambio.Server.Entities;
using MediatR;

namespace Chambio.Server.Requests;

public record ListCountriesRequest() : IRequest<List<Country>>;
