using ProtoBuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuneLab.Base.Science;
using TuneLab.Extensions.Voices;
using UtaubaseForTuneLab.UProjectGenerator;
using UtauSharpApi.UNote;

namespace UtaubaseForTuneLab.Utils
{
    public class AutoPropertyGetter
    {
        public enum PropertyType
        {
            AttrackPoint,
            ReleasePoint,
            MiddlePoint,
            AttrackBar,
            ReleaseBar,
            MiddleBar,
            FullBar
        }
        public enum ValueSelectType
        {
            Maxmium,
            Minimum,
            Average,
            NearZero,
            FarZero
        }
        public class VirtualNote
        {
            public double StartTime { get; set; } = 0;
            public double EndTime { get; set; } = 0;
            public VirtualNote(ISynthesisNote note)
            {
                StartTime = note.StartTime;
                EndTime= note.EndTime;
            }
            public VirtualNote(ISynthesisData data, URenderNote note)
            {
                StartTime = data.StartTime() + ((note.StartMSec - UtauProject.HeadPreSequenceMillsectionTime) / 1000.0);
                EndTime = data.StartTime() + ((note.EndMSec - UtauProject.HeadPreSequenceMillsectionTime) / 1000.0);
            }
            public VirtualNote(ISynthesisData data, UMidiNote note)
            {
                StartTime = data.StartTime() + ((note.StartMSec - UtauProject.HeadPreSequenceMillsectionTime) / 1000.0);
                EndTime = data.StartTime() + ((note.DurationMSec+note.StartMSec - UtauProject.HeadPreSequenceMillsectionTime) / 1000.0);
            }
            public VirtualNote(double startTime, double endTime)
            {
                StartTime = startTime;
                EndTime = endTime;
            }
            public VirtualNote(ISynthesisData data, double startRelTime, double endRelTime)
            {
                StartTime = data.StartTime() + ((startRelTime * 1000.0 - UtauProject.HeadPreSequenceMillsectionTime) / 1000.0);
                EndTime = data.StartTime() + ((endRelTime * 1000.0 - UtauProject.HeadPreSequenceMillsectionTime) / 1000.0);
            }

            public Tuple<double,double> GetTimeArea(PropertyType spaceType = PropertyType.AttrackBar)
            {
                double barDuration = 60;
                double timeStart;
                double timeEnd;
                switch (spaceType)
                {
                    case PropertyType.AttrackPoint:
                        timeStart = StartTime;
                        timeEnd = timeStart;
                        break;
                    case PropertyType.ReleasePoint:
                        timeStart = EndTime;
                        timeEnd = timeStart;
                        break;
                    case PropertyType.MiddlePoint:
                        timeStart = (EndTime + StartTime) / 2;
                        timeEnd = timeStart;
                        break;
                    case PropertyType.FullBar:

                            timeStart = StartTime;
                            timeEnd = EndTime;
                        break;
                    case PropertyType.MiddleBar:
                        {

                            double timeMidNote = (StartTime + EndTime) / 2;
                            double hfDur = Math.Min((EndTime - StartTime) / 2, barDuration / 1000.0);
                            timeEnd = timeMidNote + hfDur / 2;
                            timeStart = timeMidNote - hfDur / 2;
                        }
                        break;
                    case PropertyType.ReleaseBar:
                        {
                            timeEnd = EndTime;
                            double timeMidNote = (StartTime + EndTime) / 2;
                            timeStart = Math.Max(timeMidNote, EndTime - (barDuration / 1000.0));
                        }
                        break;
                    case PropertyType.AttrackBar:
                    default:
                        {
                            timeStart = StartTime;
                            double timeMidNote = (StartTime + EndTime) / 2;
                            timeEnd = Math.Min(timeMidNote, StartTime + (barDuration / 1000.0));
                        }
                        break;
                }
                return new Tuple<double, double>(timeStart, timeEnd);
            }
        }

        double defaultValue = 0;
        ISynthesisData data;
        IAutomationValueGetter? automation;
        double minValue, maxValue;
        public AutoPropertyGetter(ISynthesisData data, string automationID, double minValue, double maxValue, double defaultValue = 0)
        {
            this.data = data;
            data.GetAutomation(automationID, out var automation);
            this.automation = automation;
            this.defaultValue = defaultValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
        public double[] GetAllPointValue(double[] times)
        {
            if (automation == null) return [];
            return automation.GetValue(times);
        }
        public double GetOnePointValue(double time)
        {
            if (automation == null) return defaultValue;
            return automation.GetValue([time])[0];
        }

        public double GetNoteBarValue(VirtualNote note,PropertyType spaceType=PropertyType.AttrackBar, ValueSelectType selectType= ValueSelectType.Maxmium)
        {   
            double timeStart;
            double timeEnd;
            var area = note.GetTimeArea(spaceType);
            switch (spaceType)
            {
                case PropertyType.AttrackPoint:
                    return GetOnePointValue(note.StartTime);
                case PropertyType.ReleasePoint:
                    return GetOnePointValue(note.EndTime);
                case PropertyType.MiddlePoint:
                    return GetOnePointValue((note.EndTime + note.StartTime) / 2);
                default:
                    timeStart = area.Item1;
                    timeEnd = area.Item2;
                    break;
            }

            var times = Enumerable.Range((int)Math.Round(timeStart*1000.0), (int)Math.Round((timeEnd-timeStart) * 1000.0)).Select(p => ((double)p/1000.0)).ToArray();
            if (times.Length == 0) times = [timeStart];
            var result = GetAllPointValue(times).Where(p => p >= double.MinValue && p <= double.MaxValue).ToArray();
            if (result.Length == 0) return defaultValue;
            double max = result.Max();
            double min = result.Min();
            double avg = result.Average();
            switch(selectType)
            {
                case ValueSelectType.NearZero:
                    return result.OrderBy(Math.Abs).FirstOrDefault(defaultValue);
                case ValueSelectType.FarZero:
                    return result.OrderByDescending(Math.Abs).FirstOrDefault(defaultValue);
                case ValueSelectType.Average:
                    return avg;
                case ValueSelectType.Minimum:
                    return min;
                case ValueSelectType.Maxmium:
                default:
                    return max;
            }
        }
    }
}
