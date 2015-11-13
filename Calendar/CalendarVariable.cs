using System;
using System.Diagnostics;
using Citect.Ampla.Framework;
using Citect.Ampla.General.Server.Calendar;
using Citect.Ampla.Runtime.Data;
using Citect.Ampla.Runtime.Data.Samples;
using Citect.Ampla.Runtime.Data.Streams;

namespace Code.Calendar
{
    /// <summary>
    ///     Fluent interface for configuring Calendar Variables
    /// </summary>
    public interface IFluentCalendar
    {
        IFluentCalendar ForCalendar(string calendarName);
        IFluentCalendar ForItem(Item item);
        IFluentCalendar ForPeriod(TimeSpan period);
        IFluentCalendar UseDefault(double defaultValue);
        IFluentCalendar WithDependency(ISampleStream sampleStream);
        IFluentCalendar WithWarning();
        Sample GetSample(Project project, DateTime time);
    }

    /// <summary>
    ///     Configure a Calendar Variable
    /// </summary>
    public class CalendarVariable : IFluentCalendar
    {
        private string calendarName;
        private double defaultDouble;
        private bool useDefault;
        private bool warnOnNoValue;
        private TimeSpan period;
        private bool useRate;

        /// <summary>
        ///     Use the specified calendar
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <returns></returns>
        public IFluentCalendar ForCalendar(string calendar)
        {
            calendarName = calendar;
            return this;
        }

        /// <summary>
        ///     Use the Specified item's full name as the calendar
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public IFluentCalendar ForItem(Item item)
        {
            calendarName = item.FullName;
            return this;
        }

        /// <summary>
        ///     If the calendar is a rate, then calculate for the specified period
        /// </summary>
        /// <param name="ratePeriod">The rate period.</param>
        /// <returns></returns>
        public IFluentCalendar ForPeriod(TimeSpan ratePeriod)
        {
            period = ratePeriod;
            useRate = true;
            return this;
        }

        /// <summary>
        ///     If no calendar value exists, then use the default value
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public IFluentCalendar UseDefault(double defaultValue)
        {
            defaultDouble = defaultValue;
            useDefault = true;
            return this;
        }

        /// <summary>
        ///     References the stream dependency for triggering calculations
        /// </summary>
        /// <param name="sampleStream">The sample stream.</param>
        /// <returns></returns>
        public IFluentCalendar WithDependency(ISampleStream sampleStream)
        {
            // Expression  sampleStream is used to register the dependency
            return this;
        }

        /// <summary>
        ///     If the calendar is not set then write a warning message when calculating
        /// </summary>
        /// <returns></returns>
        public IFluentCalendar WithWarning()
        {
            warnOnNoValue = true;
            return this;
        }

        /// <summary>
        /// Gets the value from the calendar
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        private object GetValue(Project project, DateTime time)
        {
            // Ampla 5.2 (and earlier)
            /* 
            return useRate
                    ? Citect.Ampla.General.Server.Calendar.CalendarCache.GetValue(calendarName, time, period)
                    : Citect.Ampla.General.Server.Calendar.CalendarCache.GetValue(calendarName, time);
            */
              
            // Ampla 6.0 (and later)
            
            ICalendarCache calendarCache = project.Resolve<ICalendarCache>();
            
            if (calendarCache != null)
            {
                return useRate
                           ? calendarCache.GetValue(calendarName, time, period)
                           : calendarCache.GetValue(calendarName, time);
            }
            
            return null;
        }

        /// <summary>
        ///     Gets a sample from calendar at the specified time
        /// </summary>
        /// <param name="project"></param>
        /// <param name="time">Time for the sample</param>
        /// <returns></returns>
        public Sample GetSample(Project project, DateTime time)
        {
            object value = GetValue(project, time);

            if (value == null || value is DBNull)
            {
                if (warnOnNoValue)
                {
                    Citect.Common.Diagnostics.Write(TraceLevel.Warning, "No calendar value for: '{0}'", calendarName);
                }

                if (useDefault)
                {
                    return new Sample(time, defaultDouble, Quality.GoodLocalOverride);
                }
                return new Sample(time, 0.0D, Quality.BadUninitialized);
            }

            double dValue = Convert.ToDouble(value);
            return new Sample(time, dValue, Quality.Good);
        }
    }
}