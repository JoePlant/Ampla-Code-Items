using Code.Records;

namespace Code.Custom
{
    /// <summary>
    ///     Record Updater that will calculate the log of the source field.
    /// </summary>
    /// <example>
    ///     // handle the record changed event
    ///     new Code.Custom.LogFieldRecordUpdater("Source Field", "Log Field").HandleRecordChanged(eventItem, eventArgs);
    /// </example>
    public class LogFieldRecordUpdater : RecordUpdater
    {
        private readonly string logResultField;
        private readonly string sourceField;

        public LogFieldRecordUpdater(string sourceField, string logResultField)
        {
            this.sourceField = sourceField;
            this.logResultField = logResultField;
        }
        protected override void OnUpdateNewRecord(IRecord record)
        {
        }

        /// <summary>
        /// Called when the record is to be updated
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void OnUpdateRecord(IRecord record)
        {
            double source = record.GetFieldValue<double>(sourceField, 0);
            if (source > 0)
            {
                double log = System.Math.Log10(source);
                record.SetFieldValue<double>(logResultField, log);
            }
            else
            {
                TraceError("{0} - Unable to calculate ['{1}'] = Math.Log10({2})  ({2} = {3})", this, logResultField, sourceField, source);
            }
        }
    }
}