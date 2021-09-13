using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextGenerator.Models
{
    public class TextParams
    {
        public int Length { get; set; } = 100;
        public double Temperature { get; set; } = 1.0;
        public int K { get; set; } = 10;
        public double P { get; set; } = 0.9;
        public double RepetitionPenalty { get; set; } = 1.0;
        public int NumReturnSequences { get; set; } = 1;

        public bool paraphrase { get; set; } = false;
        public bool expand { get; set; } = false;

    }
}
