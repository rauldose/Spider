//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by Spider Developer at 2020/9/1 16:29:31.
//  Version 1.0
//  Spider Developer
//==============================================================

using Cdy.Spider.DevelopCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cdy.Spider.ModbusDriver.Develop
{
    /// <summary>
    /// 
    /// </summary>
    public class ModbusDriverDevelopViewModel: ViewModelBase
    {

        #region ... Variables  ...
        static string[] mEightFormates;
        static string[] mFourFormates;
        static string[] mStringEncodings;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        static ModbusDriverDevelopViewModel()
        {
            mEightFormates = Enum.GetNames( typeof(EightValueFormate));
            mFourFormates = Enum.GetNames(typeof(FourValueFormate));
            mStringEncodings = Enum.GetNames(typeof(StringEncoding));
        }

        #endregion ...Constructor...

        #region ... Properties ...
        
        /// <summary>
        /// 
        /// </summary>
        public ModbusIpDriverData Model { get; set; }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public string[] EightFormates
        {
            get
            {
                return mEightFormates;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] FourFormates
        {
            get
            {
                return mFourFormates;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] StringEncodings
        {
            get
            {
                return mStringEncodings;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public int IntFormate
        {
            get
            {
                return (int)Model.IntFormate;
            }
            set
            {
                Model.IntFormate = (FourValueFormate)value;
                OnPropertyChanged("IntFormate");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int FloatFormate
        {
            get
            {
                return (int)Model.FloatFormate;
            }
            set
            {
                Model.FloatFormate = (FourValueFormate)value;
                OnPropertyChanged("FloatFormate");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int LongFormate
        {
            get
            {
                return (int)Model.LongFormate;
            }
            set
            {
                Model.LongFormate = (EightValueFormate)value;
                OnPropertyChanged("LongFormate");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int DoubleFormate
        {
            get
            {
                return (int)Model.DoubleFormate;
            }
            set
            {
                Model.DoubleFormate = (EightValueFormate)value;
                OnPropertyChanged("DoubleFormate");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int StringEncoding
        {
            get
            {
                return (int)Model.StringEncoding;
            }
            set
            {
                Model.StringEncoding = (Spider.StringEncoding)value;
                OnPropertyChanged("StringEncoding");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get
            {
                return Model.Id;
            }
            set
            {
                Model.Id = value;
                OnPropertyChanged("Id");
            }
        }


        /// <summary>
            /// 
            /// </summary>
        public ushort PackageLen
        {
            get
            {
                return Model.PackageLen;
            }
            set
            {
                if (Model.PackageLen != value)
                {
                    Model.PackageLen = value;
                    OnPropertyChanged("PackageLen");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public System.Windows.Visibility ScanCircleVisibility
        {
            get
            {
                return Model.Model == WorkMode.Active ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }


        /// <summary>
            /// 
            /// </summary>
        public int ScanCircle
        {
            get
            {
                return Model.ScanCircle;
            }
            set
            {
                if (Model.ScanCircle != value)
                {
                    Model.ScanCircle = value;
                    OnPropertyChanged("ScanCircle");
                }
            }
        }



        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
