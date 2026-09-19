namespace DeliveryRoutePlanner.Entities
{
    public class Trip
    {
        public string Name { get; set; }
        public double Capacity { get; set; } = 0;
        public List<Delivery> Deliveries { get; set; } = new List<Delivery>();

        public override string? ToString()
        {
            return $"{Name}, Capacity: {Capacity}, Deliveries: {Deliveries.Count}\n";
        }
    }
}
