using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Runtime.Serialization.Json;
using GCloudPhone.Models;
using System.Text.Json;

namespace GCloudPhone
{
    public class OrderTools
    {
        private async Task DeleteOrdersAsync(SqliteConnection sqliteConn)
        {
            using (var command = sqliteConn.CreateCommand())
            {
                command.CommandText = "DELETE FROM OrderItems";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Orders";
                await command.ExecuteNonQueryAsync();
                
            }
        }
        private async Task SaveOrderAsync<T>(SqliteConnection sqliteConn, T item)
        {
            // Beispielhafte Methode zum Speichern eines Elements in die SQLite-Datenbank
            try
            {
                using (var transaction = sqliteConn.BeginTransaction())
                {
                    using (var command = sqliteConn.CreateCommand())
                    {
                        if (typeof(T) == typeof(Orders))
                        {
                            var order = item as Orders;
                            command.CommandText = @"INSERT INTO Orders (OrderID, UserID, OrderDate, DeliveryDate, DeliveryTime, DeliveryAddress, DeliveryCity, DeliveryZip, DeliveryCountry, DeliveryPhone, DeliveryEmail, DeliveryContact, DeliveryNotes, PaymentMethod, PaymentStatus, OrderStatus, TotalAmount, TotalVAT, Reference, Idc)
                                            VALUES (@OrderID, @UserID, @OrderDate, @DeliveryDate, @DeliveryTime, @DeliveryAddress, @DeliveryCity, @DeliveryZip, @DeliveryCountry, @DeliveryPhone, @DeliveryEmail, @DeliveryContact, @DeliveryNotes, @PaymentMethod, @PaymentStatus, @OrderStatus, @TotalAmount, @TotalVAT, @Reference, @Idc)";
                            command.Parameters.AddWithValue("@OrderID", order.OrderID);
                            command.Parameters.AddWithValue("@UserID", order.UserID);
                            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                            command.Parameters.AddWithValue("@DeliveryDate", order.DeliveryDate);
                            command.Parameters.AddWithValue("@DeliveryTime", order.DeliveryTime);
                            command.Parameters.AddWithValue("@DeliveryAddress", order.DeliveryAddress);
                            command.Parameters.AddWithValue("@DeliveryCity", order.DeliveryCity);
                            command.Parameters.AddWithValue("@DeliveryZip", order.DeliveryZip);
                            command.Parameters.AddWithValue("@DeliveryCountry", order.DeliveryCountry);
                            command.Parameters.AddWithValue("@DeliveryPhone", order.DeliveryPhone);
                            command.Parameters.AddWithValue("@DeliveryEmail", order.DeliveryEmail);
                            command.Parameters.AddWithValue("@DeliveryContact", order.DeliveryContact);
                            command.Parameters.AddWithValue("@DeliveryNotes", order.DeliveryNotes);
                            command.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod);
                            command.Parameters.AddWithValue("@PaymentStatus", order.PaymentStatus);
                            command.Parameters.AddWithValue("@OrderStatus", order.OrderStatus);
                            command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                            command.Parameters.AddWithValue("@TotalVAT", order.TotalVAT);
                            command.Parameters.AddWithValue("@Reference", order.Reference);
                        }
                        else if (typeof(T) == typeof(OrderItems))
                        {
                            var orderItem = item as OrderItems;
                            command.CommandText = @"INSERT INTO OrderItems (OrderID, ProductID, ProductDescription1, ProductDescription2, Amount, Quantity, VAT, Reference)
                                            VALUES (@OrderID, @ProductID, @ProductDescription1, @ProductDescription2, @Amount, @Quantity, @VAT, @Reference)";
                            command.Parameters.AddWithValue("@OrderID", orderItem.OrderID);
                            command.Parameters.AddWithValue("@ProductID", orderItem.ProductID);
                            command.Parameters.AddWithValue("@ProductDescription1", orderItem.ProductDescription1);
                            command.Parameters.AddWithValue("@ProductDescription2", orderItem.ProductDescription2);
                            command.Parameters.AddWithValue("@Amount", orderItem.Amount);
                            command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);
                            command.Parameters.AddWithValue("@VAT", orderItem.VAT);
                            command.Parameters.AddWithValue("@Reference", orderItem.Reference);
                        }
                        await command.ExecuteNonQueryAsync();
                    }
                    await transaction.CommitAsync();
                }
            }
            catch (Exception)
            {
                // Fehlerbehandlung
                //Console.WriteLine($"Fehler beim Speichern des Elements: {ex.Message}");
            }
        }

        public async Task<Orders> GetOrderAsync(SqliteConnection sqliteConn, string orderId)
        {
            Orders order = null;

            using (var command = sqliteConn.CreateCommand())
            {
                command.CommandText = @"SELECT OrderID, UserID, OrderDate, DeliveryDate, DeliveryTime, DeliveryAddress, 
                                DeliveryCity, DeliveryZip, DeliveryCountry, DeliveryPhone, DeliveryEmail, 
                                DeliveryContact, DeliveryNotes, PaymentMethod, PaymentStatus, OrderStatus, 
                                TotalAmount, TotalVAT,  OrderType, Reference, Tip
                                FROM Orders WHERE OrderID = @OrderID";
                command.Parameters.AddWithValue("@OrderID", orderId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        order = new Orders
                        {
                            OrderID = reader.GetString(reader.GetOrdinal("OrderID")),
                            UserID = reader.GetString(reader.GetOrdinal("UserID")),
                            OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                            DeliveryDate = reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
                            DeliveryTime = reader.IsDBNull(reader.GetOrdinal("DeliveryTime")) ? null : reader.GetString(reader.GetOrdinal("DeliveryTime")),
                            DeliveryAddress = reader.IsDBNull(reader.GetOrdinal("DeliveryAddress")) ? null : reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                            DeliveryCity = reader.IsDBNull(reader.GetOrdinal("DeliveryCity")) ? null : reader.GetString(reader.GetOrdinal("DeliveryCity")),
                            DeliveryZip = reader.IsDBNull(reader.GetOrdinal("DeliveryZip")) ? null : reader.GetString(reader.GetOrdinal("DeliveryZip")),
                            DeliveryCountry = reader.IsDBNull(reader.GetOrdinal("DeliveryCountry")) ? null : reader.GetString(reader.GetOrdinal("DeliveryCountry")),
                            DeliveryPhone = reader.IsDBNull(reader.GetOrdinal("DeliveryPhone")) ? null : reader.GetString(reader.GetOrdinal("DeliveryPhone")),
                            DeliveryEmail = reader.IsDBNull(reader.GetOrdinal("DeliveryEmail")) ? null : reader.GetString(reader.GetOrdinal("DeliveryEmail")),
                            DeliveryContact = reader.IsDBNull(reader.GetOrdinal("DeliveryContact")) ? null : reader.GetString(reader.GetOrdinal("DeliveryContact")),
                            DeliveryNotes = reader.IsDBNull(reader.GetOrdinal("DeliveryNotes")) ? null : reader.GetString(reader.GetOrdinal("DeliveryNotes")),
                            PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                            PaymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus")) ? null : reader.GetString(reader.GetOrdinal("PaymentStatus")),
                            OrderStatus = reader.IsDBNull(reader.GetOrdinal("OrderStatus")) ? null : reader.GetString(reader.GetOrdinal("OrderStatus")),
                            TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                            TotalVAT = reader.IsDBNull(reader.GetOrdinal("TotalVAT")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TotalVAT")),
                            OrderType = reader.GetString(reader.GetOrdinal("OrderType")),
                            Reference = reader.IsDBNull(reader.GetOrdinal("Reference")) ? null : reader.GetString(reader.GetOrdinal("Reference")),
                            Tip = reader.IsDBNull(reader.GetOrdinal("Tip")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Tip"))

                        };
                    }
                }
            }

            return order;
        }

        public async Task<List<OrderItems>> GetOrderItemsAsync(SqliteConnection sqliteConn, string orderId)
        {
            var orderItems = new List<OrderItems>();

            using (var command = sqliteConn.CreateCommand())
            {
                command.CommandText = @"SELECT Idc, OrderID, ProductID, ProductDescription1, ProductDescription2, 
                                Amount, Quantity, VAT, Reference 
                                FROM OrderItems WHERE OrderID = @OrderID";
                command.Parameters.AddWithValue("@OrderID", orderId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var orderItem = new OrderItems
                        {
                            Idc = reader.GetInt32(reader.GetOrdinal("Idc")),
                            OrderID = reader.GetString(reader.GetOrdinal("OrderID")),
                            ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                            ProductDescription1 = reader.IsDBNull(reader.GetOrdinal("ProductDescription1")) ? null : reader.GetString(reader.GetOrdinal("ProductDescription1")),
                            ProductDescription2 = reader.IsDBNull(reader.GetOrdinal("ProductDescription2")) ? null : reader.GetString(reader.GetOrdinal("ProductDescription2")),
                            Amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Amount")),
                            Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Quantity")),
                            VAT = reader.IsDBNull(reader.GetOrdinal("VAT")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("VAT")),
                            Reference = reader.IsDBNull(reader.GetOrdinal("Reference")) ? null : reader.GetString(reader.GetOrdinal("Reference"))
                        };
                        orderItems.Add(orderItem);
                    }
                }
            }

            return orderItems;
        }

        public async Task<OrderWithItems> GetOrderWithItemsAsync(SqliteConnection sqliteConn, string orderId)
        {
            var orderWithItems = new OrderWithItems();
            orderWithItems.Items = new List<OrderItems>();

            using (sqliteConn)
            {
                await sqliteConn.OpenAsync();

                // Abfrage für die Bestellung mit expliziten Spalten
                using (var orderCommand = sqliteConn.CreateCommand())
                {
                    orderCommand.CommandText = @"
                SELECT OrderID, UserID, StoreID, OrderDate, DeliveryDate, DeliveryTime, DeliveryAddress, DeliveryCity, 
                       DeliveryZip, DeliveryCountry, DeliveryPhone, DeliveryEmail, DeliveryContact, DeliveryNotes, 
                       PaymentMethod, PaymentStatus, OrderStatus, TotalAmount, TotalVAT, OrderType, Reference, Tip 
                FROM Orders 
                WHERE OrderID = @OrderID";
                    orderCommand.Parameters.AddWithValue("@OrderID", orderId);

                    using (var reader = await orderCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            orderWithItems.Order = new Orders
                            {
                                OrderID = reader.GetString(reader.GetOrdinal("OrderID")),
                                UserID = reader.GetString(reader.GetOrdinal("UserID")),
                                StoreID = reader.GetString(reader.GetOrdinal("StoreID")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                                DeliveryDate = reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
                                DeliveryTime = reader.IsDBNull(reader.GetOrdinal("DeliveryTime")) ? null : reader.GetString(reader.GetOrdinal("DeliveryTime")),
                                DeliveryAddress = reader.IsDBNull(reader.GetOrdinal("DeliveryAddress")) ? null : reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                                DeliveryCity = reader.IsDBNull(reader.GetOrdinal("DeliveryCity")) ? null : reader.GetString(reader.GetOrdinal("DeliveryCity")),
                                DeliveryZip = reader.IsDBNull(reader.GetOrdinal("DeliveryZip")) ? null : reader.GetString(reader.GetOrdinal("DeliveryZip")),
                                DeliveryCountry = reader.IsDBNull(reader.GetOrdinal("DeliveryCountry")) ? null : reader.GetString(reader.GetOrdinal("DeliveryCountry")),
                                DeliveryPhone = reader.IsDBNull(reader.GetOrdinal("DeliveryPhone")) ? null : reader.GetString(reader.GetOrdinal("DeliveryPhone")),
                                DeliveryEmail = reader.IsDBNull(reader.GetOrdinal("DeliveryEmail")) ? null : reader.GetString(reader.GetOrdinal("DeliveryEmail")),
                                DeliveryContact = reader.IsDBNull(reader.GetOrdinal("DeliveryContact")) ? null : reader.GetString(reader.GetOrdinal("DeliveryContact")),
                                DeliveryNotes = reader.IsDBNull(reader.GetOrdinal("DeliveryNotes")) ? null : reader.GetString(reader.GetOrdinal("DeliveryNotes")),
                                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                PaymentStatus = reader.IsDBNull(reader.GetOrdinal("PaymentStatus")) ? null : reader.GetString(reader.GetOrdinal("PaymentStatus")),
                                OrderStatus = reader.IsDBNull(reader.GetOrdinal("OrderStatus")) ? null : reader.GetString(reader.GetOrdinal("OrderStatus")),
                                TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                TotalVAT = reader.IsDBNull(reader.GetOrdinal("TotalVAT")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TotalVAT")),
                                OrderType = reader.GetString(reader.GetOrdinal("OrderType")),
                                Reference = reader.IsDBNull(reader.GetOrdinal("Reference")) ? null : reader.GetString(reader.GetOrdinal("Reference")),
                                Tip = reader.IsDBNull(reader.GetOrdinal("Tip")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Tip"))
                            };
                        }
                    }
                }

                // Abfrage für die Bestellpositionen mit expliziten Spalten
                using (var itemsCommand = sqliteConn.CreateCommand())
                {
                    itemsCommand.CommandText = @"
                SELECT Idc, OrderID, ProductID, ProductDescription1, ProductDescription2, Amount, Quantity, VAT, 
                       Reference, ItemNote 
                FROM OrderItems 
                WHERE OrderID = @OrderID";
                    itemsCommand.Parameters.AddWithValue("@OrderID", orderId);

                    using (var reader = await itemsCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new OrderItems
                            {
                                Idc = reader.GetInt32(reader.GetOrdinal("Idc")),
                                OrderID = reader.GetString(reader.GetOrdinal("OrderID")),
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                ProductDescription1 = reader.IsDBNull(reader.GetOrdinal("ProductDescription1")) ? null : reader.GetString(reader.GetOrdinal("ProductDescription1")),
                                ProductDescription2 = reader.IsDBNull(reader.GetOrdinal("ProductDescription2")) ? null : reader.GetString(reader.GetOrdinal("ProductDescription2")),
                                Amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Amount")),
                                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Quantity")),
                                VAT = reader.IsDBNull(reader.GetOrdinal("VAT")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("VAT")),
                                Reference = reader.IsDBNull(reader.GetOrdinal("Reference")) ? null : reader.GetString(reader.GetOrdinal("Reference")),
                                ItemNote = reader.IsDBNull(reader.GetOrdinal("ItemNote")) ? null : reader.GetString(reader.GetOrdinal("ItemNote"))
                            };
                            orderWithItems.Items.Add(item);
                        }
                    }
                }
            }

            return orderWithItems;
        }



        public static string SerializeOrderWithItemsAsync(OrderWithItems orderWithItems)
        {
            var serializer = new DataContractJsonSerializer(typeof(OrderWithItems));
            string orderWithItemsJson;
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, orderWithItems);
                orderWithItemsJson = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            var envelope = new MessageEnvelope
            {
                MessageType = "OrderWithItems",
                Data = orderWithItemsJson
            };

            string envelopeJson;
            var envelopeSerializer = new DataContractJsonSerializer(typeof(MessageEnvelope));
            using (var envelopeMemoryStream = new MemoryStream())
            {
                envelopeSerializer.WriteObject(envelopeMemoryStream, envelope);
                envelopeJson = Encoding.UTF8.GetString(envelopeMemoryStream.ToArray());
            }

            return envelopeJson;
        }

        public static string SerializeInfoMessage(string message)
        {
            var infoMessage = new InfoMessage
            {
                Message = message
            };

            // Serialisieren von InfoMessage in einen JSON-String
            string serializedMessage = JsonSerializer.Serialize(infoMessage);

            // Erstellen einer Instanz von MessageEnvelope
            var messageEnvelope = new MessageEnvelope
            {
                MessageType = "InfoMessage",
                Data = serializedMessage
            };

            // Serialisieren von MessageEnvelope in einen JSON-String
            string serializedEnvelope = JsonSerializer.Serialize(messageEnvelope);


            return serializedEnvelope;
        }

        public async Task<string> SerializeOrderWithItemsAsync(SqliteConnection sqliteConn, string orderId)
        {
            var orderWithItems = await GetOrderWithItemsAsync(sqliteConn, orderId);

            return SerializeOrderWithItemsAsync(orderWithItems);
        }

        public static string GetShortId()
        {
            Thread.Sleep(6);

            DateTime baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime currentUtc = DateTime.UtcNow;

            long totalMilliseconds = (long)(currentUtc - baseDate).TotalMilliseconds;

            long currentSeconds = totalMilliseconds / 10;

            long combined = currentSeconds * 32;

            string base64 = Base64Encode(combined);

            string padded = new string('0', 7) + base64;
            string shortId = padded.Substring(padded.Length - 7, 7);

            return shortId;
        }

        public static string Base64Encode(long value)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            string result = string.Empty;

            while (value > 0)
            {
                int index = (int)(value % 64);
                result = chars[index] + result;
                value /= 64;
            }

            return result;
        }
    }

}
