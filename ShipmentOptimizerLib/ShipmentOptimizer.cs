using System;
using System.Linq;
using System.Collections.Generic;
using System.Device.Location;

namespace ShipmentOptimizerLib
{
    public class ShipmentOptimizer
    {
        public const double MetersToMiles = 0.0006213699495;

        private List<TransferData> dataSet_ = new List<TransferData>();

        private static Dictionary<string, GeoCoordinate> locationSet_ = new Dictionary<string, GeoCoordinate>();

        public double MaxRadius { get; set; }

        public double MaxWeight { get; set; }

        public double MinWeight { get; set; }

        public double ExtraCostOut { get; set; }

        public int MaxPickups { get; set; }

        public int MaxDeliveries { get; set; }

        public ShipmentOptimizer()
        {
        }

        public void Remove(TransferData shipment)
        {
            dataSet_.Remove(shipment);
        }

        public bool IsContain(TransferData shipment)
        {
            return dataSet_.Contains(shipment);
        }


        public GeoCoordinate GetLocationCoord(string name)
        {
            if (locationSet_.ContainsKey(name))
            {
                return locationSet_[name];
            }

            return null;
        }

        public IEnumerable<IEnumerable<TransferData>> GetGropedShipment()
        {
            var result = new List<List<TransferData>>();

            foreach (var data in dataSet_)
            {
                bool flag = true;
                foreach (var bundle in result)
                {
                    if (bundle[0].fromName == data.fromName && bundle[0].toName == data.toName)
                    {
                        bundle.Add(data);
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    var dist = locationSet_[data.fromName].GetDistanceTo(locationSet_[data.toName]) * 1.34 * MetersToMiles;
                    var radius = dist * 0.15;

                    //Console.WriteLine($"{data.fromName,28} - {data.toName,28} : Road Dist = {(long)dist:D5} miles, Radius = {(long)radius:D4}");

                    var list = new List<TransferData>();
                    list.Add(data);
                    result.Add(list);
                }
            }

            foreach (var data in dataSet_)
            {
                foreach (var bundle in result)
                {
                    if (bundle[0].fromName != data.fromName || bundle[0].toName != data.toName)
                    {
                        //var radius = MaxRadius;
                        var radius = locationSet_[bundle[0].fromName].GetDistanceTo(locationSet_[bundle[0].toName]) * 1.34 * 0.15 * MetersToMiles;

                        if (locationSet_[data.fromName].GetDistanceTo(locationSet_[bundle[0].fromName]) * MetersToMiles <= radius &&
                            locationSet_[data.toName].GetDistanceTo(locationSet_[bundle[0].toName]) * MetersToMiles <= radius)
                        {
                            bundle.Add(data);
                        }
                    }
                }
            }

            return result;
        }

        public IEnumerable<string> GetNearestLocations(string Name)
        {
            var result = new List<string>();

            try
            {
                var base_loc = locationSet_[Name];

                foreach (var location in locationSet_)
                {
                    double dist = base_loc.GetDistanceTo(location.Value) * MetersToMiles;

                    if (dist <= MaxRadius && Name != location.Key)
                    {
                        result.Add(location.Key);
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        public double GetBundleRate(IList<TransferData> data)
        {
            //var pick = new List<string>();
            //var deliv = new List<string>();

            double rate = 0.0;

            foreach (var s in data)
            {
                rate += s.rate;
                //pick.Add(s.fromName);
                //deliv.Add(s.toName);
            }

            return rate;// - (pick.Distinct().Count() + deliv.Distinct().Count() - 2) * ExtraCostOut;
        }

        public double GetBundleFactor(IList<TransferData> data)
        {
            double factor = 0.0;

            foreach (var s in data)
            {
                factor += s.multiplier;
            }

            return factor;
        }
        private void putBestBundle(List<TransferData> result, IEnumerable<TransferData> best)
        {            
            if (best != null)
            {
                if (result.Count == 0)
                {
                    result.AddRange(best);
                }
                else
                {
                    var rbest = GetBundleRate(best.ToList());
                    var rres = GetBundleRate(result);

                    if (rbest > rres || (rbest == rres && GetBundleFactor(best.ToList()) < GetBundleFactor(result)))
                    {
                        result.Clear();
                        result.AddRange(best);
                    }
                }
            }
        }

        private bool isExceedMaxPickupsDeliveries(TransferData base_shipment, IList<TransferData> additional, int maxPickups, int maxDeliveries)
        {
            var added = new List<TransferData>();

            int pick = 0, deliv = 0;

            foreach (var shipment in additional)
            {          
                if (shipment.fromName != base_shipment.fromName)
                {
                    bool fl = true;

                    foreach (var sh in added)
                    {
                        if (shipment.fromName == sh.fromName) fl = false;
                    }

                    if (fl)
                    {
                        pick++;                        
                    }
                }
                if (shipment.toName != base_shipment.toName)
                {
                    bool fl = true;

                    foreach (var sh in added)
                    {
                        if (shipment.toName == sh.toName) fl = false;
                    }

                    if (fl)
                    {
                        deliv++;                        
                    }
                }

                if (pick > maxPickups || deliv > maxDeliveries) return true;
                else
                {
                    added.Add(shipment);
                } 
            }

            return false;
        }

        public IEnumerable<TransferData> SelectBestBundlebyGA(List<TransferData> shipmentList)
        {
            return GeneticAlgorithm.GetBestBundle(shipmentList, MinWeight, MaxWeight);
        }

        public IEnumerable<TransferData> SelectBestBundle(List<TransferData> shipmentList)
        {
            var base_from = shipmentList[0].fromName;
            var base_to = shipmentList[0].toName;

            var same_base = new List<TransferData>();
            var not_same = new List<TransferData>();
            
            foreach (var shipment in shipmentList)
            {
                if (shipment.fromName == base_from && shipment.toName == base_to)
                {
                    same_base.Add(shipment);
                }
                else
                {
                    not_same.Add(shipment);                  
                }
            }

            //Console.WriteLine("--------- base ------------");
            //foreach (var s in same_base)
            //{
            //    Console.WriteLine($"{s.fromName,28} - {s.toName,28}");
            //}
            //Console.WriteLine("--------- diff ------------");
            //foreach (var s in not_same)
            //{
            //    Console.WriteLine($"{s.fromName,28} - {s.toName,28}");
            //}

            foreach (var s in same_base)
            {
                if (same_base.IndexOf(s) != 0)
                    s.rate -= ExtraCostOut;
            }

            foreach (var s in not_same)
            {                
                s.rate -= ExtraCostOut;
            }

            if (!isExceedMaxPickupsDeliveries(same_base[0], not_same, MaxPickups - 1, MaxDeliveries - 1))
            {
                return selectBestBundle(shipmentList);
            }

            if (MaxPickups == 1 && MaxDeliveries == 1)
            {
                return selectBestBundle(same_base);
            }

            var added = new List<TransferData>();
            var resList = new List<TransferData>();
            selectBestFromCombo(same_base, not_same, MaxPickups - 1, MaxDeliveries - 1, added, resList);

            return resList;

            //var result = new List<KeyValuePair<double, int>>();

            //foreach (var res in resList)
            //{
            //    double r = GetBundleRate(res);
                
            //    result.Add(new KeyValuePair<double, int>(r, resList.IndexOf(res)));
            //}

            //result.Sort((s1, s2) => { return (int)(s2.Key - s1.Key); });

            ////return resList[result[0].Value];

            //int i = 0;
            //foreach (var res in result)
            //{
            //    Console.WriteLine("-------------------------------------");

            //    double f = GetBundleFactor(resList[res.Value]);
            //    foreach (var s in resList[res.Value])
            //    {
                    
            //        Console.WriteLine($"{s.fromName,28} - {s.toName,28}");
            //    }

            //    Console.WriteLine($"{++i:D4}: f = {f,5} r = {res.Key,7}$");
            //}

            //return null;
        }

        private void selectBestFromCombo(List<TransferData> same_base, List<TransferData> not_same, int from, int to, List<TransferData> added, List<TransferData> result)
        {      
            bool isAdded = false;
                  
            foreach (var s in not_same)
            {
                int ind = not_same.IndexOf(s);

                if (added.Count == 5)
                {
                    int y = 0;
                }

                var sub_not_same = new List<TransferData>(not_same.GetRange(ind, not_same.Count - ind));

                sub_not_same.AddRange(added);

                if (!isExceedMaxPickupsDeliveries(same_base[0], sub_not_same, MaxPickups - 1, MaxDeliveries - 1))
                {
                    var grouped = new List<TransferData>(same_base);

                    grouped.AddRange(sub_not_same);

                    //Console.WriteLine("-----------grouped by max pickup and delivery-----------");
                    //foreach (var sh in grouped)
                    //{
                    //    Console.WriteLine($"{sh.fromName,28} - {sh.toName,28}");
                    //}

                    //result.Add(selectBestBundle(grouped).ToList());

                    var best = selectBestBundle(grouped);
                    putBestBundle(result, best);

                    return;
                }
                else
                {   
                    var new_not_same = new List<TransferData>(not_same.GetRange(ind, not_same.Count - ind));

                    var new_added = new List<TransferData>(added);

                    new_not_same.Remove(s);

                    new_added.Add(s);

                    if (!isExceedMaxPickupsDeliveries(same_base[0], new_added, MaxPickups - 1, MaxDeliveries - 1))
                    {
                        isAdded = true;

                        if (new_not_same.Count > 0)
                        {
                            int fr = 0, tt = 0;

                            bool fl = true;

                            if (s.fromName != same_base[0].fromName)
                            {
                                foreach (var sh in added)
                                {
                                    if (s.fromName == sh.fromName) fl = false;
                                }

                                if (fl) fr = 1;
                            }

                            if (s.toName != same_base[0].toName)
                            {
                                fl = true;

                                foreach (var sh in added)
                                {
                                    if (s.toName == sh.toName) fl = false;
                                }

                                if (fl) tt = 1;
                            }

                            selectBestFromCombo(same_base, new_not_same, from - fr, to - tt, new_added, result);
                        }
                        else
                        {
                            var grouped = new List<TransferData>(same_base);

                            grouped.AddRange(new_added);

                            //Console.WriteLine("-----------grouped by max pickup and delivery-----------");
                            //foreach (var sh in grouped)
                            //{
                            //    Console.WriteLine($"{sh.fromName,28} - {sh.toName,28}");
                            //}

                            //result.Add(selectBestBundle(grouped).ToList());

                            var best = selectBestBundle(grouped);
                            putBestBundle(result, best);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            if (!isAdded)
            {
                var grouped = new List<TransferData>(same_base);

                grouped.AddRange(added);

                //Console.WriteLine("-----------grouped by max pickup and delivery-----------");
                //foreach (var sh in grouped)
                //{
                //    Console.WriteLine($"{sh.fromName,28} - {sh.toName,28}");
                //}

                //result.Add(selectBestBundle(grouped).ToList());

                var best = selectBestBundle(grouped);
                putBestBundle(result, best);
            }

        }
        
        private IEnumerable<TransferData> selectBestBundle(List<TransferData> shipmentList)
        {
            var result = new List<TransferData>();

            shipmentList.Sort((s1, s2) => { return (int)(s1.multiplier * 10.0 - s2.multiplier * 10.0); });

            int W = (int)(MaxWeight * 10.0);
            int K = shipmentList.Count;

            double[] A = new double[(K + 1) * (W + 1)];

            int k, w;
            for (k = 1; k <= K; ++k)
            {
                for (w = 1; w <= W; ++w)
                {
                    if (w >= (int)(shipmentList[k - 1].multiplier * 10.0))
                    {
                        A[k * (W + 1) + w] = Math.Max(A[(k - 1) * (W + 1) + w], A[(k - 1) * (W + 1) + w - (int)(shipmentList[k - 1].multiplier * 10.0)] + shipmentList[k - 1].rate);
                    }
                    else
                    {
                        A[k * (W + 1) + w] = A[(k - 1) * (W + 1) + w];
                    }
                }
            }


            double weight = 0, cost = 0;
            w = W;
            k = K;
            while (k > 0)
            {
                if (A[k * (W + 1) + w] > A[(k - 1) * (W + 1) + w])
                {
                    weight += shipmentList[k - 1].multiplier;
                    cost += shipmentList[k - 1].rate;

                    result.Add(shipmentList[k - 1]);
                    w = w - (int)(shipmentList[k - 1].multiplier * 10.0);
                }

                if (A[(k - 1) * (W + 1) + w] == 0) break;

                --k;
            }
            
            if (weight < MinWeight) return null;

            return result;
        }

        public void ClearInputData()
        {
            dataSet_.Clear();
            locationSet_.Clear();
        }

        public void LoadDataFromXLS(string path, string sheetName)
        {
            DataManager.LoadFromXLSFile(path, sheetName, dataSet_, locationSet_);
        }

        public void GetRandomData(int count)
        {
            DataManager.GetRandomData(count, dataSet_, locationSet_);
        }
    }
}
