using System.Collections.Generic;
using System.Linq;
using Encog.ML.EA.Opp;
using Encog.ML.EA.Train;
using Encog.ML.EA.Genome;
using Encog.ML.Genetic.Genome;
using Encog.MathUtil.Randomize;

namespace ShipmentOptimizerLib
{
    public class SpliceAndShuffle : IEvolutionaryOperator
    {
        private List<TransferData> shipmentList;

        private IEvolutionaryAlgorithm owner;

        public SpliceAndShuffle(List<TransferData> shList)
        {
            shipmentList = shList;
        }

        private double getGenomeFactor(IntegerArrayGenome genome)
        {
            double factor = 0.0;

            for (int i = 0; i < genome.Size; i++)
            {
                if (genome.Data[i] > 0)
                {
                    factor += shipmentList[genome.Data[i]].multiplier;
                }
            }

            return factor;
        }

        private void removeLowestGen(EncogRandom rnd, IntegerArrayGenome genome)
        {
            var indices = new SortedSet<int>();
                  
            int ind = 0;
            while (genome.Data[ind++] == 0) ;

            double minRate = shipmentList[genome.Data[--ind]].rate;

            indices.Add(ind);

            for (int i = ind + 1; i < genome.Size; i++)
            {
                if (genome.Data[i] > 0)
                {
                    indices.Add(i);

                    if (shipmentList[genome.Data[i]].rate < shipmentList[genome.Data[ind]].rate)
                    {
                        minRate = shipmentList[genome.Data[i]].rate;
                        ind = i;
                    }
                }
            }

            //genome.Data[ind] = 0;
            genome.Data[indices.ToArray()[(int)(rnd.NextDouble()*(indices.Count-1))]] = 0;
        }

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

        /// <inheritdoc/>
        public void PerformOperation(EncogRandom rnd, IGenome[] parents, int parentIndex,
                IGenome[] offspring, int offspringIndex)
        {
            IntegerArrayGenome mother = (IntegerArrayGenome)parents[parentIndex];
            IntegerArrayGenome father = (IntegerArrayGenome)parents[parentIndex + 1];
            IntegerArrayGenome offspring1 = (IntegerArrayGenome)this.owner.Population.GenomeFactory.Factor();
            IntegerArrayGenome offspring2 = (IntegerArrayGenome)this.owner.Population.GenomeFactory.Factor();

            offspring[offspringIndex] = (IArrayGenome)offspring1;
            offspring[offspringIndex + 1] = (IArrayGenome)offspring2;

            int geneLength = mother.Size;

            for (int i = 0; i < geneLength; i++)
            {
                offspring1.Data[i] = mother.Data[i];
                offspring2.Data[i] = father.Data[i];
            }

            for (int i = 0; i < geneLength; i++)
            {
                if (mother.Data[i] > 0)
                {
                    offspring2.Data[i] = mother.Data[i];
                }

                if (father.Data[i] > 0)
                {
                    offspring1.Data[i] = father.Data[i];
                }
            }

            distinctGenome(offspring1);
            distinctGenome(offspring2);

            while (getGenomeFactor(offspring1) > 9.0)
                removeLowestGen(rnd, offspring1);

            while (getGenomeFactor(offspring2) > 9.0)
                removeLowestGen(rnd, offspring2);

            for (int i = 0; i < geneLength; i++)
            {
                int ind1 = (int)(rnd.NextDouble() * (geneLength - 1));
                int ind2 = (int)(rnd.NextDouble() * (geneLength - 1));

                int t = offspring1.Data[ind2];
                offspring1.Data[ind2] = offspring1.Data[ind1];
                offspring1.Data[ind1] = t;
            }

            for (int i = 0; i < geneLength; i++)
            {
                int ind1 = (int)(rnd.NextDouble() * (geneLength - 1));
                int ind2 = (int)(rnd.NextDouble() * (geneLength - 1));

                int t = offspring2.Data[ind2];
                offspring2.Data[ind2] = offspring2.Data[ind1];
                offspring2.Data[ind1] = t;
            }
        }

        /// <inheritdoc/>
        public int OffspringProduced
        {
            get
            {
                return 2;
            }
        }

        /// <inheritdoc/>
        public int ParentsNeeded
        {
            get
            {
                return 2;
            }
        }

        /// <inheritdoc/>
        public void Init(IEvolutionaryAlgorithm theOwner)
        {
            this.owner = theOwner;

        }
    }
}
