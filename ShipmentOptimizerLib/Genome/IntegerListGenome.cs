using Encog.ML.EA.Genome;
using Encog.ML.Genetic.Genome;
using System.Collections.Generic;

namespace ShipmentOptimizerLib.Genome
{
    public class IntegerListGenome : BasicGenome, IArrayGenome
    {
        /// <summary>
        /// The data.
        /// </summary>
        private List<int> data;

        /// <summary>
        /// Construct a genome of a specific size.
        /// </summary>
        /// <param name="size">The size.</param>
        public IntegerListGenome(int size)
        {
            this.data = new List<int>(size);
        }

        /// <summary>
        /// Construct a genome based on another genome.
        /// </summary>
        /// <param name="other">The other genome.</param>
        public IntegerListGenome(IntegerListGenome other)
        {
            this.data = new List<int>(other.Data);
        }

        /// <inheritdoc/>
        public override int Size
        {
            get
            {
                return this.data.Count;
            }
        }

        /// <inheritdoc/>
        public void Copy(IArrayGenome source, int sourceIndex, int targetIndex)
        {
            IntegerListGenome sourceInt = (IntegerListGenome)source;
            this.data[targetIndex] = sourceInt.data[sourceIndex];
        }

        /// <summary>
        /// The data.
        /// </summary>
        public List<int> Data
        {
            get
            {
                return this.data;
            }
        }

        /// <inheritdoc/>
        public override void Copy(IGenome source)
        {
            IntegerListGenome sourceList = (IntegerListGenome)source;
            this.data = new List<int>(sourceList.Data);            
            Score = source.Score;
            AdjustedScore = source.AdjustedScore;
        }

        /// <inheritdoc/>
        public void Swap(int iswap1, int iswap2)
        {
            int temp = this.data[iswap1];
            this.data[iswap1] = this.data[iswap2];
            this.data[iswap2] = temp;
        }
    }
}

