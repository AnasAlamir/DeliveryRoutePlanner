namespace DeliveryRoutePlanner
{
    // internal class Program
    public class Program
    {
        // static void Main(string[] args)
        public static void Main(string[] args)
        {
            ProcessDeliveryFile("input_samples/deliveries.json");

            ProcessDeliveryFile("input_samples/deliveries-area-edge-case.json");

            ProcessDeliveryFile("input_samples/deliveries-area-same-priority-edge-case.json");

            ProcessDeliveryFile("input_samples/deliveries-invalid.json");

            Console.Write("\nDone");
        }

        private static void ProcessDeliveryFile(string fileName)
        {
            Console.WriteLine("---------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"======Planning deliveries from {fileName}======");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("---------------------------------------------------------------------------------\n");

            var planner = new Planner();
            planner.PlanDeliveries(fileName);
        }
    }
}