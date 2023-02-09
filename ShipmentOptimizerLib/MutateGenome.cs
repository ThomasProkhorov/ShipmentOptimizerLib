using System.Collections.Generic;
using System.Linq;
using Encog.ML.EA.Opp;
using Encog.ML.EA.Train;
using Encog.ML.EA.Genome;
using Encog.ML.Genetic.Genome;
using Encog.MathUtil.Randomize;

namespace ShipmentOptimizerLib
{
    class MutateGenome : IEvolutionaryOperator
    {
        private List<TransferData> shipmentList;

        private double portion;

        private IEvolutionaryAlgorithm owner;

        private void distinctGenome(IntegerArrayGenome genome)
        {
            var repeats = new SortedSet<int>();

            for (int i = 0; i < genome.Size; i++)
            {
                if (genome.Data[i] > 0)
                {
                    if (repeats.Contains(genome.Data[i]))
                    {
                        genome.Data[i] = 0;
                    }
                    else
                    {
                        repeats.Add(genome.Data[i]);
                    }
                }
            }
        }


        public MutateGenome(List<TransferData> shList, double genePortion)
        {
            shipmentList = shList;
            portion = genePortion;
        }
                
        /// <inheritdoc/>
        public void PerformOperation(EncogRandom rnd, IGenome[] parents, int parentIndex,
                IGenome[] offspring, int offspringIndex)
        {
            IntegerArrayGenome parent = (IntegerArrayGenome)parents[parentIndex];
            offspring[offspringIndex] = this.owner.Population.GenomeFactory.Factor();
            IntegerArrayGenome child = (IntegerArrayGenome)offspring[offspringIndex];

            child.Copy(parent);

            for (int i = 0; i < parent.Size * portion; ++i)
            {
                child.Data[rnd.Next(parent.Size - 1)] = (int)shipmentList[rnd.Next(shipmentList.Count - 1)].id;
            }

            distinctGenome(child);
        }

        /// <inheritdoc/>
        public int OffspringProduced
        {
            get
            {
                return 1;
            }
        }

        /// <inheritdoc/>
        public int ParentsNeeded
        {
            get
            {
                return 1;
            }
        }

        /// <inheritdoc/>
        public void Init(IEvolutionaryAlgorithm theOwner)
        {
            this.owner = theOwner;

        }
    }
}
