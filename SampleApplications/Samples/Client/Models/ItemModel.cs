// ------------------------------------------------------------------------------
//  Copyright (c) 2015 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// ------------------------------------------------------------------------------

namespace Opc.Ua.SampleClient.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public class ItemModel : INotifyPropertyChanged
    {

        public static SolidColorBrush solidColorBrushRed   = new SolidColorBrush(Colors.Red);
        public static SolidColorBrush solidColorBrushBlue  = new SolidColorBrush(Colors.Blue);
        public static SolidColorBrush solidColorBrushBlack = new SolidColorBrush(Colors.Black);

        public ItemModel(UaClientItems uaClientItems)
        {
            this.UaClientItems = uaClientItems;
        }

        public ItemModel(UaClientItems uaClientItems, ItemModel itemModel)
        {
            this.UaClientItems      = uaClientItems;
            this.Value              = itemModel.Value;
            this.statusCodeStr      = itemModel.statusCodeStr;
            this.Timestamp          = itemModel.Timestamp;
            this.SolidColorBrush    = itemModel.SolidColorBrush;
        }

        private string accessLevel;
        public string AccessLevel
        {
            get
            {
                return this.accessLevel;
            }
            set
            {
                this.accessLevel = value;
                OnPropertyChanged("AccessLevel");
            }
        }

        private List<Grid> arrayItems;
        public List<Grid> ArrayItems
        {
            get
            {
                return this.arrayItems;
            }
            set
            {
                this.arrayItems = value;
                OnPropertyChanged("ArrayItems");
            }
        }

        private BuiltInType builtInType = 0;
        public BuiltInType BuiltInType
        {
            get
            {
                return this.builtInType;
            }
            set
            {
                this.builtInType = value;
                OnPropertyChanged("BuiltInType");
            }
        }

        private List<ItemModel> changes;
        public List<ItemModel> Changes
        {
            get
            {
                return this.changes;
            }
            set
            {
                this.changes = value;
                OnPropertyChanged("Changes");
            }
        }

        private uint clientHandle;
        public uint ClientHandle
        {
            get
            {
                return this.clientHandle;
            }
            set
            {
                this.clientHandle = value;
                OnPropertyChanged("ClientHandle");
            }
        }

        private string displayName = string.Empty;
        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
            set
            {
                this.displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        public Visibility headerVisibility
        {
            get;
            set;
        }

        public Visibility restVisibility
        {
            get;
            set;
        }

        private ItemModel itemModelIndex;
        public ItemModel ItemModelIndex
        {
            get
            {
                return this.itemModelIndex;
            }
            set
            {
                this.itemModelIndex = value;
                OnPropertyChanged("ItemModelIndex");
            }
        }

        private List<Grid> nodeIdItems;
        public List<Grid> NodeIdItems
        {
            get
            {
                return this.nodeIdItems;
            }
            set
            {
                this.nodeIdItems = value;
                OnPropertyChanged("NodeIdItems");
            }
        }

        private NodeId nodeId;
        public NodeId NodeId
        {
            get
            {
                return this.nodeId;
            }
            set
            {
                this.nodeId = value;
                OnPropertyChanged("NodeId");
            }
        }

        private String nodeIdStr;
        public String NodeIdStr
        {
            get
            {
                return this.nodeIdStr;
            }
            set
            {
                this.nodeIdStr = value;
                OnPropertyChanged("NodeIdStr");
            }
        }

        private object objNodeId;
        public object ObjNodeId
        {
            get
            {
                return this.objNodeId;
            }
            set
            {
                this.objNodeId = value;
                OnPropertyChanged("ObjNodeId");
            }
        }

        private object objValue;
        public object ObjValue
        {
            get
            {
                return this.objValue;
            }
            set
            {
                this.objValue = value;
                OnPropertyChanged("ObjValue");
            }
        }

        private SolidColorBrush solidColorBrush;
        public SolidColorBrush SolidColorBrush
        {
            get
            {
                return this.solidColorBrush;
            }
            set
            {
                this.solidColorBrush = value;
                OnPropertyChanged("SolidColorBrush");
            }
        }


        private String statusCodeStr;
        public String StatusCodeStr
        {
            get
            {
                return this.statusCodeStr;
            }
            set
            {
                this.statusCodeStr = value;
                OnPropertyChanged("StatusCodeStr");
            }
        }

        private String timestamp;
        public String Timestamp
        {
            get
            {
                return this.timestamp;
            }
            set
            {
                this.timestamp = value;
                OnPropertyChanged("Timestamp");
            }
        }

        private string type = string.Empty;
        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
                OnPropertyChanged("Type");
            }
        }

        private UaClientItems uaClientItems;
        public UaClientItems UaClientItems
        {
            get
            {
                return this.uaClientItems;
            }
            set
            {
                this.uaClientItems = value;
                OnPropertyChanged("UaClientItems");
            }
        }

        private string value = string.Empty;
        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        public void AddChange(uint SizeHistoricItems, ItemModel itemModel)
        {
            if (this.Changes.Count < SizeHistoricItems + 1) this.Changes.Add(null);
            for( int i = this.Changes.Count - 2; i > 0 ; i--)
            {
                this.Changes[i + 1] = this.Changes[i];
            }
            this.Changes[1] = new ItemModel( uaClientItems, itemModel);
        }

        public void setAttributes(DataValue dataValue, String value)
        {
            if (StatusCode.IsUncertain(dataValue.StatusCode)) SolidColorBrush   = solidColorBrushBlack;
            if (StatusCode.IsBad(dataValue.StatusCode))       SolidColorBrush   = solidColorBrushRed;
            if (StatusCode.IsGood(dataValue.StatusCode))      SolidColorBrush   = solidColorBrushBlue;
            StatusCodeStr = dataValue.StatusCode.ToString();
            Timestamp = dataValue.SourceTimestamp.ToUniversalTime().ToString("yyyy-MM-dd hh:mm:ss.fff");
            Value = value;
        }

        //INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
