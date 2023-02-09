using System;
using System.Collections.Generic;
using Encog.ML.Genetic.Genome;
using Encog.ML.EA.Train;
using Encog.ML.EA.Population;
using Encog.ML.EA.Species;
using Encog.Neural.Networks.Training;
using Encog.MathUtil.Randomize;

namespace ShipmentOptimizerLib
{
    class GeneticAlgorithm
    {
        private static List<TransferData> shipmentList;

        private static EncogRandom rnd = new EncogRandom();

        private static double minFactor;
        private static double maxFactor;

        public const int GENOME_SIZE = 30;
        public static int POPULATION_SIZE = 5000;
        public const int MAX_SAME_SOLUTION = 10;

        private static TrainEA genetic;

        private static IntegerArrayGenome RandomGenome()
        {
            IntegerArrayGenome result = new IntegerArrayGenome(GENOME_SIZE);
            int[] organism = result.Data;
            bool[] taken = new bool[GENOME_SIZE];

            //bool fl = false;
            //for (int i = 0; i < GENOME_SIZE; ++i)
            //{
            //    do
            //    {
            //        organism[i] = -(int)(rnd.NextDouble() * int.MaxValue);
            //        fl = false;
            //        for (int j = 0; j < i; ++j)
            //        {
            //            if (organism[j] == organism[i]) fl = true;
            //        }
            //    }
            //    while (fl);
            //}

            double factor = 0.0;
            
            while(factor < maxFactor / 2)
            {
                int ind;
                
                while (taken[(ind = (int)(rnd.NextDouble() * GENOME_SIZE))]) ;
                
                taken[ind] = true;                

                var trData = shipmentList[(int)(rnd.NextDouble() * (shipmentList.Count-1))];

                bool flag = false;

                for (int i = 0; i < GENOME_SIZE; ++i)
                {
                    if (organism[i] == (int)trData.id) flag = true;
                }

                if (!flag)
                {
                    organism[ind] = (int)(trData.id & 0xFFFFFFFF);

                    factor += trData.multiplier;
                }
            }
            
            return result;
        }


        private static IPopulation initPopulation()
        {
            IPopulation result = new BasicPopulation(POPULATION_SIZE, null);

            BasicSpecies defaultSpecies = new BasicSpecies();
            defaultSpecies.Population = result;
            for (int i = 0; i < POPULATION_SIZE; i++)
            {
                IntegerArrayGenome genome = RandomGenome();
                defaultSpecies.Members.Add(genome);
            }
            result.GenomeFactory = new IntegerArrayGenomeFactory(GENOME_SIZE);
            result.Species.Add(defaultSpecies);

            return result;
        }

        public static IEnumerable<TransferData> GetBestBundle(List<TransferData> shList, double minF, double maxF)
        {
            POPULATION_SIZE = Math.Max(shList.Count/2, 1500);

            var result = new List<TransferData>();

            shipmentList = shList;

            minFactor = minF;

            maxFactor = maxF;

            IPopulation pop = initPopulation();
            //var pop = new Population.Population(POPULATION_SIZE, 5, shList.Count - 1);

            ICalculateScore score = new GAScore(shList);
            //ICalculateScore score = new Evolution.GenomeScore(shList);

            genetic = new TrainEA(pop, score);

            //genetic.AddOperation(1.0, new Evolution.Splice(shList));
            //genetic.AddOperation(1.0, new Evolution.Mutate(shList, 0.5));
            //genetic.AddOperation(1.0, new Evolution.Reduce(shList));

            
            genetic.AddOperation(1.0, new SpliceAndShuffle(shList));
            genetic.AddOperation(0.1, new MutateGenome(shList, 0.3));

            int sameSolutionCount = 0;
            int iteration = 1;
            double lastSolution = Double.MaxValue;

            while (sameSolutionCount < MAX_SAME_SOLUTION)
            {   
                genetic.Iteration();

                double thisSolution = genetic.Error;

                Console.WriteLine($"Iteration: {iteration++}, Best Rate = {thisSolution}");

                if (Math.Abs(lastSolution - thisSolution) < 1.0)
                {
                    sameSolutionCount++;
                }
                else
                {
                    sameSolutionCount = 0;
                }

                lastSolution = thisSolution;
            }

            var bestgenome = (IntegerArrayGenome)genetic.BestGenome;
            //var bestgenome = (Genome.IntegerListGenome)genetic.BestGenome;

            foreach (var gen in bestgenome.Data)
            {
                if (gen > 0)
                    result.Add(shipmentList[gen]);
            }

            return result;
        }
    }
}
