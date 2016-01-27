using System;
using Citect.Ampla.Framework;
using Citect.Ampla.Runtime.Data.Samples;
using Citect.Ampla.Runtime.Data.Streams;
using Citect.Ampla.StandardItems;

namespace Code.TimeCalculations
{
    /// <summary>
    ///     Fluent interface for configuring OperatingTime Variables
    /// </summary>
    public interface IFluentOperatingTime
    {
        IFluentOperatingTime UsingCondition(ISampleStream conditionStream);
        IFluentOperatingTime StoreStatusIn(StoredVariable storedVariable);
        IFluentOperatingTime UpdateEvery(ISampleStream timerStream);
        IFluentOperatingTime ResetAfter(TimeSpan resetAfter);
        IFluentOperatingTime TotalHours();
        IFluentOperatingTime TotalDays();
        IFluentOperatingTime TotalMinutes();
        Sample GetSample(Project project, DateTime time);
    }

    public class OperatingTimeVariable : IFluentOperatingTime
    {
        public OperatingTimeVariable(CalculatedVariable host)
        {
            this.host = host;
            resetAfterPeriod = TimeSpan.Zero;
        }

        private ISampleStream conditionStream;
        private CalculationUnits calculationUnits = CalculationUnits.Seconds;
        private TimeSpan resetAfterPeriod;
        private readonly CalculatedVariable host;
        private StoredVariable statusVariable;

        public IFluentOperatingTime UsingCondition(ISampleStream usingConditionStream)
        {
            conditionStream = usingConditionStream;
            return this;
        }

        public IFluentOperatingTime StoreStatusIn(StoredVariable storedVariable)
        {
            statusVariable = storedVariable;
            return this;
        }

        public IFluentOperatingTime UpdateEvery(ISampleStream timerStream)
        {
            // Expression timerStream is used to register the dependency
            return this;
        }

        public IFluentOperatingTime ResetAfter(TimeSpan resetAfter)
        {
            resetAfterPeriod = resetAfter;
            return this;
        }

        public IFluentOperatingTime TotalHours()
        {
            calculationUnits = CalculationUnits.Hours;
            return this;
        }

        public IFluentOperatingTime TotalDays()
        {
            calculationUnits = CalculationUnits.Days;
            return this;
        }

        public IFluentOperatingTime TotalMinutes()
        {
            calculationUnits = CalculationUnits.Minutes;
            return this;
        }

        public Sample GetSample(Project project, DateTime time)
        {
            IOperatingTimeCalculator calculator = new OperatingTimeCalculator(host);
            calculator.SetConditionStream(conditionStream);
            calculator.SetStatusVariable(statusVariable);
            calculator.SetResetDuration(resetAfterPeriod);
            calculator.SetCalculationUnits(calculationUnits);
            calculator.Calculate(time);
            return calculator.GetSample(time);
        }
    }
}