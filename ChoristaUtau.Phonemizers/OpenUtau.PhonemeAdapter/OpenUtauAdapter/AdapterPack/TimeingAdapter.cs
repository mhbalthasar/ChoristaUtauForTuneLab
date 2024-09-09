using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoristaUtauApi.UPhonemizer.OpenUtauAdapter
{
    public class Timeing125Adapter: TimeAxis
    {
        public override double GetBpmAtTick(int tick)
        {
            return 125.0;//temp=125 means tick=millsecond
        }

        public override double TickPosToMsPos(double tick)
        {
            return tick;
        }

        public override double MsPosToNonExactTickPos(double ms)
        {
            return ms;
        }

        public override int MsPosToTickPos(double ms)
        {
            return (int)Math.Round(ms);
        }

        public override int TicksBetweenMsPos(double msPos, double msEnd)
        {
            return MsPosToTickPos(msEnd) - MsPosToTickPos(msPos);
        }

        public override double MsBetweenTickPos(double tickPos, double tickEnd)
        {
            return TickPosToMsPos(tickEnd) - TickPosToMsPos(tickPos);
        }

        /// <summary>
        /// Convert ms duration to tick at a given reference tick position
        /// </summary>
        /// <param name="durationMs">Duration in ms, positive value means starting from refTickPos, negative value means ending at refTickPos</param>
        /// <param name="refTickPos">Reference tick position</param>
        /// <returns>Duration in ticks</returns>
        public override int MsToTickAt(double offsetMs, int refTickPos)
        {
            return TicksBetweenMsPos(
                TickPosToMsPos(refTickPos),
                TickPosToMsPos(refTickPos) + offsetMs);
        }


        public override UTempo[] TemposBetweenTicks(int start, int end)
        {
            return new UTempo[1] { new UTempo(0,125.0) };
        }

        public override TimeAxis Clone()
        {
            return new Timeing125Adapter();
        }
    }
}
