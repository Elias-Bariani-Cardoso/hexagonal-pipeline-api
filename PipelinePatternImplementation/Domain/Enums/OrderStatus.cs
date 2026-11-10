using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Enums
{
    public enum OrderStatus
    {
        Created = 0,
        Validated = 1,
        Paid = 2,
        Shipped = 3,
        Invalid = 4,
        Failed = 5
    }

    public static class OrderStatusConverter
    {
        private static readonly IReadOnlyDictionary<string, OrderStatus>
            _fromText = Enum.GetValues<OrderStatus>()
                .ToDictionary(
                    status => status.ToString(),
                    status => status,
                    StringComparer.OrdinalIgnoreCase);

        private static readonly IReadOnlyDictionary<OrderStatus, string>
            _toText = Enum.GetValues<OrderStatus>()
                .ToDictionary(
                    status => status,
                    status => status.ToString());

        public static bool TryFromText(
            string? value,
            out OrderStatus status)
        {
            status = default;

            return !string.IsNullOrWhiteSpace(value)
                && _fromText.TryGetValue(value.Trim(), out status);
        }

        public static OrderStatus FromText(string? value)
        {
            if (TryFromText(value, out var status))
            {
                return status;
            }

            throw new ArgumentException(
                $"Status do pedido inválido: '{value}'. " +
                $"Valores permitidos: {string.Join(", ", _fromText.Keys)}.",
                nameof(value));
        }

        public static string ToText(this OrderStatus status)
        {
            if (_toText.TryGetValue(status, out var text))
            {
                return text;
            }

            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Status do pedido não reconhecido.");
        }
    }
}