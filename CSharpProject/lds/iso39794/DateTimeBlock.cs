using System;
using System.Collections.Generic;
using System.Linq;

namespace org.jmrtd.lds.iso39794
{
    public class DateTimeBlock : Block
    {
        private int year;
        private int month;
        private int day;
        private int hour;
        private int minute;
        private int second;
        private int millisecond;

        public DateTimeBlock(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            this.millisecond = millisecond;
        }

        internal DateTimeBlock(object asn1Encodable)
        {
            // TODO: Implement ASN1Util when ASN1 support is added
            // var taggedObjects = ASN1Util.DecodeTaggedObjects(asn1Encodable);
            // this.year = ASN1Util.DecodeInt(taggedObjects[0]);
            // this.month = taggedObjects.ContainsKey(1) ? ASN1Util.DecodeInt(taggedObjects[1]) : -1;
            // this.day = taggedObjects.ContainsKey(2) ? ASN1Util.DecodeInt(taggedObjects[2]) : -1;
            // this.hour = taggedObjects.ContainsKey(3) ? ASN1Util.DecodeInt(taggedObjects[3]) : -1;
            // this.minute = taggedObjects.ContainsKey(4) ? ASN1Util.DecodeInt(taggedObjects[4]) : -1;
            // this.second = taggedObjects.ContainsKey(5) ? ASN1Util.DecodeInt(taggedObjects[5]) : -1;
            // this.millisecond = taggedObjects.ContainsKey(6) ? ASN1Util.DecodeInt(taggedObjects[6]) : -1;
        }

        public int GetYear() => year;
        public int GetMonth() => month;
        public int GetDay() => day;
        public int GetHour() => hour;
        public int GetMinute() => minute;
        public int GetSecond() => second;
        public int GetMillisecond() => millisecond;

        public override int GetHashCode()
        {
            return HashCode.Combine(day, hour, millisecond, minute, month, second, year);
        }

        public override bool Equals(object? obj)
        {
            if (obj == this) return true;
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;

            var other = (DateTimeBlock)obj;
            return year == other.year && month == other.month && day == other.day && 
                   hour == other.hour && minute == other.minute && second == other.second && 
                   millisecond == other.millisecond;
        }

        public override string ToString()
        {
            return $"DateTimeBlock [year: {year}, month: {month}, day: {day}, hour: {hour}, minute: {minute}, second: {second}, millisecond: {millisecond}]";
        }

        public override byte[] GetEncoded()
        {
            // TODO: Implement when ASN1 support is added
            return Array.Empty<byte>();
        }

        internal override object GetASN1Object()
        {
            // TODO: Implement ASN1Util when ASN1 support is added
            // var taggedObjects = new Dictionary<int, object>();
            // taggedObjects[0] = ASN1Util.EncodeInt(year);
            // if (month >= 0) taggedObjects[1] = ASN1Util.EncodeInt(month);
            // if (day >= 0) taggedObjects[2] = ASN1Util.EncodeInt(day);
            // if (hour >= 0) taggedObjects[3] = ASN1Util.EncodeInt(hour);
            // if (minute >= 0) taggedObjects[4] = ASN1Util.EncodeInt(minute);
            // if (second >= 0) taggedObjects[5] = ASN1Util.EncodeInt(second);
            // if (millisecond >= 0) taggedObjects[6] = ASN1Util.EncodeInt(millisecond);
            // return ASN1Util.EncodeTaggedObjects(taggedObjects);
            return new object();
        }
    }
}
