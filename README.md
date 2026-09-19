# DeliveryRoutePlanner

DeliveryRoutePlanner is a simple C# console application that organizes delivery requests into vehicle trips. It is written for .NET 9 and aims to respect vehicle capacity and area constraints while keeping the implementation straightforward.

## Requirements

- .NET 9 SDK
- Visual Studio 2022 or any editor that supports .NET 9

## Get the code

Clone the repository and change into the project directory:

```bash
git clone https://github.com/AnasAlamir/DeliveryRoutePlanner.git
cd DeliveryRoutePlanner
```

## Build and run

From the repository root you can build and run the project:

```bash
dotnet build
dotnet run --project DeliveryRoutePlanner
```

Or change into the `DeliveryRoutePlanner` project folder and run:

```bash
cd DeliveryRoutePlanner
dotnet run
```

Alternatively, open the solution in Visual Studio 2022 and run the `DeliveryRoutePlanner` project.

## Input format

Input is JSON looks like:

```json
[
  {
    "id": 1,
    "area": "Nasr City",
    "priority": 2,
    "packageWeight": 4.5
  },
  {
    "id": 2,
    "area": "Maadi",
    "priority": 1,
    "packageWeight": 2.0
  }
]
```

The sample input files are located at `DeliveryRoutePlanner/input_samples`. The common sample filename used by the project is `deliveries.json` (and there are several example files such as `deliveries-invalid.json`).

How to use sample files: modify Program.cs and change the file path in the `ProcessDeliveryFile` method call to point to the desired sample file. Then run the project. The console output shows trips and the planning summary.

```
static void Main(string[] args)
{
    ProcessDeliveryFile("input_samples/deliveries.json"); // Change this path to use a different sample file
}
```
## Solution Approach

1. load the deliveries from the JSON file.
2. Invalid deliveries are removed (e.g. non-positive weight, missing area, weight > 10 kg).
3. Deliveries are sorted by priority, area and weight.
4. Deliveries are grouped by priority into a SortedDictionary.
5. Trips are created with a maximum capacity of 10 kg.
6. The algorithm processes higher-priority deliveries first and, for each delivery, tries to place it into an existing trip that has remaining capacity and (when possible) already contains the same area. If none fit, a new trip is started.

This is a greedy algorithm that aims for a good, predictable grouping while keeping the implementation simple.

## Most difficult part

Balancing the requirements

1. finding how to organize the deliveries.
1. process higher-priority deliveries first.
2. keep every trip within the 10 kg capacity and try to fill trips as much as possible.
3. group deliveries by area when it is reasonable.

These goals can conflict and required some trade-offs.

## Situations where your algorithm may not produce the best possible grouping

The greedy approach does not guarantee a globally optimal grouping. For example, it does not reorder deliveries to pack trips more tightly (it does not always choose the largest fitting delivery first), and it starts new trips based on the input order. This can leave unused space that a different ordering might fill better.

## Scalability for 1,000,000 deliveries

- Keeping all deliveries in memory and sorting them can be slow and memory-intensive.
- Scanning existing trips to find space for each delivery (and using temporary collections during grouping) could become a performance bottleneck.

## Improvements if given another day

- Try to optimize the packing algorithm.
- Refactor and improve code quality.
- Add unit tests for edge cases and behaviors.

## Extension: Planning Summary

The program prints a Planning Summary that includes:

- Total deliveries
- Delivered deliveries
- Rejected deliveries
- Number of trips
- Total weight
- Average trip load
- Vehicle utilization

This feature provides a quick overview of how effective the generated plan is without inspecting every trip.