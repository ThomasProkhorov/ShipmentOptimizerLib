using System;
using System.Linq;
using System.Device.Location;
using ShipmentOptimizerLib;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestOptimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var optimizer = new ShipmentOptimizer();

            while (true)
            {
                optimizer.ClearInputData();

                optimizer.LoadDataFromXLS(Environment.CurrentDirectory + "\\DataSample.xlsx", "Sample Data");

                //int count = 0;
                //Console.Write("Input shipment count: ");
                //count = Convert.ToInt32(Console.ReadLine());

                //optimizer.GetRandomData(count);
            
                optimizer.MaxRadius = 500.0;

                //Console.Write("Input max size factor: ");
                //optimizer.MaxWeight = Convert.ToDouble(Console.ReadLine());

                //Console.Write("Input min size factor: ");
                //optimizer.MinWeight = Convert.ToDouble(Console.ReadLine());

                //Console.Write("Input extra cost: ");
                //optimizer.ExtraCostOut = Convert.ToDouble(Console.ReadLine());

                //Console.Write("Input max pickups: ");
                //optimizer.MaxPickups = Convert.ToInt32(Console.ReadLine());

                //Console.Write("Input max deliveries: ");
                //optimizer.MaxDeliveries = Convert.ToInt32(Console.ReadLine());

                optimizer.MaxWeight = 15.0;

                
                optimizer.MinWeight = 5.0;
                                
                optimizer.ExtraCostOut = 0.0;

                optimizer.MaxPickups = 5;

                optimizer.MaxDeliveries = 5;

                var elapsed = Stopwatch.StartNew();                
                               
                PrintDataSet(optimizer);

                elapsed.Stop();
                Console.WriteLine($"Time elapsed: {elapsed.Elapsed.Hours:D2}:{elapsed.Elapsed.Minutes:D2}:{elapsed.Elapsed.Seconds:D2}.{elapsed.Elapsed.Milliseconds:D3}");

                //PrintNearestLocation("WATERFORD, MI", optimizer);

                Console.WriteLine("Press enter key...");
                Console.ReadLine();
            }
        }

        public static void PrintDataSet(ShipmentOptimizer optimizer)
        {
            //foreach (var data in dataSet_)
            //{
            //    Console.WriteLine($"{data.id,20} {data.fromName,28} - {data.toName,28}\t\t: {(long)(locationSet_[data.fromName].GetDistanceTo(locationSet_[data.toName]) * MetersToMiles),5} miles");
            //}

            //foreach (var location in locationSet_)
            //{
            //    Console.WriteLine($"{location.Key,28} : {location.Value.Latitude,12} , {location.Value.Longitude,12}");
            //}

            //foreach (var location in locationSet_)
            //{
            //    var nearLoc = getNearLocation(location.Key).ToList();
            //    if (nearLoc.Count > 0)
            //    {
            //        Console.WriteLine($"{location.Key,28} : {location.Value.Latitude,12} , {location.Value.Longitude,12}");
            //        foreach (var name in nearLoc)
            //        {
            //            var loc = locationSet_[name];
            //            Console.WriteLine($" => {name,28} : {loc.Latitude,12} , {loc.Longitude,12} -> {(long)(location.Value.GetDistanceTo(loc) * MetersToMiles),2} miles");
            //        }
            //    }
            //}

            //var grouped = getGropedShipment();

            //int ind = 0;
            //foreach (var group in grouped)
            //{
            //    Console.WriteLine($"Group #{++ind:D2}");
            //    foreach (var item in group)
            //    {
            //        Console.WriteLine($"\t{item.fromName,28} - {item.toName,28}");
            //    }
            //}

            var total_bundles = new List<List<TransferData>>();

            do
            {
                var grouped = optimizer.GetGropedShipment();

                //var group = grouped.ToList()[359];

                var bundles = new List<List<TransferData>>();

                foreach (var group in grouped)
                {
                    //foreach (var item in group)
                    //{
                    //    Console.WriteLine($"{item.id,20} {item.fromName} - {item.toName} Cost = {item.rate:N2} Weight = {item.multiplier:N2}");
                    //}

                    if (grouped.ToList().IndexOf(group) == 322)
                    {
                        int y = 0;
                    }

                    var elapsed = Stopwatch.StartNew();

                    var best = optimizer.SelectBestBundle(group.ToList());

                    elapsed.Stop();
                    Console.WriteLine($"Time elapsed for Dinamic Programming: {elapsed.Elapsed.Hours:D2}:{elapsed.Elapsed.Minutes:D2}:{elapsed.Elapsed.Seconds:D2}.{elapsed.Elapsed.Milliseconds:D3}");

                    if (best != null && best.Count() > 0)
                    {
                        //bundles.Add(best.ToList());
                        total_bundles.Add(best.ToList());
                    }

                    elapsed = Stopwatch.StartNew();

                    best = optimizer.SelectBestBundlebyGA(group.ToList());

                    elapsed.Stop();
                    Console.WriteLine($"Time elapsed for Genetic Algorithm: {elapsed.Elapsed.Hours:D2}:{elapsed.Elapsed.Minutes:D2}:{elapsed.Elapsed.Seconds:D2}.{elapsed.Elapsed.Milliseconds:D3}");

                    if (best != null && best.Count() > 0)
                    {
                        //bundles.Add(best.ToList());
                        total_bundles.Add(best.ToList());
                    }
                }

                break;

                if (bundles.Count == 0) break;

                bundles.Sort((s1, s2) => { return (int)(optimizer.GetBundleRate(s2) - optimizer.GetBundleRate(s1)); });
                
                total_bundles.Add(bundles[0]);

                foreach (var ship in bundles[0])
                {
                    optimizer.Remove(ship);
                }

                bundles.Remove(bundles[0]);

                foreach (var bundle in bundles)
                {
                    bool good = true;

                    foreach (var ship in bundle)
                    {
                        if (!optimizer.IsContain(ship))
                        {
                            good = false;
                        }
                    }

                    if (good)
                    {
                        foreach (var ship in bundle)
                        {
                            optimizer.Remove(ship);
                        }

                        total_bundles.Add(bundle);
                    }
                }
            }
            while (true);

            var shipids = new List<ulong>();

            int ind = 0;
            foreach (var bundle in total_bundles)
            {
                foreach(var item in bundle)
                {
                    if (shipids.Contains(item.id))
                    {
                        int y = 0;
                    }
                    shipids.Add(item.id);
                }

                Console.WriteLine($"========================= BUNDLE {++ind,3} =======================");
                double cost = optimizer.GetBundleRate(bundle);
                double weight = optimizer.GetBundleFactor(bundle);

                foreach (var item in bundle)
                {
                    Console.WriteLine($"{item.id,20} {item.fromName} - {item.toName} Cost = {item.rate:N2} Weight = {item.multiplier:N2}");
                }
                Console.WriteLine($"-------------------------------------------------");
                Console.WriteLine($"\t\tTotal Cost: {cost}, Total Weight = {weight}");
                Console.WriteLine();
            }
        }

        public static void PrintNearestLocation(string locName, ShipmentOptimizer optimizer)
        {
            try
            {
                var base_coord = optimizer.GetLocationCoord(locName);

                foreach (var name in optimizer.GetNearestLocations(locName))
                {
                    var coord = optimizer.GetLocationCoord(name);

                    if (coord != null)
                    {
                        Console.WriteLine($" => {name,28} : {coord.Latitude,12} , {coord.Longitude,12} -> {(long)(base_coord.GetDistanceTo(coord) * ShipmentOptimizer.MetersToMiles),2} miles");
                    }
                }
            }
            catch
            {
                Console.WriteLine($"Not found location: {locName}");
            }
        }
    }
}
