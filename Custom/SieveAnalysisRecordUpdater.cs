using System.Collections.Generic;
using Code.Records;

namespace Code.Custom
{
    public class SieveAnalysisRecordUpdater : RecordUpdater
    {
        private readonly string[] coarseMeasurements = new [] {"+16mm", "+12mm", "+10mm", "+08mm", "+06mm", "+04mm", "+2-8mm", "-2-8mm"};
        private readonly string[] mediumMeasurements = new[] { "+6mm", "+4mm", "+2-8mm", "+2mm", "+1-18mm", "+0-85mm", "-0-85mm"};
        private readonly string[] fineMeasurements = new[] { "+2-8mm", "+2mm", "+1-18mm", "+0-85mm", "+0-30mm", "+0-15mm", "-0-15mm" };
        private readonly string[] ballmillMeasurements = new[] { "+0-85mm", "+0-30mm", "+0-15mm", "+0-075mm", "+0-037mm", "-0-037mm" };

        private readonly string[] coarseButtMeasurements = new[] { "+8mm", "+6mm", "+4mm", "+2-8mm", "+2mm", "-2mm"};
        private readonly string[] fineButtMeasurements = new[] { "+2-8mm", "+2mm", "+1-18mm", "+0-85mm", "+0-30mm", "+0-15mm", "+0-075mm", "+0-037mm", "-0-037mm" };

        protected override void OnUpdateNewRecord(IRecord record)
        {
            // no special updates required for new records.
        }

        protected override void OnUpdateRecord(IRecord record)
        {
            UpdateWeightAndPercentage(record, coarseMeasurements, "Coarse Coke Weight ({0})", "Coarse Coke Percentage ({0})");
            UpdateWeightAndPercentage(record, mediumMeasurements, "Medium Coke Weight ({0})", "Medium Coke Percentage ({0})");
            UpdateWeightAndPercentage(record, fineMeasurements, "Fine Coke Weight ({0})", "Fine Coke Percentage ({0})");
            UpdateWeightAndPercentage(record, ballmillMeasurements, "Ball Mill Product Weight ({0})", "Ball Mill Product Percentage ({0})");
            UpdateWeightAndPercentage(record, coarseButtMeasurements, "Coarse Butt Weight ({0})", "Coarse Butt Percentage ({0})");
            UpdateWeightAndPercentage(record, fineButtMeasurements, "Fine Butt Weight ({0})", "Fine Butt Percentage ({0})");
        }

        private static void UpdateWeightAndPercentage(IRecord record, string[] measurements, string weightFormat, string percentageFormat)
        {
            using (RecordAggregate aggregate = new RecordAggregate(record))
            {
                string[] weightFieldNames = ExpandFieldNames(weightFormat, measurements);

                double total = aggregate.SumValues(0, weightFieldNames);

                record.SetFieldValue(ExpandFieldName(weightFormat, "Total"), total);

                double runningTotal = 0;
                foreach (string measurement in measurements)
                {
                    string weightField = ExpandFieldName(weightFormat, measurement);
                    string percentageField = ExpandFieldName(percentageFormat, measurement);
                    double ratio = aggregate.GetRatio(weightField, total);
                    runningTotal += ratio;
                    record.SetFieldValue(percentageField, ratio*100);
                }

                record.SetFieldValue(ExpandFieldName(percentageFormat, "Total"), runningTotal);
            }
        }

        private static string[] ExpandFieldNames(string format, IEnumerable<string> parts)
        {
            List<string> fieldNames = new List<string>();
            foreach (var part in parts)
            {
                fieldNames.Add(ExpandFieldName(format, part));
            }
            return fieldNames.ToArray();
        }

        private static string ExpandFieldName(string format, string part)
        {
            return string.Format(format, part);
        }
    }
}