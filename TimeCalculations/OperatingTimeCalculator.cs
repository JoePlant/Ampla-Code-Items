using System;
using System.Diagnostics;
using System.Globalization;
using Citect.Ampla.Framework;
using Citect.Ampla.Runtime.Data;
using Citect.Ampla.Runtime.Data.Samples;
using Citect.Ampla.Runtime.Data.Streams;
using Citect.Ampla.StandardItems;

namespace Code.TimeCalculations
{

    public enum CalculationUnits
    {
        Seconds,
        Minutes,
        Hours,
        Days,
    };

    public interface IOperatingTimeCalculator
    {
        void SetConditionStream(ISampleStream conditionStream);
        void SetStatusVariable(StoredVariable statusVariable);
        void SetResetDuration(TimeSpan resetPeriod);
        void SetCalculationUnits(CalculationUnits calculationUnits);
        void Calculate(DateTime time);
        Sample GetSample(DateTime time);
    }

    /// <summary>
    ///     Operating Time Calculator that uses states to manage the operating time evaluation
    /// </summary>
    public class OperatingTimeCalculator : IOperatingTimeCalculator
    {
        private readonly Item hostItem;
        private ISampleStream conditionStream;
        private StoredVariable statusVariable;
        private TimeSpan resetPeriod;
        private CalculationUnits units;
        private TimeState state;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatingTimeCalculator"/> class.
        /// </summary>
        /// <param name="host">The host item.</param>
        public OperatingTimeCalculator(Item host)
        {
            hostItem = host;
            SetCalculationUnits(CalculationUnits.Seconds);
            SetResetDuration(TimeSpan.Zero);
        }

        /// <summary>
        /// Sets the condition stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void SetConditionStream(ISampleStream stream)
        {
            conditionStream = stream;
        }

        /// <summary>
        /// Sets the string status variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void SetStatusVariable(StoredVariable variable)
        {
            statusVariable = variable;
        }

        /// <summary>
        /// Sets the duration of the reset window
        /// </summary>
        /// <param name="resetTimeSpan">The reset time span.</param>
        public void SetResetDuration(TimeSpan resetTimeSpan)
        {
            resetPeriod = resetTimeSpan;
        }

        /// <summary>
        /// Sets the Time units to calculate the operating time in
        /// </summary>
        /// <param name="calculationUnits">The calculation units.</param>
        public void SetCalculationUnits(CalculationUnits calculationUnits)
        {
            units = calculationUnits;
            switch (units)
            {
                case CalculationUnits.Days:
                    {
                        calculationFunc = span => Math.Max(span.TotalDays, 0.0D);
                        break;
                    }
                case CalculationUnits.Hours:
                    {
                        calculationFunc = span => Math.Max(span.TotalHours, 0.0D);
                        break;
                    }
                case CalculationUnits.Minutes:
                    {
                        calculationFunc = span => Math.Max(span.TotalMinutes, 0.0D);
                        break;
                    }
                default:
                    {
                        calculationFunc = span => Math.Max(span.TotalSeconds, 0.0D);
                        break;
                    }
            }
        }

        /// <summary>
        /// Calculates the operating time for the specified time looking at changes in state.
        /// </summary>
        /// <param name="time">The time.</param>
        public void Calculate(DateTime time)
        {
            if (statusVariable == null)
            {
                TraceError("Unable to Calculate OperatingTimeVariable as no StoredVariable available for storing the status.\r\nAdd .StoreStatusIn(storedVariable) to the Expression.");
            }
            
            if (statusVariable != null
                && statusVariable.SampleTypeCode != SampleTypeCode.String)
            {
                statusVariable.WriteTraceMessage(TraceLevel.Error,
                    "SampleTypeCode must be String for storing the status of the OperatingTimeVariable as defined on {0}",
                                                 hostItem.FullName);
            }
            if (statusVariable != null 
                && statusVariable.UpdateMode != SampleStreamUpdateMode.OnWrite)
            {
                statusVariable.WriteTraceMessage(TraceLevel.Error,
                    "UpdateMode must be OnWrite for storing the status of the OperatingTimeVariable as defined on {0}",
                                                 hostItem.FullName);
            }
            
            if (conditionStream == null)
            {
                TraceError("No Condition is specified to monitor for the OperatingTimeVariable. \r\nAdd UsingCondition(variable.Samples) to the Expression.");
            }
            if (conditionStream != null && conditionStream.SampleTypeCode != SampleTypeCode.Boolean)
            {
                TraceError("Unable to monitor the condition as it is not a Boolean Sample.");
            }

            ISample statusSample = statusVariable != null ? GetLastSample(statusVariable.Samples, time) : null;
            ISample conditionSample = GetLastSample(conditionStream, time);

            string status = GetSampleValue<string>(statusSample);
            state = TimeState.Parse(this, status);

            state.UpdateCondition(conditionSample);
            state.UpdateCurrentTime(time);

            if (statusVariable != null)
            {
                // save the current state
                Sample sample = new StringSample(time, state.GetPersistenceValue(), Quality.Good);
                statusVariable.Samples.Write(sample);
            }
        }

        /// <summary>
        /// Gets the sample representing the operating time 
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public Sample GetSample(DateTime time)
        {
            return state.GetSample(time);
        }

        private void Trace(string format, params object[] args)
        {
            hostItem.WriteTraceMessage(TraceLevel.Verbose, format, args);
        }

        private void TraceError(string format, params object[] args)
        {
            hostItem.WriteTraceMessage(TraceLevel.Error, format, args);
        }

        private static ISample GetLastSample(ISampleStream stream, DateTime time)
        {
            if (stream != null)
            {
                int indexOfResult = stream.IndexOnOrBefore(time);
                if (indexOfResult >= 0)
                {
                    ISample sample = stream[indexOfResult];

                    if (sample.IsGood)
                    {
                        return sample;
                    }
                }
            }
            return null;
        }

        protected static internal T GetSampleValue<T>(ISample sample)
        {
            if (sample != null && sample.IsGood)
            {
                try
                {
                    return (T)Convert.ChangeType(sample.Value, typeof(T));
                }
                catch (Exception)
                {
                }
            }
            return default(T);
        }

        protected internal double Calculate(TimeSpan delta)
        {
            return calculationFunc(delta);
        }

        private Func<TimeSpan, double> calculationFunc;

        /// <summary>
        /// Gets the expiry date for the current state
        /// </summary>
        /// <param name="lastTimeStamp">The last time stamp.</param>
        /// <returns></returns>
        public DateTime GetExpiryDate(DateTime lastTimeStamp)
        {
            return lastTimeStamp.Add(resetPeriod);
        }

        /// <summary>
        /// Changes the state of the operating time calculator
        /// </summary>
        /// <param name="newState">The new state.</param>
        public void ChangeState(TimeState newState)
        {
            string oldState = state.GetStateName();
            Trace("StateChange: {0} -> {1} ({2})", oldState, newState.GetStateName(), newState.PreviousValue);
            state = newState;
        }
    }

    /// <summary>
    ///     Interface used for OperatingTime state calculations
    /// </summary>
    public interface ITimeState
    {
        void UpdateCondition(ISample conditionSample);
        void UpdateCurrentTime(DateTime time);
        Sample GetSample(DateTime time);
        string GetPersistenceValue();
    }

    public abstract class TimeState : ITimeState
    {
        protected TimeState(OperatingTimeCalculator calculator, double prevValue, DateTime timeStamp)
        {
            Calculator = calculator;
            PreviousValue = prevValue;
            LastTimeStamp = timeStamp;
        }

        protected OperatingTimeCalculator Calculator { get; private set; }
        public abstract string GetStateName();
        protected DateTime LastTimeStamp { get; private set; }
        public double PreviousValue { get; private set; }

        /// <summary>
        /// Parses the State for the calculator.
        /// </summary>
        /// <param name="timeCalculator">The time calculator.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static TimeState Parse(OperatingTimeCalculator timeCalculator, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return new ResetState(timeCalculator, DateTime.MinValue);
            }

            string[] parts = status.Split(':');
            if (parts.Length == 3)
            {
                string state = parts[0];
                double prevValue;
                DateTime timestamp = DateTime.MinValue;

                if (!double.TryParse(parts[1], out prevValue))
                {
                    prevValue = 0;
                }
                long ticks;
                if (long.TryParse(parts[2], out ticks))
                {
                    timestamp = new DateTime(ticks, DateTimeKind.Utc);
                }

                switch (state)
                {
                    case "Reset":
                        {
                            return new ResetState(timeCalculator, timestamp);
                        }
                    case "Operating":
                        {
                            return new OperatingState(timeCalculator, prevValue, timestamp);
                        }
                    case "Hold":
                        {
                            return new HoldingState(timeCalculator, prevValue, timestamp);
                        }
                }
            }

            return new ResetState(timeCalculator, DateTime.MinValue);
        }


        private class OperatingState : TimeState
        {
            public OperatingState(OperatingTimeCalculator calculator, double prevValue, DateTime timeStamp)
                : base(calculator, prevValue, timeStamp)
            {
            }

            public override string GetStateName()
            {
                return "Operating";
            }

            protected override double CalculateValue(DateTime time)
            {
                TimeSpan delta = time.Subtract(LastTimeStamp);
                double offsetValue = Calculator.Calculate(delta);

                return PreviousValue + offsetValue;
            }

            protected override void OnFalseCondition(DateTime timeStamp)
            {
                double value = CalculateValue(timeStamp);
                Calculator.ChangeState(new HoldingState(Calculator, value, timeStamp));
            }

            protected override void OnTrueCondition(DateTime timeStamp)
            {
                // no change in state
            }

            protected override void OnUpdateTime(DateTime time)
            {
                // no change in state
            }
        }

        private class ResetState : TimeState
        {
            public ResetState(OperatingTimeCalculator calculator, DateTime timeStamp)
                : base(calculator, 0.0d, timeStamp)
            {
            }

            public override string GetStateName()
            {
                return "Reset";
            }

            protected override double CalculateValue(DateTime time)
            {
                return PreviousValue;
            }

            protected override void OnTrueCondition(DateTime timeStamp)
            {
                Calculator.ChangeState(new OperatingState(Calculator, 0.0D, timeStamp));
            }

            protected override void OnFalseCondition(DateTime timeStamp)
            {
                if (LastTimeStamp == DateTime.MinValue)
                {
                    Calculator.ChangeState(new ResetState(Calculator, timeStamp));
                }
            }

            protected override void OnUpdateTime(DateTime time)
            {
                if (LastTimeStamp == DateTime.MinValue)
                {
                    Calculator.ChangeState(new ResetState(Calculator, time));
                }
            }
        }

        private class HoldingState : TimeState
        {
            public HoldingState(OperatingTimeCalculator calculator, double prevValue, DateTime timeStamp)
                : base(calculator, prevValue, timeStamp)
            {
            }

            public override string GetStateName()
            {
                return "Hold";
            }

            protected override void OnTrueCondition(DateTime timeStamp)
            {
                Calculator.ChangeState(new OperatingState(Calculator, PreviousValue, timeStamp));
            }

            protected override double CalculateValue(DateTime time)
            {
                return PreviousValue;
            }

            protected override void OnFalseCondition(DateTime timeStamp)
            {
                DateTime resetAfter = Calculator.GetExpiryDate(LastTimeStamp);
                if (timeStamp >= resetAfter)
                {
                    Calculator.ChangeState(new ResetState(Calculator, resetAfter));
                }
            }

            protected override void OnUpdateTime(DateTime time)
            {
                DateTime resetAfter = Calculator.GetExpiryDate(LastTimeStamp);
                if (time >= resetAfter)
                {
                    Calculator.ChangeState(new ResetState(Calculator, resetAfter));
                }
            }
        }

        public Sample GetSample(DateTime time)
        {
            double value = CalculateValue(time);
            return new Sample(time, value, Quality.Good);
        }

        protected abstract double CalculateValue(DateTime time);

        public void UpdateCondition(ISample conditionSample)
        {
            Boolean value = OperatingTimeCalculator.GetSampleValue<Boolean>(conditionSample);
            if (conditionSample != null)
            {
                DateTime timeStamp = conditionSample.TimeStamp;
                if (value)
                {
                    OnTrueCondition(timeStamp);
                }
                else
                {
                    OnFalseCondition(timeStamp);
                }
            }
            else
            {
                OnFalseCondition(DateTime.MinValue);
            }
        }

        public void UpdateCurrentTime(DateTime time)
        {
            OnUpdateTime(time);
        }

        protected abstract void OnFalseCondition(DateTime timeStamp);

        protected abstract void OnTrueCondition(DateTime timeStamp);

        protected abstract void OnUpdateTime(DateTime time);

        public string GetPersistenceValue()
        {
            return string.Format("{0}:{1}:{2}", GetStateName(), PreviousValue, LastTimeStamp.Ticks);
        }
    }
}