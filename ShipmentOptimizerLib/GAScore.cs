using Encog.ML.Genetic.Genome;
using Encog.Neural.Networks.Training;
using Encog.ML;
using System.Collections.Generic;

namespace ShipmentOptimizerLib
{
    public class GAScore : ICalculateScore
    {
        private readonly List<TransferData> shipments;

        public GAScore(List<TransferData> shipments)
        {
            this.shipments = shipments;
        }

        #region ICalculateGenomeScore Members

        public double CalculateScore(IMLMethod phenotype)
        {
            double rate = 0.0, factor = 0.0;
            IntegerArrayGenome genome = (IntegerArrayGenome)phenotype;
            int[] path = genome.Data;

            for (int i = 0; i < genome.Data.GetLength(0) - 1; i++)
            {
                if (path[i] > 0)
                {
                    rate += shipments[(int)path[i]].rate;
                    factor += shipments[(int)path[i]].multiplier;
                }
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
