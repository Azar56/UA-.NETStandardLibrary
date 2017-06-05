using Opc.Ua.Client;
using Opc.Ua.SampleClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Opc.Ua.SampleClient
{
    public class Util
    {
        static public void ComboBoxNodeId_DropDownOpened(object sender, object e)
        {
            var cb              = sender as ComboBox;
            var itemModelIndex  = (ItemModel)cb.Tag;

            itemModelIndex.ObjNodeId = cb.Items[0];
            cb.Items.Clear();
            foreach (Grid grid in itemModelIndex.NodeIdItems)
            {
                cb.Items.Add(grid);
            }
            cb.SelectedIndex = -1;
        }

        static public void ComboBoxNodeId_DropDownClosed(object sender, object e)
        {
            ComboBox cb = sender as ComboBox;
            var itemModelIndex = (ItemModel)cb.Tag;
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;

            cb.Items.Clear();
            cb.Items.Add(itemModelIndex.ObjNodeId);
            cb.SelectedIndex = 0;
        }

        static public void ComboBoxValue_DropDownOpened(object sender, object e)
        {
            ComboBox comboBox       = sender as ComboBox;
            var itemModelIndex      = (ItemModel)(comboBox.Tag);
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;

            lock (uaClientItems.results)
            {
                uaClientItems.comboBox = comboBox;
                uaClientItems.comboBox.Padding = new Thickness(1);

                itemModelIndex.ObjValue = uaClientItems.comboBox.Items[0];
                uaClientItems.comboBox.Items.RemoveAt(0);
                ((ListView)(uaClientItems.comboBox.Items[0])).DataContext = itemModelIndex.Changes;
                if (itemModelIndex.ArrayItems == null)
                {
                    itemModelIndex.ArrayItems = new List<Grid>();
                    if (itemModelIndex.AccessLevel.IndexOf("W") != -1 && itemModelIndex.BuiltInType != BuiltInType.ExtensionObject) itemModelIndex.ArrayItems.Add(generateGridButton(itemModelIndex, WriteButton_Click, 100, "Write", 320, ""));
                }

                foreach (Grid grid in itemModelIndex.ArrayItems)
                {
                    uaClientItems.comboBox.Items.Add(grid);
                }
                uaClientItems.comboBox.SelectedIndex = -1;
            }
        }

        static public void ComboBoxValue_DropDownClosed(object sender, object e)
        {
            ComboBox cb         = sender as ComboBox;
            var itemModelIndex  = (ItemModel)cb.Tag;
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;
            var temp            = cb.Items[0];
            cb.Items.Clear();

            lock (uaClientItems.results)
            {
                if (uaClientItems.m_result != null)
                {
                    Util.ReleaseContinuationPoints(itemModelIndex);
                }
            }

            cb.Items.Add(itemModelIndex.ObjValue);
            ((ListView)(temp)).DataContext = null;
            cb.Items.Add(temp);
            cb.SelectedIndex               = 0;
        }

        static public string generateBuildInfo(String prefix, BuildInfo buildInfo, List<Grid> arrayItems)
        {
            String ret = "{";
            if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, prefix + "BuildDate", buildInfo.BuildDate.ToString()));
            ret += buildInfo.BuildDate.ToString() + "|";
            if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, prefix + "BuildNumber", buildInfo.BuildNumber.ToString()));
            ret += buildInfo.BuildNumber.ToString() + "|";
            if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, prefix + "ManufacturerName", buildInfo.ManufacturerName.ToString()));
            ret += buildInfo.ManufacturerName.ToString() + "|";
            if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, prefix + "ProductName", buildInfo.ProductName.ToString()));
            ret += buildInfo.ProductName.ToString() + "|";
            if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, prefix + "ProductUri", buildInfo.ProductUri.ToString()));
            ret += buildInfo.ProductUri.ToString() + "|";
            if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, prefix + "SoftwareVersion", buildInfo.SoftwareVersion.ToString()));
            ret += buildInfo.SoftwareVersion.ToString() + "}";
            return ret;
        }

        static public Grid generateGrid(int leftWidth, int rightWidth, string leftValue, string rightValue)
        {
            Grid grid = new Grid();
            grid.Height = 25;
            grid.Padding = new Thickness(0, 0, 10, 0);
            var column1 = new ColumnDefinition();
            column1.MaxWidth = leftWidth;
            grid.ColumnDefinitions.Add(column1);
            var column2 = new ColumnDefinition();
            column2.MaxWidth = rightWidth;
            grid.ColumnDefinitions.Add(column2);

            TextBlock tb = new TextBlock();
            tb.Margin = new Thickness(0, 1, 0, 0);
            tb.Padding = new Thickness(0.0);
            tb.Height = 23;
            tb.Text = leftValue + " : ";
            tb.FontSize = 12;
            tb.TextAlignment = TextAlignment.Right;
            tb.SetValue(Grid.ColumnProperty, 0);
            grid.Children.Add(tb);

            TextBox tb2 = new TextBox();
            tb2.BorderThickness = new Thickness(0.0);
            tb2.Padding = new Thickness(0.0);
            tb2.Height = 23;
            tb2.Text = rightValue;
            tb2.FontSize = 12;
            tb2.TextAlignment = TextAlignment.Right;
            tb2.IsReadOnly = true;
            tb2.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(tb2);

            return grid;
        }

        static public Grid generateGridButton(ItemModel itemModelIndex, RoutedEventHandler leftRoutedEventHandler, int leftWidth, string leftButtonValue, int rightWidth, string rightValue, RoutedEventHandler rightRoutedEventHandler = null)
        {
            Grid grid       = new Grid();
            grid.Height     = 35;
            grid.Padding    = new Thickness(0, 0, 10, 0);
            var column1     = new ColumnDefinition();
            column1.MaxWidth = leftWidth;
            grid.ColumnDefinitions.Add(column1);
            var column2     = new ColumnDefinition();
            column2.MaxWidth = rightWidth;
            grid.ColumnDefinitions.Add(column2);

            Button tb       = new Button();
            tb.Margin       = new Thickness(1);
            tb.Padding      = new Thickness(0.0);
            tb.Width        = leftWidth;
            tb.Height       = 33;
            tb.Content      = leftButtonValue;
            tb.FontSize     = 12;
            tb.Click        += leftRoutedEventHandler;
            tb.SetValue(Grid.ColumnProperty, 0);
            grid.Children.Add(tb);

            if (rightRoutedEventHandler == null)
            {
                TextBox tb2     = new TextBox();
                tb2.BorderThickness = new Thickness(0.0);
                tb2.Padding     = new Thickness(0.0);
                tb2.Height      = 33;
                tb2.Text        = rightValue;
                tb2.FontSize    = 12;
                tb2.Tag         = itemModelIndex;
                tb2.TextAlignment = TextAlignment.Right;
                tb2.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(tb2);
                tb.Tag      = tb2;
            }
            else
            {
                tb.Foreground   = ItemModel.solidColorBrushRed;
                Button tb2      = new Button();
                tb2.IsEnabled   = false;
                tb2.Margin      = new Thickness(1);
                tb2.Padding     = new Thickness(0.0);
                tb2.Width       = rightWidth;
                tb2.Height      = 33;
                tb2.Content     = rightValue;
                tb2.FontSize    = 12;
                tb2.Tag         = itemModelIndex;
                tb2.Click       += rightRoutedEventHandler;
                tb2.SetValue(Grid.ColumnProperty, 1);
                grid.Children.Add(tb2);
                tb.Tag          = tb2;
            }

            return grid;
        }

        static public Grid generateGridValue(int i, string value)
        {
            return generateGrid(30, 380, i.ToString(), value);
        }

        static public Grid generateGridNodeId(string attributeName, string value)
        {
            return generateGrid(100, 310, attributeName, value);
        }

        static public void HistoryReadNext_Click(object sender, RoutedEventArgs e)
        {
            var btn             = sender as Button;
            var tb2             = btn.Tag as Button;
            var itemModelIndex  = (ItemModel)tb2.Tag;
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;
            lock (uaClientItems.results)
            {
                var temp = itemModelIndex.Changes;

                ((ListView)(uaClientItems.comboBox.Items[0])).DataContext = null;
                var temp2 = uaClientItems.comboBox.Items[0];
                uaClientItems.comboBox.Items.Clear();
                uaClientItems.comboBox.Items.Add(temp2);

                try
                {
                    //          Clear old changes
                    ItemModel temp3 = itemModelIndex.Changes[0];
                    itemModelIndex.Changes.Clear();
                    itemModelIndex.Changes.Add(temp3);

                    Util.ReadRaw(itemModelIndex);
                }
                catch
                {
                    itemModelIndex.Changes = (List<ItemModel>)temp;
                }

                ((ListView)(uaClientItems.comboBox.Items[0])).DataContext = itemModelIndex.Changes;
                foreach (Grid grid in itemModelIndex.ArrayItems)
                {
                    uaClientItems.comboBox.Items.Add(grid);
                }
                uaClientItems.comboBox.SelectedIndex = -1;
                uaClientItems.comboBox.Focus(FocusState.Programmatic);
                uaClientItems.comboBox.UpdateLayout();
                if (uaClientItems.comboBox.Items.Count > 1 && ((Grid)(uaClientItems.comboBox.Items[1])).Children.Count > 1)
                {
                    ((Button)(((Grid)(uaClientItems.comboBox.Items[1])).Children[0])).Foreground = ItemModel.solidColorBrushBlack;
                    ((Button)(((Grid)(uaClientItems.comboBox.Items[1])).Children[1])).IsEnabled = true;
                }
            }
        }

        static public void HistoryRelease_Click(object sender, RoutedEventArgs e)
        {
            var btn             = sender as Button;
            var itemModelIndex  = (ItemModel)btn.Tag;
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;

            lock (uaClientItems.results)
            {
                var temp = itemModelIndex.Changes;
                if (uaClientItems.m_result != null)
                {

                    // ((ListView)(uaClientItems.comboBox.Items[0])).DataContext = null;
                    var temp2 = uaClientItems.comboBox.Items[0];
                    uaClientItems.comboBox.Items.Clear();
                    uaClientItems.comboBox.Items.Add(temp2);

                    try
                    {
                        //          Clear old changes
                        ItemModel temp3 = itemModelIndex.Changes[0];
                        itemModelIndex.Changes.Clear();
                        itemModelIndex.Changes.Add(temp3);

                        Util.ReleaseContinuationPoints(itemModelIndex);
                    }
                    catch
                    {
                        itemModelIndex.Changes = (List<ItemModel>)temp;
                    }

                    // ((ListView)(uaClientItems.comboBox.Items[0])).DataContext = itemModelIndex.Changes;
                    foreach (Grid grid in itemModelIndex.ArrayItems)
                    {
                        uaClientItems.comboBox.Items.Add(grid);
                    }
                    uaClientItems.comboBox.SelectedIndex = -1;
                    uaClientItems.comboBox.Focus(FocusState.Programmatic);
                    uaClientItems.comboBox.UpdateLayout();
                    if (uaClientItems.comboBox.Items.Count > 1 && ((Grid)(uaClientItems.comboBox.Items[1])).Children.Count > 1)
                    {
                        ((Button)(((Grid)(uaClientItems.comboBox.Items[1])).Children[0])).Foreground = ItemModel.solidColorBrushBlack;
                        ((Button)(((Grid)(uaClientItems.comboBox.Items[1])).Children[1])).IsEnabled = false;
                    }
                }
            }
        }

        static public string getValue(DataValue dataValue, List<Grid> arrayItems = null)
        {
            string value = String.Empty;
            Object obj = dataValue.Value;
            if (obj is Array)
            {
                var array = obj as Array;
                if (array.Length > 0)
                {
                    value = "{";
                    for (int i = 0; i < array.Length; i++)
                    {
                        var eobj = array.GetValue(i) as Opc.Ua.ExtensionObject;
                        if (eobj == null)
                        {
                            LocalizedText lt = array.GetValue(i) as LocalizedText;
                            if (lt != null)
                            {
                                String str = "\"" + lt.Locale + "\",\"" + lt.Text + "\"";
                                if (arrayItems != null) arrayItems.Add(generateGridValue(i, str));
                                value += str + "|";
                            }
                            else
                            {
                                if (arrayItems != null) arrayItems.Add(generateGridValue(i, array.GetValue(i).ToString()));
                                value += array.GetValue(i).ToString() + "|";
                            }
                        }
                        else
                        {
                            bool useValue = false;
                            var temp = specialTypes(eobj, arrayItems, ref useValue);
                            if (useValue)
                            {
                                value += temp + "|";
                            }
                            else
                            {
                                value += array.GetValue(i).ToString() + "|";
                            }
                        }
                    }
                    value = value.Substring(0, value.Length - 1);
                    value += "}";
                }
                else value = dataValue.Value.ToString();
            }
            else
            {
                value = dataValue.Value.ToString();
                var eobj = dataValue.Value as Opc.Ua.ExtensionObject;
                if (eobj == null)
                {
                    LocalizedText lt = dataValue.Value as LocalizedText;
                    if (lt != null)
                    {
                        String str = "\"" + lt.Locale + "\",\"" + lt.Text + "\"";
                        if (arrayItems != null) arrayItems.Add(generateGridValue(0, str));
                    }
                    else
                    {
                        if (arrayItems != null) arrayItems.Add(generateGridValue(0, value));
                    }
                }
                else
                {
                    bool useValue = false;
                    var temp = specialTypes(eobj, arrayItems, ref useValue);
                    if (useValue)
                    {
                        value = temp;
                    }
                }
            }
            return value;
        }

        static public void ReadRaw(ItemModel itemModelIndex, bool isReadModified = true)
        {
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;

            ReadRawModifiedDetails details = new ReadRawModifiedDetails();
            details.StartTime = DateTime.UtcNow.AddHours(-1.0);
            details.EndTime = DateTime.UtcNow.AddDays(1);
            details.IsReadModified = false;
            details.NumValuesPerNode = uaClientItems.SizeHistoricItems;
            details.ReturnBounds = false;


            HistoryReadValueId nodeToRead = new HistoryReadValueId();
            nodeToRead.NodeId = itemModelIndex.NodeId;

            if (uaClientItems.m_result != null)
            {
                nodeToRead.ContinuationPoint = uaClientItems.m_result.ContinuationPoint;
            }

            HistoryReadValueIdCollection nodesToRead = new HistoryReadValueIdCollection();
            nodesToRead.Add(nodeToRead);

            HistoryReadResultCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            uaClientItems.m_session.HistoryRead(
                null,
                new ExtensionObject(details),
                TimestampsToReturn.Source,
                false,
                nodesToRead,
                out results,
                out diagnosticInfos);

            Session.ValidateResponse(results, nodesToRead);
            Session.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            if (StatusCode.IsBad(results[0].StatusCode))
            {
                throw new ServiceResultException(results[0].StatusCode);
            }

            HistoryData data = ExtensionObject.ToEncodeable(results[0].HistoryData) as HistoryData;

            for (int i = 0; i < data.DataValues.Count; i++)
            {
                ItemModel itemModel = new ItemModel(uaClientItems);
                itemModel.setAttributes(data.DataValues[i], getValue(data.DataValues[i]));
                itemModelIndex.AddChange(uaClientItems.SizeHistoricItems, itemModel);
            }

            uaClientItems.m_result = results[0];
        }

        static public void ReleaseContinuationPoints(ItemModel itemModelIndex)
        {
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;

            ReadRawModifiedDetails details = new ReadRawModifiedDetails();

            HistoryReadValueId nodeToRead = new HistoryReadValueId();
            nodeToRead.NodeId = itemModelIndex.NodeId;

            if (uaClientItems.m_result != null)
            {
                nodeToRead.ContinuationPoint = uaClientItems.m_result.ContinuationPoint;
            }

            HistoryReadValueIdCollection nodesToRead = new HistoryReadValueIdCollection();
            nodesToRead.Add(nodeToRead);

            HistoryReadResultCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            uaClientItems.m_session.HistoryRead(
                null,
                new ExtensionObject(details),
                TimestampsToReturn.Source,
                true,
                nodesToRead,
                out results,
                out diagnosticInfos);

            Session.ValidateResponse(results, nodesToRead);
            Session.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            uaClientItems.m_result = null;
        }

        static public DateTime ReadFirstDate(ItemModel itemModelIndex)
        {
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;

            ReadRawModifiedDetails details = new ReadRawModifiedDetails();
            details.StartTime = new DateTime(1970, 1, 1);
            details.EndTime = DateTime.UtcNow.AddDays(1);
            details.IsReadModified = false;
            details.NumValuesPerNode = uaClientItems.SizeHistoricItems;
            details.ReturnBounds = false;

            HistoryReadValueId nodeToRead = new HistoryReadValueId();
            nodeToRead.NodeId = itemModelIndex.NodeId;

            HistoryReadValueIdCollection nodesToRead = new HistoryReadValueIdCollection();
            nodesToRead.Add(nodeToRead);

            HistoryReadResultCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            uaClientItems.m_session.HistoryRead(
                null,
                new ExtensionObject(details),
                TimestampsToReturn.Source,
                false,
                nodesToRead,
                out results,
                out diagnosticInfos);

            Session.ValidateResponse(results, nodesToRead);
            Session.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            if (results == null)
            {
                return DateTime.MinValue;
            }

            if (StatusCode.IsBad(results[0].StatusCode))
            {
                return DateTime.MinValue;
            }

            HistoryData data = ExtensionObject.ToEncodeable(results[0].HistoryData) as HistoryData;
            DateTime startTime = DateTime.MinValue;

            if (data != null)
            {
                startTime = data.DataValues[0].SourceTimestamp;
                for (int i = 0; i < data.DataValues.Count; i++)
                {
                    ItemModel itemModel = new ItemModel(uaClientItems);
                    itemModel.setAttributes(data.DataValues[i], getValue(data.DataValues[i]));
                    itemModelIndex.AddChange(uaClientItems.SizeHistoricItems, itemModel);
                }
            }

            if (results[0].ContinuationPoint != null)
            {
                nodeToRead.ContinuationPoint = results[0].ContinuationPoint;

                uaClientItems.m_session.HistoryRead(
                    null,
                    new ExtensionObject(details),
                    TimestampsToReturn.Source,
                    true,
                    nodesToRead,
                    out results,
                    out diagnosticInfos);

                Session.ValidateResponse(results, nodesToRead);
                Session.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);
            }

            return startTime;
        }

        static public string specialTypes(Opc.Ua.ExtensionObject eobj, List<Grid> arrayItems, ref bool useValue)
        {
            bool found = false;
            String ret = String.Empty;
            var type = eobj.Body.GetType();
            useValue = true;
            if (type.Name == "Byte[]")
            {
                Array arr = eobj.Body as Array;
                ret += "{";
                for (int i = 0; i < arr.Length; i++)
                {
                    ret += arr.GetValue(i).ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGridValue(i, arr.GetValue(i).ToString()));
                }
                ret = ret.Substring(0, ret.Length - 1);
                ret += "}";
                found = true;
                useValue = true;
            }

            BuildInfo buildInfo = eobj.Body as BuildInfo;
            if (!found && buildInfo != null)
            {
                ret += generateBuildInfo("", buildInfo, arrayItems);
                found = true;
            }

            var t = eobj.Body.GetType();
            if (!found)
            {
                ServerStatusDataType serverStatusDataType = eobj.Body as ServerStatusDataType;
                if (serverStatusDataType != null)
                {
                    ret = "{";
                    if (serverStatusDataType.BuildInfo is BuildInfo)
                    {
                        buildInfo = serverStatusDataType.BuildInfo as BuildInfo;
                        ret += generateBuildInfo("BuildInfo.", buildInfo, arrayItems) + "|";
                    }
                    else
                    {
                        if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "BuildInfo", serverStatusDataType.BuildInfo.ToString()));
                        ret += serverStatusDataType.BuildInfo.ToString() + "|";
                    }
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "CurrentTime", serverStatusDataType.CurrentTime.ToString()));
                    ret += serverStatusDataType.CurrentTime.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "SecondsTillShutdown", serverStatusDataType.SecondsTillShutdown.ToString()));
                    ret += serverStatusDataType.SecondsTillShutdown.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "ShutdownReason", serverStatusDataType.ShutdownReason.ToString()));
                    ret += serverStatusDataType.ShutdownReason.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "StartTime", serverStatusDataType.StartTime.ToString()));
                    ret += serverStatusDataType.StartTime.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "State", serverStatusDataType.State.ToString()));
                    ret += serverStatusDataType.State.ToString() + "}";
                    found = true;
                }
            }

            if (!found)
            {
                SubscriptionDiagnosticsDataType subscriptionDiagnosticsDataType = eobj.Body as SubscriptionDiagnosticsDataType;
                if (subscriptionDiagnosticsDataType != null)
                {
                    ret = "{";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "CurrentKeepAliveCount", subscriptionDiagnosticsDataType.CurrentKeepAliveCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.CurrentKeepAliveCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "CurrentLifetimeCount", subscriptionDiagnosticsDataType.CurrentLifetimeCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.CurrentLifetimeCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "DataChangeNotificationsCount", subscriptionDiagnosticsDataType.DataChangeNotificationsCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.DataChangeNotificationsCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "DisableCount", subscriptionDiagnosticsDataType.DisableCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.DisableCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "DisabledMonitoredItemCount", subscriptionDiagnosticsDataType.DisabledMonitoredItemCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.DisabledMonitoredItemCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "DiscardedMessageCount", subscriptionDiagnosticsDataType.DiscardedMessageCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.DiscardedMessageCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "EnableCount", subscriptionDiagnosticsDataType.EnableCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.EnableCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "EventNotificationsCount", subscriptionDiagnosticsDataType.EventNotificationsCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.EventNotificationsCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "EventQueueOverFlowCount", subscriptionDiagnosticsDataType.EventQueueOverFlowCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.EventQueueOverFlowCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "LatePublishRequestCount", subscriptionDiagnosticsDataType.LatePublishRequestCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.LatePublishRequestCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "MaxKeepAliveCount", subscriptionDiagnosticsDataType.MaxKeepAliveCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.MaxKeepAliveCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "MaxLifetimeCount", subscriptionDiagnosticsDataType.MaxLifetimeCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.MaxLifetimeCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "MaxNotificationsPerPublish", subscriptionDiagnosticsDataType.MaxNotificationsPerPublish.ToString()));
                    ret += subscriptionDiagnosticsDataType.MaxNotificationsPerPublish.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "ModifyCount", subscriptionDiagnosticsDataType.ModifyCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.ModifyCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "MonitoredItemCount", subscriptionDiagnosticsDataType.MonitoredItemCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.MonitoredItemCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "MonitoringQueueOverflowCount", subscriptionDiagnosticsDataType.MonitoringQueueOverflowCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.MonitoringQueueOverflowCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "NextSequenceNumber", subscriptionDiagnosticsDataType.NextSequenceNumber.ToString()));
                    ret += subscriptionDiagnosticsDataType.NextSequenceNumber.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "NotificationsCount", subscriptionDiagnosticsDataType.NotificationsCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.NotificationsCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "Priority", subscriptionDiagnosticsDataType.Priority.ToString()));
                    ret += subscriptionDiagnosticsDataType.Priority.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "PublishingEnabled", subscriptionDiagnosticsDataType.PublishingEnabled.ToString()));
                    ret += subscriptionDiagnosticsDataType.PublishingEnabled.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "PublishingInterval", subscriptionDiagnosticsDataType.PublishingInterval.ToString()));
                    ret += subscriptionDiagnosticsDataType.PublishingInterval.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "PublishRequestCount", subscriptionDiagnosticsDataType.PublishRequestCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.PublishRequestCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "RepublishMessageCount", subscriptionDiagnosticsDataType.RepublishMessageCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.RepublishMessageCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "RepublishMessageRequestCount", subscriptionDiagnosticsDataType.RepublishMessageRequestCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.RepublishMessageRequestCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "RepublishRequestCount", subscriptionDiagnosticsDataType.RepublishRequestCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.RepublishRequestCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "SessionId", subscriptionDiagnosticsDataType.SessionId.ToString()));
                    ret += subscriptionDiagnosticsDataType.SessionId.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "SubscriptionId", subscriptionDiagnosticsDataType.SubscriptionId.ToString()));
                    ret += subscriptionDiagnosticsDataType.SubscriptionId.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "TransferredToAltClientCount", subscriptionDiagnosticsDataType.TransferredToAltClientCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.TransferredToAltClientCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "TransferredToSameClientCount", subscriptionDiagnosticsDataType.TransferredToSameClientCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.TransferredToSameClientCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "TransferRequestCount", subscriptionDiagnosticsDataType.TransferRequestCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.TransferredToSameClientCount.ToString() + "|";
                    if (arrayItems != null) arrayItems.Add(Util.generateGrid(180, 230, "UnacknowledgedMessageCount", subscriptionDiagnosticsDataType.UnacknowledgedMessageCount.ToString()));
                    ret += subscriptionDiagnosticsDataType.UnacknowledgedMessageCount.ToString() + "}";
                    found = true;
                }
            }
            return ret;
        }

        static public void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tb = btn.Tag as TextBox;
            var itemModelIndex = (ItemModel)tb.Tag;
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;

            writeValue(tb.Text, itemModelIndex);
            uaClientItems.comboBox.IsDropDownOpen = false;
        }

        static private void writeValue(String str, ItemModel itemModelIndex)
        {
            bool valueOK = false;
            UaClientItems uaClientItems = itemModelIndex.UaClientItems;

            WriteValueCollection wvc = new WriteValueCollection();
            WriteValue wv = new WriteValue();
            wv.Value = new DataValue();
            wv.AttributeId = Attributes.Value;
            String[] parts;

            String s = str.Replace("{", "").Replace("}", "");
            parts = s.Split('|');
            String type = itemModelIndex.Type;
            bool isArray = false;
            if (type.Contains("[]")) isArray = true;

            switch (itemModelIndex.BuiltInType)
            {
                case BuiltInType.Boolean:
                    if (!isArray)
                    {
                        bool bl = false;
                        if (bool.TryParse(str, out bl)) valueOK = true;
                        wv.Value.Value = bl;
                    }
                    else
                    {
                        valueOK = true;
                        bool[] boolArr = new bool[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            bool ba = false;
                            if (!bool.TryParse(parts[j], out ba)) valueOK = false;
                            boolArr[j] = ba;
                        }
                        wv.Value.Value = boolArr;
                    }
                    break;
                case BuiltInType.SByte:
                    if (!isArray)
                    {
                        sbyte sb = 0;
                        if (sbyte.TryParse(str, out sb)) valueOK = true;
                        wv.Value.Value = sb;
                    }
                    else
                    {
                        valueOK = true;
                        sbyte[] sbArr = new sbyte[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            sbyte ba = 0;
                            if (!sbyte.TryParse(parts[j], out ba)) valueOK = false;
                            sbArr[j] = ba;
                        }
                        wv.Value.Value = sbArr;
                    }
                    break;
                case BuiltInType.Byte:
                    if (!isArray)
                    {
                        byte b = 0;
                        if (byte.TryParse(str, out b)) valueOK = true;
                        wv.Value.Value = b;
                    }
                    else
                    {
                        valueOK = true;
                        byte[] bArr = new byte[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            byte ba = 0;
                            if (!byte.TryParse(parts[j], out ba)) valueOK = false;
                            bArr[j] = ba;
                        }
                        wv.Value.Value = bArr;
                    }
                    break;
                case BuiltInType.Int16:
                    if (!isArray)
                    {

                        short sh = 0;
                        if (short.TryParse(str, out sh)) valueOK = true;
                        wv.Value.Value = sh;
                    }
                    else
                    {
                        valueOK = true;
                        short[] sbArr = new short[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            short ba = 0;
                            if (!short.TryParse(parts[j], out ba)) valueOK = false;
                            sbArr[j] = ba;
                        }
                        wv.Value.Value = sbArr;
                    }
                    break;
                case BuiltInType.UInt16:
                    if (!isArray)
                    {

                        ushort us = 0;
                        if (ushort.TryParse(str, out us)) valueOK = true;
                        wv.Value.Value = us;
                    }
                    else
                    {
                        valueOK = true;
                        ushort[] usArr = new ushort[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            ushort ba = 0;
                            if (!ushort.TryParse(parts[j], out ba)) valueOK = false;
                            usArr[j] = ba;
                        }
                        wv.Value.Value = usArr;
                    }
                    break;
                case BuiltInType.Int32:
                    if (!isArray)
                    {
                        int i = 0;
                        if (int.TryParse(str, out i)) valueOK = true;
                        wv.Value.Value = i;
                    }
                    else
                    {
                        valueOK = true;
                        int[] iArr = new int[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            int ba = 0;
                            if (!int.TryParse(parts[j], out ba)) valueOK = false;
                            iArr[j] = ba;
                        }
                        wv.Value.Value = iArr;
                    }
                    break;
                case BuiltInType.UInt32:
                    if (!isArray)
                    {
                        uint ui = 0;
                        if (uint.TryParse(str, out ui)) valueOK = true;
                        wv.Value.Value = ui;
                    }
                    else
                    {
                        valueOK = true;
                        uint[] uiArr = new uint[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            uint ba = 0;
                            if (!uint.TryParse(parts[j], out ba)) valueOK = false;
                            uiArr[j] = ba;
                        }
                        wv.Value.Value = uiArr;
                    }
                    break;
                case BuiltInType.Int64:
                    if (!isArray)
                    {
                        long l = 0;
                        if (long.TryParse(str, out l)) valueOK = true;
                        wv.Value.Value = l;
                    }
                    else
                    {
                        valueOK = true;
                        long[] lArr = new long[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            long ba = 0;
                            if (!long.TryParse(parts[j], out ba)) valueOK = false;
                            lArr[j] = ba;
                        }
                        wv.Value.Value = lArr;
                    }
                    break;
                case BuiltInType.UInt64:
                    if (!isArray)
                    {
                        ulong ul = 0;
                        if (ulong.TryParse(str, out ul)) valueOK = true;
                        wv.Value.Value = ul;
                    }
                    else
                    {
                        valueOK = true;
                        ulong[] ulArr = new ulong[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            ulong ba = 0;
                            if (!ulong.TryParse(parts[j], out ba)) valueOK = false;
                            ulArr[j] = ba;
                        }
                        wv.Value.Value = ulArr;
                    }
                    break;
                case BuiltInType.Float:
                    if (!isArray)
                    {
                        float f = 0;
                        if (float.TryParse(str, out f)) valueOK = true;
                        wv.Value.Value = f;
                    }
                    else
                    {
                        valueOK = true;
                        float[] fArr = new float[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            float ba = 0;
                            if (!float.TryParse(parts[j], out ba)) valueOK = false;
                            fArr[j] = ba;
                        }
                        wv.Value.Value = fArr;
                    }
                    break;
                case BuiltInType.Double:
                    if (!isArray)
                    {
                        double d = 0;
                        if (double.TryParse(str, out d)) valueOK = true;
                        wv.Value.Value = d;
                    }
                    else
                    {
                        valueOK = true;
                        double[] dArr = new double[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            double ba = 0;
                            if (!double.TryParse(parts[j], out ba)) valueOK = false;
                            dArr[j] = ba;
                        }
                        wv.Value.Value = dArr;
                    }
                    break;
                case BuiltInType.String:
                    if (!isArray)
                    {
                        valueOK = true;
                        wv.Value.Value = str;
                    }
                    else
                    {
                        valueOK = true;
                        string[] stArr = new string[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            stArr[j] = parts[j];
                        }
                        wv.Value.Value = stArr;
                    }
                    break;
                case BuiltInType.DateTime:
                    if (!isArray)
                    {

                        DateTime dt;
                        if (DateTime.TryParse(str, out dt)) valueOK = true;
                        wv.Value.Value = dt;
                    }
                    else
                    {
                        valueOK = true;
                        DateTime[] dtArr = new DateTime[parts.Length];
                        for (int j = 0; j < parts.Length; j++)
                        {
                            DateTime ba;
                            if (!DateTime.TryParse(parts[j], out ba)) valueOK = false;
                            dtArr[j] = ba;
                        }
                        wv.Value.Value = dtArr;
                    }
                    break;
                case BuiltInType.ByteString:
                    valueOK = true;
                    byte[] barr = new byte[parts.Length];
                    for (int j = 0; j < parts.Length; j++)
                    {
                        byte ba = 0;
                        if (!byte.TryParse(parts[j], out ba)) valueOK = false;
                        barr[j] = ba;
                    }
                    wv.Value.Value = barr;
                    break;
            }

            if (valueOK)
            {
                wv.NodeId = itemModelIndex.NodeId;
                wvc.Add(wv);
                StatusCodeCollection statusCodeCollection;
                DiagnosticInfoCollection diagnosticInfoCollection;
                var responseHeader = uaClientItems.m_session.Write(null, wvc, out statusCodeCollection, out diagnosticInfoCollection);
                String writeResult = "Write result is: ";
                if (statusCodeCollection.Count > 0) writeResult += statusCodeCollection[0].ToString() + " !";
                var dialog = new Windows.UI.Popups.MessageDialog(writeResult);
                var result = dialog.ShowAsync();
            }
        }
    }
}
