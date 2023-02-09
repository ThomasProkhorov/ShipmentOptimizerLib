using System.Collections.Generic;
using System.Linq;
using Encog.ML.EA.Opp;
using Encog.ML.EA.Train;
using Encog.ML.EA.Genome;
using Encog.MathUtil.Randomize;
using ShipmentOptimizerLib.Genome;

namespace ShipmentOptimizerLib.Evolution
{
    class Mutate : IEvolutionaryOperator
    {
        private List<TransferData> shipmentList;

        private double portion;

        private IEvolutionaryAlgorithm owner;

        public Mutate(List<TransferData> shList, double genePortion)
        {
            shipmentList = shList;
            portion = genePortion;
        }

        /// <inheritdoc/>
        public void PerformOperation(EncogRandom rnd, IGenome[] parents, int parentIndex,
                IGenome[] offspring, int offspringIndex)
        {
            IntegerListGenome parent = (IntegerListGenome)parents[parentIndex];
            offspring[offspringIndex] = this.owner.Population.GenomeFactory.Factor();
            IntegerListGenome child = (IntegerListGenome)offspring[offspringIndex];

            List<int> tmp = new List<int>(parent.Data);
            
            for (int i = 0; i < tmp.Count * portion; ++i)
            {
                tmp[rnd.Next(parent.Size - 1)] = (int)shipmentList[rnd.Next(shipmentList.Count - 1)].id;
            }

            child.Data.AddRange(tmp.Distinct());
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
