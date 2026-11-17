using Adapter.InBound.HttpAdapter.DTOs.Xml;
using Adapter.InBound.HttpAdapter.Mappers;
using Domain.Enums;
using Domain.Models.Xml;
using Domain.Ports.InBound;
using Microsoft.AspNetCore.Mvc;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Adapter.InBound.HttpAdapter.Ports
{
    public static class XmlEndpoint
    {
        public static IEndpointRouteBuilder MapXmlEndpoint(
            this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints
                .MapGroup("/xml")
                .WithTags("Xml");

            group.MapPost("/", ExecutarXmlAsync)
                .WithName("ExecutarXml")
                .Produces<List<XmlResponse>>(
                    StatusCodes.Status200OK)
                .Produces<BaseError>(
                    StatusCodes.Status400BadRequest)
                .Produces<BaseError>(
                    StatusCodes.Status500InternalServerError);

            return endpoints;
        }

        private static async Task<IResult> ExecutarXmlAsync(
            [FromServices] IXmlUseCase useCase,
            CancellationToken cancellationToken)
        {
            BaseReturn<List<Xml>> retorno =
                await useCase.ExecutarAsync(cancellationToken);

            if (retorno.Status == EnumStatus.SUCESSO)
            {
                if (retorno.SuccessObject is not { } registros)
                {
                    return Results.Problem(
                        title: "A consulta retornou sucesso sem uma lista de XMLs.",
                        statusCode:
                            StatusCodes.Status500InternalServerError);
                }

                List<XmlResponse> response = registros
                    .Select(XmlMapper.XmlDomainToResponseMapper)
                    .ToList();

                return Results.Ok(response);
            }

            return retorno.Status switch
            {
                EnumStatus.NEGOCIO => Results.Json(
                    retorno.ErrorObject,
                    statusCode: StatusCodes.Status400BadRequest),

                EnumStatus.SISTEMA => Results.Json(
                    retorno.ErrorObject,
                    statusCode:
                        StatusCodes.Status500InternalServerError),

                _ => Results.Problem(
                    title: "Status de retorno não reconhecido.",
                    statusCode:
                        StatusCodes.Status500InternalServerError)
            };
        }
    }
}