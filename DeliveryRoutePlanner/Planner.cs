using DeliveryRoutePlanner.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeliveryRoutePlanner
{
    public class Planner
    {
        private const double MaxVehicleCapacity = 10.0;

        private SortedDictionary<int, List<Delivery>> DeliveriesGroupedByPriority { get; set; } = new();

        private IEnumerable<Delivery> LoadDelevieries(string fileName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDir, fileName);
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            IEnumerable<Delivery> deliveries = JsonSerializer.Deserialize<List<Delivery>>(json, options) ?? [];
            return deliveries;
        }

        public void PlanDeliveries(string fileName)
        {
            List<Delivery> deliveries = LoadDelevieries(fileName).ToList();

            if (!deliveries.Any())
            {
                Console.WriteLine("No deliveries found.");
                return;
            }

            RemoveOversizedDeliveries(deliveries);

            List<Trip> trips = OrganizeDeliveriesToTrips(deliveries);

            DisplayTrips(trips);
        }

        private List<Trip> OrganizeDeliveriesToTrips(IEnumerable<Delivery> deliveries)
        {
            var sortedDeliveries = deliveries
                                        .OrderBy(d => d.Priority)
                                        .ThenBy(d => d.Area)
                                        .ThenByDescending(d => d.PackageWeight)
                                        .ToList();

            DeliveriesGroupedByPriority = new SortedDictionary<int, List<Delivery>>(
                                sortedDeliveries
                                    .GroupBy(d => d.Priority)
                                    .ToDictionary(
                                        g => g.Key,
                                        g => g.ToList()
                                    )
                );


            List<Trip> tripResults = new List<Trip>();

            while (DeliveriesGroupedByPriority.Any())
            {
                Trip newTrip = new Trip
                {
                    Name = $"Trip {tripResults.Count() + 1}",
                    Capacity = 0,
                    Deliveries = new List<Delivery>()
                };

                foreach (var currentGroup in DeliveriesGroupedByPriority)
                {
                    List<Delivery> currentGroupDeliveries = currentGroup.Value;
                    var deleveriesThatFit = currentGroupDeliveries.Where(d => d.PackageWeight + newTrip.Capacity <= MaxVehicleCapacity).ToList();

                    while (newTrip.Capacity < MaxVehicleCapacity)
                    {
                        var deliveryToAdd = deleveriesThatFit.FirstOrDefault(d => d.PackageWeight + newTrip.Capacity <= MaxVehicleCapacity);

                        if (deliveryToAdd == null)
                            break;

                        HashSet<string> newTripAreas = newTrip.Deliveries.Select(d => d.Area).ToHashSet();
                        var candidatesFromExistingAreas = deleveriesThatFit.Where(d => newTripAreas.Contains(d.Area)).ToList();

                        if (candidatesFromExistingAreas.Any())
                        {
                            while (newTrip.Capacity < MaxVehicleCapacity)
                            {
                                var deliveryFromSameAreaToAdd = candidatesFromExistingAreas
                                                                        .FirstOrDefault(d => d.PackageWeight + newTrip.Capacity <= MaxVehicleCapacity);

                                if (deliveryFromSameAreaToAdd == null)
                                    break;

                                AddDeliveryToTrip(deliveryFromSameAreaToAdd, newTrip, candidatesFromExistingAreas);
                                deleveriesThatFit.Remove(deliveryFromSameAreaToAdd);
                            }
                        }
                        else
                        {
                            AddDeliveryToTrip(deliveryToAdd, newTrip, deleveriesThatFit);
                        }
                    }
                }
                DeliveriesGroupedByPriority.Where(g => !g.Value.Any()).ToList().ForEach(g => DeliveriesGroupedByPriority.Remove(g.Key));

                tripResults.Add(newTrip);
            }
            return tripResults;
        }

        private void AddDeliveryToTrip(Delivery delivery, Trip trip, List<Delivery> deliveriesToRemoveFrom)
        {
            trip.Deliveries.Add(delivery);
            trip.Capacity += delivery.PackageWeight;
            deliveriesToRemoveFrom.Remove(delivery);
            DeliveriesGroupedByPriority[delivery.Priority].Remove(delivery);
        }

        private void RemoveOversizedDeliveries(List<Delivery> deliveries)
        {
            var delevieriesDoesntFit = deliveries.Where(d => d.PackageWeight > MaxVehicleCapacity).ToList();
            while (delevieriesDoesntFit.Any())
            {
                var delivery = delevieriesDoesntFit.First();
                Console.WriteLine($"Delivery [ID: {delivery.Id}, PackageWeight: {delivery.PackageWeight}] exceeds the maximum" +
                       $" vehicle capacity and will be removed.\n");
                deliveries.Remove(delivery);
                delevieriesDoesntFit.Remove(delivery);
            }
        }

        private void DisplayTrips(List<Trip> trips)
        {
            foreach (var trip in trips)
            {
                Console.WriteLine($"{trip}\n");
                foreach (var delivery in trip.Deliveries)
                {
                    Console.WriteLine($"\t{delivery}");
                }
                Console.Write("\n\n");
            }
        }
    }
}