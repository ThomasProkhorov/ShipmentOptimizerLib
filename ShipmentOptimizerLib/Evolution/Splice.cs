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
    public class Splice : IEvolutionaryOperator
    {
        private List<TransferData> shipmentList;

        private IEvolutionaryAlgorithm owner;

        public Splice(List<TransferData> shList)
        {
            shipmentList = shList;
        }

        private double GetGenomeFactor(IntegerListGenome genome)
        {
            double factor = 0.0;

            foreach (var gene in genome.Data)
            {                
                factor += shipmentList[gene].multiplier;                
            }

            return factor;
        }
               
        
        /// <inheritdoc/>
        public void PerformOperation(EncogRandom rnd, IGenome[] parents, int parentIndex,
                IGenome[] offspring, int offspringIndex)
        {
            IntegerListGenome mother = (IntegerListGenome)parents[parentIndex];
            IntegerListGenome father = (IntegerListGenome)parents[parentIndex + 1];
            IntegerListGenome offspring1 = (IntegerListGenome)this.owner.Population.GenomeFactory.Factor();
            IntegerListGenome offspring2 = (IntegerListGenome)this.owner.Population.GenomeFactory.Factor();

            offspring[offspringIndex] = (IArrayGenome)offspring1;
            offspring[offspringIndex + 1] = (IArrayGenome)offspring2;

            var child1 = new List<int>();
            var child2 = new List<int>();

            mother.Data.Sort((c1, c2) => { return (int)((shipmentList[c1].rate / shipmentList[c1].multiplier - shipmentList[c2].rate / shipmentList[c2].multiplier) * 100000.0); });
            father.Data.Sort((c1, c2) => { return (int)((shipmentList[c1].rate / shipmentList[c1].multiplier - shipmentList[c2].rate / shipmentList[c2].multiplier) * 100000.0); });

            child1.AddRange(mother.Data.GetRange(0, (int)(mother.Data.Count * 0.9)));
            child1.AddRange(father.Data.GetRange(0, (int)(father.Data.Count * 0.7)));

            child2.AddRange(mother.Data.GetRange(0, (int)((mother.Data.Count-1) * 0.7)));
            child2.AddRange(father.Data.GetRange(0, (int)((father.Data.Count-1) * 0.9)));
            
            offspring1.Data.AddRange(child1.Distinct());
            offspring2.Data.AddRange(child2.Distinct());
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
