using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.IO;
using OfficeOpenXml;

namespace ShipmentOptimizerLib
{
    public class DataManager
    {
        public static void LoadFromXLSFile(string path, string sheetName, List<TransferData> dataSet, Dictionary<string, GeoCoordinate> locationSet)
        {
            var excel = new ExcelPackage(new FileInfo(path));

            var sheet = excel.Workbook.Worksheets[sheetName];

            int rownum = 1;
            while (sheet.Cells[$"A{++rownum}"].Value != null)
            {
                double lat, lon;
                var dataItem = new TransferData();
                int length = 0;

                try
                {
                    length = sheet.Cells[$"A{rownum}"].Value.ToString().Length;
                    dataItem.id = (ulong)long.Parse(sheet.Cells[$"A{rownum}"].Value.ToString());
                    dataItem.rate = double.Parse(sheet.Cells[$"B{rownum}"].Value.ToString(), CultureInfo.InvariantCulture);

                    lat = double.Parse(sheet.Cells[$"C{rownum}"].Value.ToString());
                    lon = double.Parse(sheet.Cells[$"D{rownum}"].Value.ToString());
                    dataItem.fromName = sheet.Cells[$"E{rownum}"].Value.ToString();

                    if (locationSet.ContainsKey(dataItem.fromName))
                    {
                        if (Math.Abs(locationSet[dataItem.fromName].Latitude - lat) > 0.000005 ||
                            Math.Abs(locationSet[dataItem.fromName].Longitude - lon) > 0.000005)
                        {
                            int ind = 0;
                            while (locationSet.ContainsKey($"{dataItem.fromName}_{++ind}")) ;
                            dataItem.fromName = $"{dataItem.fromName}_{ind}";
                            locationSet.Add(dataItem.fromName, new GeoCoordinate(lat, lon));
                        }
                    }
                    else
                    {
                        locationSet.Add(dataItem.fromName, new GeoCoordinate(lat, lon));
                    }

                    lat = double.Parse(sheet.Cells[$"F{rownum}"].Value.ToString());
                    lon = double.Parse(sheet.Cells[$"G{rownum}"].Value.ToString());
                    dataItem.toName = sheet.Cells[$"H{rownum}"].Value.ToString();

                    if (locationSet.ContainsKey(dataItem.toName))
                    {
                        if (Math.Abs(locationSet[dataItem.toName].Latitude - lat) > 0.000005 ||
                            Math.Abs(locationSet[dataItem.toName].Longitude - lon) > 0.000005)
                        {
                            int ind = 0;
                            while (locationSet.ContainsKey($"{dataItem.toName}_{++ind}")) ;
                            dataItem.toName = $"{dataItem.toName}_{ind}";
                            locationSet.Add(dataItem.toName, new GeoCoordinate(lat, lon));
                        }
                    }
                    else
                    {
                        locationSet.Add(dataItem.toName, new GeoCoordinate(lat, lon));
                    }

                    dataItem.multiplier = double.Parse(sheet.Cells[$"I{rownum}"].Value.ToString(), CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + $" - {length}\n");
                    Console.ReadLine();
                    continue;
                }

                dataSet.Add(dataItem);
            }
        }

        public static void GetRandomData(int count, List<TransferData> dataSet, Dictionary<string, GeoCoordinate> locationSet)
        {
            var rng = new Random((int)DateTime.Now.Ticks);

            locationSet.Add("1", new GeoCoordinate(0,0));
            locationSet.Add("2", new GeoCoordinate(0,0));

            for (int i = 0; i < count; ++i)
            {
                var item = new TransferData();

                item.fromName = "1";

                item.toName = "2";

                item.id = (ulong)i + 1;

                item.multiplier = (int)(rng.NextDouble() * 10.0 + 5.0) / 10.0;

                item.rate = rng.NextDouble() * 500.0 + 100.0;

                dataSet.Add(item);
            }
        }
    }
}
