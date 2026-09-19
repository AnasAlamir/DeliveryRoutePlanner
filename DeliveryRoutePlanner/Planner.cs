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
            IEnumerable<Delivery> deliveries = JsonSerializer.Deserialize<List<Delivery>>(json, options) ?? Enumerable.Empty<Delivery>();
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

            int totalDeliveries = deliveries.Count;
            List<Delivery> rejectedDeliveries = RemoveInvalidDeliveries(deliveries);

            List<Trip> trips = OrganizeDeliveriesToTrips(deliveries);

            DisplayTrips(trips);

            DisplaySummary(totalDeliveries, rejectedDeliveries, trips);
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

        private List<Delivery> RemoveInvalidDeliveries(List<Delivery> deliveries)
        {
            // Collect invalid deliveries first (avoid mutating while enumerating)
            var invalidDeliveries = deliveries
                .Where(d => d == null
                            || d.PackageWeight < 0
                            || d.PackageWeight > MaxVehicleCapacity
                            || string.IsNullOrWhiteSpace(d.Area)
                            || d.Priority < 0)
                .ToList();

            var removed = new List<Delivery>();

            foreach (var delivery in invalidDeliveries)
            {
                if (delivery == null)
                {
                    Console.WriteLine("Found null delivery entry in the input; it will be removed.\n");
                    deliveries.Remove(null);
                    continue;
                }

                var reasons = new List<string>();
                if(delivery.Id < 0)
                    reasons.Add($"negative ID ({delivery.Id})");
                if (delivery.PackageWeight <= 0)
                    reasons.Add($"package weight ({delivery.PackageWeight}, must be positive)");
                if (delivery.PackageWeight > MaxVehicleCapacity)
                    reasons.Add($"package weight ({delivery.PackageWeight}) exceeds maximum capacity ({MaxVehicleCapacity})");
                if (string.IsNullOrWhiteSpace(delivery.Area))
                    reasons.Add("missing or empty area");
                if (delivery.Priority < 0)
                    reasons.Add($"negative priority ({delivery.Priority})");

                string reasonText = string.Join("; ", reasons);
                Console.WriteLine($"Delivery [ID: {delivery.Id}] is invalid: {reasonText}. It will be removed.\n");

                // Remove from the main list and add to removed list
                if (deliveries.Remove(delivery))
                {
                    removed.Add(delivery);
                }
            }

            return removed;
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

        private void DisplaySummary(int totalDeliveries, List<Delivery> rejectedDeliveries, List<Trip> trips)
        {
            int rejected = rejectedDeliveries.Count;
            int delivered = trips.Sum(t => t.Deliveries.Count);
            int totalTrips = trips.Count;
            double totalWeight = trips.Sum(t => t.Deliveries.Sum(d => d.PackageWeight));
            double averageTripLoad = totalTrips > 0 ? totalWeight / totalTrips : 0.0;
            double vehicleUtilization = (totalTrips > 0) ? (totalWeight / (totalTrips * MaxVehicleCapacity)) * 100.0 : 0.0;

            Console.WriteLine("Planning Summary");
            Console.WriteLine("----------------");
            Console.WriteLine($"Total deliveries: {totalDeliveries}");
            Console.WriteLine($"Delivered: {delivered}");
            Console.WriteLine($"Rejected: {rejected}");
            Console.WriteLine($"Total trips: {totalTrips}");
            Console.WriteLine($"Total weight: {totalWeight:F0} kg");
            Console.WriteLine($"Average trip load: {averageTripLoad:F2} kg");
            Console.WriteLine($"Vehicle utilization: {vehicleUtilization:F1}%");
            Console.WriteLine();
        }
    }
}