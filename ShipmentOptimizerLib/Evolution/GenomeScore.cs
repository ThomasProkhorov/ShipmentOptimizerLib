using System.Collections.Generic;
using Encog.Neural.Networks.Training;
using Encog.ML;
using ShipmentOptimizerLib.Genome;

namespace ShipmentOptimizerLib.Evolution
{    
    public class GenomeScore : ICalculateScore
    {
        private readonly List<TransferData> shipments;

        public GenomeScore(List<TransferData> shipments)
        {
            this.shipments = shipments;
        }

        #region ICalculateGenomeScore Members

        public double CalculateScore(IMLMethod phenotype)
        {
            double rate = 0.0, factor = 0.0;
            IntegerListGenome genome = (IntegerListGenome)phenotype;
            
            foreach (var gene in genome.Data)
            {
                rate += shipments[gene].rate;
                factor += shipments[gene].multiplier;
            }
            
            return (factor <= 9.0) ? rate : 0.0;
        }

        public bool ShouldMinimize
        {
            get { return false; }
        }

        #endregion

        /// <inheritdoc/>
        public bool RequireSingleThreaded
        {
            get { return false; }
        }
    }
}
