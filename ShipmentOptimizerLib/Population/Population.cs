using Encog.ML.EA.Population;
using Encog.ML.EA.Species;
using Encog.MathUtil.Randomize;
using ShipmentOptimizerLib.Genome;

namespace ShipmentOptimizerLib.Population
{
    public class Population : BasicPopulation
    {
        public Population(int size, int initCount, int maxValue) : base(size, null)
        {
            BasicSpecies defaultSpecies = new BasicSpecies();
            defaultSpecies.Population = this;
            for (int i = 0; i < size; i++)
            {
                IntegerListGenome genome = RandomGenome(initCount, maxValue);
                defaultSpecies.Members.Add(genome);
            }
            GenomeFactory = new IntegerListGenomeFactory(0);
            Species.Add(defaultSpecies);            
        }

        private IntegerListGenome RandomGenome(int initCount, int maxValue)
        {
            IntegerListGenome result = new IntegerListGenome(0);
            
            for (int i = 0; i < initCount; ++i)
            {
                result.Data.Add(RangeRandomizer.RandomInt(0, maxValue));
            }

            return result;
        }
    }
}
