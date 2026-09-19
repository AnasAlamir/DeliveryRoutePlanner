using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace DeliveryRoutePlanner.Entities
{
    public class Delivery
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("area")]
        public string Area { get; set; }
        [JsonPropertyName("priority")]
        public int Priority { get; set; }
        [JsonPropertyName("packageWeight")]
        public double PackageWeight { get; set; }

        public override string? ToString()
        {
            return $"Id = {Id,3}, Area = {Area,-10}, Priority = {Priority,3}, Package Weight = {PackageWeight,6:F2}";
        }
    }
}
