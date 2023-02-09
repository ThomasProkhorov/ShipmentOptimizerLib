using System.Collections.Generic;
using System.Linq;
using Encog.ML.EA.Opp;
using Encog.ML.EA.Train;
using Encog.ML.EA.Genome;
using Encog.ML.Genetic.Genome;
using Encog.MathUtil.Randomize;
using ShipmentOptimizerLib.Genome;

namespace ShipmentOptimizerLib.Evolution
{
    public class Reduce : IEvolutionaryOperator
    {
        private List<TransferData> shipmentList;

        private IEvolutionaryAlgorithm owner;

        public Reduce(List<TransferData> shList)
        {
            shipmentList = shList;
        }

        private double GetGenomeFactor(List<int> data)
        {
            double factor = 0.0;

            foreach (var gene in data)
            {
                factor += shipmentList[gene].multiplier;
            }

            return factor;
        }


        /// <inheritdoc/>
        public void PerformOperation(EncogRandom rnd, IGenome[] parents, int parentIndex,
                IGenome[] offspring, int offspringIndex)
        {
            IntegerListGenome parent = (IntegerListGenome)parents[parentIndex];
            offspring[offspringIndex] = this.owner.Population.GenomeFactory.Factor();
            IntegerListGenome child = (IntegerListGenome)offspring[offspringIndex];
            
            var childData = new List<int>();

            childData.AddRange(parent.Data);

            if (GetGenomeFactor(childData) > 9.0)
            {
                childData.Sort((c1, c2) => { return (int)((shipmentList[c1].rate / shipmentList[c1].multiplier - shipmentList[c2].rate / shipmentList[c2].multiplier)*100000.0); });

                while (GetGenomeFactor(childData) > 9.0)
                {
                    childData.RemoveAt(0);
                }
            }

            child.Data.AddRange(childData.Distinct());
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
