using Adapter.InBound.HttpAdapter.DTOs.Order;
using Adapter.InBound.HttpAdapter.Mappers;
using Domain.Enums;
using Domain.Models;
using Domain.Ports.InBound;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Adapter.InBound.HttpAdapter.Ports
{
    public static class OrderEndpoint
    {
        public static IEndpointRouteBuilder MapTransacoesEndpoint(
            this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints
                .MapGroup("/orders")
                .WithTags("Orders");

            group.MapPost("/", ExecutarTransacaoAsync)
                .WithName("ExecutarTransacao")
                .Produces<OrderResponse>(StatusCodes.Status200OK)
                .Produces<BaseError>(StatusCodes.Status400BadRequest)
                .Produces<BaseError>(
                    StatusCodes.Status500InternalServerError);

            return endpoints;
        }

        private static async Task<IResult> ExecutarTransacaoAsync(
            [FromBody] OrderRequest request,
            [FromServices] IOrderUseCase useCase)
        {
            Order order;

            try
            {
                order = OrderMapper.OrderRequestToDomainMapper(request);
            }
            catch (ArgumentException ex)
            {
                return Results.Json(
                    new BaseError(
                        EnumStatus.NEGOCIO,
                        1000,
                        ex.Message),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            BaseReturn<Order> retorno =
                await useCase.ExecutarAsync(order);

            if (retorno.Status == EnumStatus.SUCESSO)
            {
                if (retorno.SuccessObject is not { } processedOrder)
                {
                    return Results.Problem(
                        title: "O processamento retornou sucesso sem um pedido.",
                        statusCode:
                            StatusCodes.Status500InternalServerError);
                }

                OrderResponse response =
                    OrderMapper.OrderDomainToResponseMapper(processedOrder);

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