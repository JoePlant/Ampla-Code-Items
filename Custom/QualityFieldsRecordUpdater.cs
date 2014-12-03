using Code.Records;

namespace Code.Custom
{
    /// <summary>
    ///     Quality Fields Record Updater
    /// </summary>
    /// <example>
    ///    // to handle the record changing event call
    ///    new Code.Records.Custom.QualityFieldsRecordUpdater().HandleRecordChanging(eventItem, eventArgs); 
    ///
    ///    // to handle the record changed event call
    ///    new Code.Records.Custom.QualityFieldsRecordUpdater().HandleRecordChanged(eventItem, eventArgs); 
    /// </example>
    public class QualityFieldsRecordUpdater : RecordUpdater
    {
        /// <summary>
        /// Called when the record is a new record.
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void OnUpdateNewRecord(IRecord record)
        {
        }

        /// <summary>
        /// Called when the record is to be updated
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void OnUpdateRecord(IRecord record)
        {
            int iOne = record.GetFieldValue<int>("One", 0);
            int iTwo = record.GetFieldValue<int>("Two", 0);
            int iThree = record.GetFieldValue<int>("Three", 0);

            record.SetFieldValue<int>("One + Two", iOne + iTwo);
            record.SetFieldValue<int>("One + Three", iOne + iThree);
        }

        public override string ToString()
        {
            return "Update Quality: One, Two, Three";
        }
    }
}