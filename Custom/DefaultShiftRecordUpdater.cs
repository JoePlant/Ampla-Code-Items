using System;
using Code.Records;

namespace Code.Custom
{
    /// <summary>
    ///     Updates the Shift field for new records 
    /// </summary>
    /// <example>
    ///     // handle the record changing event
    ///     new Code.Records.Custom.DefaultShiftRecordUpdater().HandleRecordChanging(eventItem, eventArgs);
    /// </example>
    public class DefaultShiftRecordUpdater : RecordUpdater
    {
        private static readonly TimeSpan MorningShift = TimeSpan.FromHours(6);
        private static readonly TimeSpan EveningShift = TimeSpan.FromHours(18);

        /// <summary>
        /// Called when the record is a new record.
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void OnUpdateNewRecord(IRecord record)
        {
            string currentValue = record.GetFieldValue<string>("Shift", null);

            if (string.IsNullOrEmpty(currentValue))
            {
                DateTime utcSample = record.GetFieldValue<DateTime>("SampleDateTime", DateTime.UtcNow);
                TimeSpan time = utcSample.ToLocalTime().TimeOfDay;

                string shift = time >= MorningShift && time < EveningShift ? "Day" : "Night";
                record.SetFieldValue("Shift", shift);
            }
        }

        /// <summary>
        /// Called when the record is to be updated
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void OnUpdateRecord(IRecord record)
        {
        }

        public override string ToString()
        {
            return "Default Shift Updater";
        }
    }
}