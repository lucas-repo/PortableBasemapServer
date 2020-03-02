using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Vishcious.ArcGIS.SLContrib
{
    public class DbfField
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName
        {
            get;
            set;
        }

        /// <summary>
        /// 字段类型
        /// </summary>
        public byte FieldType
        {
            get;
            set;
        }


        public uint FieldDataAddress
        {
            get;
            set;
        }

        public byte FieldLengthInBytes
        {
            get;
            set;
        }

        public byte NumberOfDecimalPlaces
        {
            get;
            set;
        }

        public byte FieldFlags
        {
            get;
            set;
        }

        public uint NextAutoIncrementValue
        {
            get;
            set;
        }

        public byte AutoIncrementStepValue
        {
            get;
            set;
        }

        public byte[] ReservedBytes
        {
            get;
            set;
        }
    }
}
