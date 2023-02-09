using Encog.ML.EA.Genome;

namespace ShipmentOptimizerLib.Genome
{
    public class IntegerListGenomeFactory : IGenomeFactory
    {
        /// <summary>
        /// The size to create.
        /// </summary>
        private int size;

        /// <summary>
        /// Construct the genome factory.
        /// </summary>
        /// <param name="theSize">The size to create genomes of.</param>
        public IntegerListGenomeFactory(int theSize)
        {
            this.size = theSize;
        }

        /// <inheritdoc/>
        public IGenome Factor()
        {
            return new IntegerListGenome(this.size);
        }

        /// <inheritdoc/>
        public IGenome Factor(IGenome other)
        {
            // TODO Auto-generated method stub
            return new IntegerListGenome((IntegerListGenome)other);
        }
    }
}
